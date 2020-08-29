// <copyright file="AspectProcessor.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// Process aspects.
    /// </summary>
    public static class AspectProcessor
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo> ProxyMemberMappings =
            new ConcurrentDictionary<Type, PropertyInfo>();

        private static readonly ConcurrentDictionary<MethodInfo, IAdvice?> AdviceMappings =
            new ConcurrentDictionary<MethodInfo, IAdvice?>();

        /// <summary>
        /// Processes advices for the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="proxy">The proxy object for interception.</param>
        [DebuggerStepThrough]
        public static void Process(this IInvocationInfo invocation, object? proxy = default)
        {
            var invocationTarget = invocation.Target;
            var proxyMember = ProxyMemberMappings.GetOrAdd(invocationTarget.GetType(), GetProxyMember);
            if (proxyMember != null && proxyMember.GetValue(invocationTarget) == null)
            {
                proxyMember.SetValue(invocationTarget, proxy);
            }

            var advice = AdviceMappings.GetOrAdd(invocation.Method, GetFirstOrDefaultAdvice);
            if (advice == null)
            {
                invocation.Proceed();
            }
            else
            {
                advice.Apply(invocation);
            }
        }

        /// <summary>
        /// Determines whether a type is the target of aspects.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is the target of aspects; otherwise, <c>false</c>.</returns>
        public static bool IsAspectTarget(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return methods.Any(m => m.GetCustomAttributes<AdviceAttribute>(false).Any());
        }

        /// <summary>
        /// Determines whether a method is a pointcut that advices should be applied.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <returns><c>true</c> if the method has any advice; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        public static bool IsPointcut(this MethodInfo method) =>
            method.GetCustomAttributes<AdviceAttribute>(false).Any()
                || method.GetCustomAttribute<AdviceTypesAttribute>(false) != null;

        private static PropertyInfo GetProxyMember(this Type type)
        {
            return type
                .GetProperties(BindingFlags.Instance)
                .FirstOrDefault(p => p.GetCustomAttribute<InterceptionProxyAttribute>(true) != null);
        }

        [DebuggerStepThrough]
        private static IAdvice? GetFirstOrDefaultAdvice(this MethodInfo method)
        {
            var adviceAttributes = method.GetCustomAttributes<AdviceAttribute>(false).OrderBy(a => a.Order).ToArray();
            var adviceTypesAttribute = method.GetCustomAttribute<AdviceTypesAttribute>(false);
            if (adviceAttributes.Length > 0)
            {
                if (adviceTypesAttribute != null)
                {
                    throw new NotSupportedException(
                        $"'{nameof(AdviceAttribute)}' and '{nameof(AdviceTypesAttribute)}' are mutually exclusive.");
                }

                var advices = new List<AdviceAttribute>();
                foreach (var adviceAttribute in adviceAttributes)
                {
                    AddAdvice(advices, adviceAttribute);
                }

                return advices[0];
            }

            return adviceTypesAttribute?.Advices[0];
        }

        [DebuggerStepThrough]
        private static void AddAdvice(IList<AdviceAttribute> advices, AdviceAttribute advice)
        {
            if (advice is AdviceBundleAttribute bundle)
            {
                foreach (var bundledAdvice in bundle.Advices)
                {
                    AddAdvice(advices, bundledAdvice);
                }
            }
            else
            {
                advices.Add(advice);
                var index = advices.Count - 1;
                if (index > 0)
                {
                    advices[index - 1].Next = advice;
                }
            }
        }
    }
}
