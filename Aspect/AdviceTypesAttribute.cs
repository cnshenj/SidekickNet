// <copyright file="AdviceTypesAttribute.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// A delegate that gets an instance of the specified advice type.
    /// </summary>
    /// <param name="adviceType">The type of the advice to get.</param>
    /// <returns>An instance of the specified advice type.</returns>
    public delegate IAdvice GetAdviceInstance(Type adviceType);

    /// <summary>
    /// The attribute to add multiple advices to program elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AdviceTypesAttribute : Attribute
    {
        private readonly Type[] adviceTypes;

        private IList<IAdvice>? advices;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceTypesAttribute"/> class.
        /// </summary>
        /// <param name="adviceTypes">
        /// The types of advices to apply to program elements.
        /// The advices will be applied in the same order that their types are specified.
        /// </param>
        [DebuggerStepThrough]
        public AdviceTypesAttribute(params Type[] adviceTypes)
        {
            if (adviceTypes == null || adviceTypes.Length == 0)
            {
                throw new ArgumentException($"At least one advice type must be provided.", nameof(adviceTypes));
            }

            if (adviceTypes.Any(t => !typeof(IAdvice).IsAssignableFrom(t) && !typeof(AdviceTypeBundle).IsAssignableFrom(t)))
            {
                throw new ArgumentException(
                    $"Advice type must implement interface '{nameof(IAdvice)}' or inherit from '{nameof(AdviceTypeBundle)}'.",
                    nameof(adviceTypes));
            }

            this.adviceTypes = adviceTypes;
        }

        /// <summary>
        /// Gets or sets a method that gets an instance of an advice type.
        /// For example, a dependency injection container can be used.
        /// </summary>
        public static GetAdviceInstance? GetAdviceInstance { get; set; }

        /// <summary>Gets advices to apply to program elements.</summary>
        public IList<IAdvice> Advices
        {
            get
            {
                // Delay the creation of advices to give time for GetAdvice to be initialized
                if (this.advices == null)
                {
                    if (GetAdviceInstance == null)
                    {
                        throw new InvalidOperationException(
                            $"'{nameof(AdviceTypesAttribute)}'.'{nameof(GetAdviceInstance)}' must be initialized first.");
                    }

                    this.advices = new List<IAdvice>();
                    foreach (var adviceType in this.adviceTypes)
                    {
                        this.AddAdvice(adviceType);
                    }
                }

                return this.advices;
            }
        }

        private void AddAdvice(Type adviceType)
        {
            var advice = GetAdviceInstance!(adviceType);
            if (advice is AdviceTypeBundle bundle)
            {
                foreach (var bundledAdviceType in bundle.AdviceTypes)
                {
                    this.AddAdvice(bundledAdviceType);
                }
            }
            else
            {
                this.advices!.Add(advice);
                var index = this.advices.Count - 1;
                if (index > 0)
                {
                    this.advices[index - 1].Next = advice;
                }
            }
        }
    }
}
