// <copyright file="InterceptionProxyAttribute.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// Indicates a property is the proxy for interception.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InterceptionProxyAttribute : Attribute
    {
    }
}
