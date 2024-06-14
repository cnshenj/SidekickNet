// <copyright file="ProxyInterception.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using SimpleInjector;
using SimpleInjector.Advanced;

namespace SidekickNet.Aspect.SimpleInjector
{
    /// <summary>
    /// Intercepts injected dependencies by creating proxies.
    /// </summary>
    public static class ProxyInterception
    {
        /// <summary>
        /// Intercepts dependencies that meet conditions defined by the specified predicate.
        /// </summary>
        /// <param name="container">The scope that the dependency registrations belong to.</param>
        /// <param name="predicate">Defines the conditions to match registrations to intercept.</param>
        /// <param name="resolveConstructor">The delegate to resolve constructors.</param>
        public static void Intercept(this Container container, Predicate<Type> predicate, ResolveConstructor resolveConstructor)
        {
            if (!(container.Options.ConstructorResolutionBehavior is UseCustomConstructorBehavior useCustomConstructorBehavior))
            {
                useCustomConstructorBehavior = new UseCustomConstructorBehavior(container.Options.ConstructorResolutionBehavior);
                container.Options.ConstructorResolutionBehavior = useCustomConstructorBehavior;
            }

            useCustomConstructorBehavior.AddResolver(predicate, resolveConstructor);
        }

        /// <summary>
        /// Intercepts dependencies that meet conditions defined by the specified predicate.
        /// </summary>
        /// <param name="container">The scope that the dependency registrations belong to.</param>
        /// <param name="predicate">Defines the conditions to match registrations to intercept.</param>
        /// <param name="createProxyMethod">The method to create proxies.</param>
        public static void Intercept(this Container container, Predicate<Type> predicate, MethodInfo createProxyMethod)
        {
            container.ExpressionBuilt += (_, e) =>
            {
                if (predicate(e.RegisteredServiceType) || predicate(e.Expression.Type))
                {
                    e.Expression = BuildProxyExpression(container, createProxyMethod, e);
                }
            };
        }

        /// <summary>
        /// Intercepts dependencies that meet conditions defined by the specified predicate.
        /// </summary>
        /// <param name="container">The scope that the dependency registrations belong to.</param>
        /// <param name="resolveConstructor">The delegate to resolve constructors.</param>
        public static void InterceptTarget(this Container container, ResolveConstructor resolveConstructor)
        {
            container.Intercept(AspectProcessor.IsAspectTarget, resolveConstructor);
        }

        /// <summary>
        /// Intercepts dependencies that meet conditions defined by the specified predicate.
        /// </summary>
        /// <param name="container">The scope that the dependency registrations belong to.</param>
        /// <param name="createProxyMethod">The method to create proxies.</param>
        public static void InterceptTarget(this Container container, MethodInfo createProxyMethod)
        {
            container.Intercept(AspectProcessor.IsAspectTarget, createProxyMethod);
        }

        /// <summary>
        /// Intercepts services that have the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute service types must have to be intercepted.</typeparam>
        /// <param name="container">The scope that the dependency registrations belong to.</param>
        /// <param name="resolveConstructor">The delegate to resolve constructors.</param>
        public static void InterceptAttribute<TAttribute>(this Container container, ResolveConstructor resolveConstructor)
            where TAttribute : Attribute
        {
            container.Intercept(type => type.GetCustomAttributes<TAttribute>().Any(), resolveConstructor);
        }

        /// <summary>
        /// Intercepts services that have the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute service types must have to be intercepted.</typeparam>
        /// <param name="container">The scope that the dependency registrations belong to.</param>
        /// <param name="createProxyMethod">The method to create proxies.</param>
        public static void InterceptAttribute<TAttribute>(this Container container, MethodInfo createProxyMethod)
            where TAttribute : Attribute
        {
            container.Intercept(type => type.GetCustomAttributes<TAttribute>().Any(), createProxyMethod);
        }

        private static Expression BuildProxyExpression(
            Container container,
            MethodInfo createProxyMethod,
            ExpressionBuiltEventArgs e)
        {
            var targetExpression = e.Expression;
            var proxyExpression = Expression.Convert(
                Expression.Call(
                    createProxyMethod,
                    Expression.Constant(e.RegisteredServiceType, typeof(Type)),
                    Expression.Constant(e.Expression.Type, typeof(Type)),
                    targetExpression),
                e.RegisteredServiceType);

            if (targetExpression is ConstantExpression)
            {
                return Expression.Constant(CreateInstance(proxyExpression), e.RegisteredServiceType);
            }

            return proxyExpression;
        }

        private static object CreateInstance(Expression expression)
        {
            var creator = Expression.Lambda<Func<object>>(expression, []).Compile();
            return creator();
        }

        private class UseCustomConstructorBehavior : IConstructorResolutionBehavior
        {
            private readonly IConstructorResolutionBehavior defaultConstructorResolutionBehavior;

            private readonly ICollection<(Predicate<Type> Predicate, ResolveConstructor Resolver)> resolvers =
                new List<(Predicate<Type>, ResolveConstructor)>();

            public UseCustomConstructorBehavior(IConstructorResolutionBehavior defaultConstructorResolutionBehavior)
            {
                this.defaultConstructorResolutionBehavior = defaultConstructorResolutionBehavior;
            }

            public ConstructorInfo? TryGetConstructor(Type implementationType, out string? errorMessage)
            {
                foreach (var (predicate, resolver) in this.resolvers)
                {
                    if (predicate(implementationType))
                    {
                        try
                        {
                            errorMessage = default;
                            return resolver(implementationType);
                        }
                        catch (Exception ex)
                        {
                            errorMessage = ex.Message;
                            return null;
                        }
                    }
                }

                return this.defaultConstructorResolutionBehavior.TryGetConstructor(implementationType, out errorMessage);
            }

            public void AddResolver(Predicate<Type> predicate, ResolveConstructor resolver)
            {
                this.resolvers.Add((predicate, resolver));
            }
        }
    }
}
