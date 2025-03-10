// <copyright file="ProxyFactory.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using SidekickNet.Utilities.Reflection;

namespace SidekickNet.Aspect.DynamicInheritance
{
    /// <summary>
    /// The factory of proxies via dynamic inheritance.
    /// </summary>
    public class ProxyFactory
    {
        // Creates closed generic methods from definition
        private static readonly MethodInfo MakeGenericMethodMethod =
            typeof(MethodInfo).GetMethod(
                nameof(MethodInfo.MakeGenericMethod),
                BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Method {nameof(MethodInfo)}.{nameof(MethodInfo.MakeGenericMethod)} not found.");

        // Creates a new InvocationInfo object.
        private static readonly ConstructorInfo InvocationInfoConstructor = typeof(InvocationInfo).GetConstructors()[0];

        // Process aspects that are specific to an invocation.
        private static readonly MethodInfo AspectProcessMethod =
            typeof(AspectProcessor).GetMethod(
                nameof(AspectProcessor.Process),
                BindingFlags.Static | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Method {nameof(AspectProcessor)}.{nameof(AspectProcessor.Process)} not found.");

        // The property of return value of an invocation.
        private static readonly PropertyInfo ReturnValueProperty =
            typeof(IInvocationInfo).GetProperty(
                nameof(IInvocationInfo.ReturnValue),
                BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Property {nameof(IInvocationInfo)}.{nameof(IInvocationInfo.ReturnValue)} not found.");

        private readonly AssemblyBuilder assemblyBuilder;

        private readonly ModuleBuilder moduleBuilder;

        private readonly IDictionary<Type, Type> proxyTypes = new Dictionary<Type, Type>();

        private readonly IDictionary<string, int> typeNameConflicts = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFactory"/> class.
        /// </summary>
        /// <param name="assemblyDisplayName">The display name of the dynamic assembly to define subclass types.</param>
        public ProxyFactory(string assemblyDisplayName = "DynamicSubtypes")
        {
            var assemblyName = new AssemblyName(assemblyDisplayName);
            this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            this.moduleBuilder = this.assemblyBuilder.DefineDynamicModule($"{assemblyDisplayName}_Module");
        }

        /// <summary>
        /// Gets a dynamic proxy type for the specified type.
        /// </summary>
        /// <param name="type">The type to get dynamic proxy for.</param>
        /// <returns>The dynamic proxy for the specified type.</returns>
        public Type GetProxyType(Type type)
        {
            if (this.proxyTypes.TryGetValue(type, out var proxyType))
            {
                return proxyType;
            }

            lock (this.proxyTypes)
            {
                // Check one more time in case another thread has created proxy while this thread is waiting for lock
                if (this.proxyTypes.TryGetValue(type, out proxyType))
                {
                    return proxyType;
                }

                this.proxyTypes[type] = proxyType = this.DefineProxyType(type);
                return proxyType;
            }
        }

        private static void DefineProxyConstructors(Type type, TypeBuilder proxyTypeBuilder)
        {
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType);
                var constructorBuilder = proxyTypeBuilder.DefineConstructor(
                    constructor.Attributes,
                    constructor.CallingConvention,
                    parameterTypes.ToArray());
                var ilGenerator = constructorBuilder.GetILGenerator();
                ilGenerator.EmitLoadArgument(0); // First parameter: this

                var parameterCount = constructor.GetParameters().Length;
                for (var i = 0; i < parameterCount; ++i)
                {
                    ilGenerator.EmitLoadArgument(i + 1);
                }

                ilGenerator.Emit(OpCodes.Call, constructor); // Call base constructor
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        private static MethodBuildingData DefineProxyMethod(
            TypeBuilder typeBuilder,
            MethodInfo method,
            string uniqueName)
        {
            if (!method.IsVirtual || method.IsFinal)
            {
                throw new NotSupportedException(
                    $"Method '{method.Name}' cannot be overridden. Add 'virtual' modifier to allow it to be overridden.");
            }

            var proxyBuilder = DefineMethodSignature(typeBuilder, method);
            var ilGenerator = proxyBuilder.GetILGenerator();
            var invocationInfo = ilGenerator.DeclareLocal(typeof(IInvocationInfo));

            ilGenerator.Emit(OpCodes.Ldarg_0); // Parameter: target = this

            // Parameter: method
            var methodField = typeBuilder.DefineField(
                $"{uniqueName}_Method",
                typeof(MethodInfo),
                FieldAttributes.Static | FieldAttributes.Private);
            ilGenerator.Emit(OpCodes.Ldsfld, methodField);
            EmitLoadCallableMethod(ilGenerator, proxyBuilder);

            // Parameter: arguments
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var typeParameterBuilders = proxyBuilder.GetGenericArguments().ToDictionary(a => a.Name);
            if (parameterTypes.Length > 0)
            {
                ilGenerator.EmitNewInitArray(
                    typeof(object),
                    parameterTypes.Length,
                    (g, i) =>
                    {
                        g.EmitLoadArgument(i + 1); // Start from 1 since 0 is "this"
                        var parameterType = parameterTypes[i];
                        if (parameterType.IsGenericMethodParameter)
                        {
                            parameterType = typeParameterBuilders[parameterType.Name];
                        }
                        else if (!parameterType.IsValueType)
                        {
                            return;
                        }

                        g.Emit(OpCodes.Box, parameterType);
                    });
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldnull);
            }

            // Parameter: executor
            var executorBuilder = DefineExecutorMethod(typeBuilder, method, uniqueName);
            var executorField = typeBuilder.DefineField(
                $"{uniqueName}_Executor",
                typeof(MethodInfo),
                FieldAttributes.Static | FieldAttributes.Private);
            ilGenerator.Emit(OpCodes.Ldsfld, executorField);
            EmitLoadCallableMethod(ilGenerator, proxyBuilder);

            // invocationInfo = new InvocationInfo(target, method, arguments)
            ilGenerator.Emit(OpCodes.Newobj, InvocationInfoConstructor);
            ilGenerator.EmitStoreLocal(invocationInfo);
            ilGenerator.EmitLoadLocal(invocationInfo);

            ilGenerator.Emit(OpCodes.Ldnull); // Parameter: proxy = null, because the proxy is the same as the target
            ilGenerator.Emit(OpCodes.Call, AspectProcessMethod); // AspectExtensions.Process(invocationInfo, proxy)

            var returnType = method.ReturnType;
            if (returnType != typeof(void))
            {
                ilGenerator.EmitLoadLocal(invocationInfo);
                ilGenerator.Emit(OpCodes.Callvirt, ReturnValueProperty.GetMethod!); // invocationInfo.ReturnValue
                if (returnType.IsGenericMethodParameter)
                {
                    returnType = typeParameterBuilders[returnType.Name];
                }

                if (returnType.IsValueType || returnType.IsGenericMethodParameter)
                {
                    ilGenerator.Emit(OpCodes.Unbox_Any, returnType);
                }
                else if (returnType != typeof(object))
                {
                    ilGenerator.Emit(OpCodes.Castclass, returnType);
                }
            }

            ilGenerator.Emit(OpCodes.Ret);

            return new MethodBuildingData(methodField.Name, executorBuilder.Name, executorField.Name);
        }

        private static MethodBuilder DefineExecutorMethod(
            TypeBuilder typeBuilder,
            MethodInfo method,
            string uniqueName)
        {
            var methodBuilder = DefineMethodSignature(typeBuilder, method, $"Execute_{uniqueName}", MethodAttributes.Private, true);

            var ilGenerator = methodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0); // this

            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; ++i)
            {
                ilGenerator.EmitLoadArgument(i + 1); // Start from 1 since 0 is "this"
            }

            if (method.IsGenericMethodDefinition)
            {
                method = method.MakeGenericMethod(methodBuilder.GetGenericArguments());
            }

            ilGenerator.Emit(OpCodes.Call, method);
            ilGenerator.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        // The signature for the proxy method and the executor method - exactly same as the original method
        // Both the proxy method and the executor method have the same signature as the original method
        // The proxy method overrides the original method to add pre-processing and post-processing
        // The executor method executes the original method properly, even when the original method is virtual
        private static MethodBuilder DefineMethodSignature(
            TypeBuilder typeBuilder,
            MethodInfo method,
            string? uniqueName = default,
            MethodAttributes? accessFlag = default,
            bool newSlot = false)
        {
            var name = uniqueName ?? method.Name;
            var attributes = method.Attributes;

            if (newSlot)
            {
                if (!attributes.HasFlag(MethodAttributes.NewSlot))
                {
                    attributes |= MethodAttributes.NewSlot;
                }
            }
            else if (attributes.HasFlag(MethodAttributes.NewSlot))
            {
                attributes -= attributes & MethodAttributes.NewSlot;
            }

            if (accessFlag != null)
            {
                if (!attributes.HasFlag(accessFlag))
                {
                    attributes -= attributes & MethodAttributes.MemberAccessMask;
                    attributes |= accessFlag.Value;
                }
            }

            MethodBuilder methodBuilder;
            if (method.IsGenericMethodDefinition)
            {
                methodBuilder = typeBuilder.DefineMethod(
                    name,
                    attributes,
                    method.CallingConvention);
                DefineGenericMethodParameters(method, methodBuilder);
            }
            else
            {
                methodBuilder = typeBuilder.DefineMethod(
                    name,
                    attributes,
                    method.CallingConvention,
                    method.ReturnType,
                    method.GetParameters().Select(p => p.ParameterType).ToArray());
            }

            return methodBuilder;
        }

        private static void DefineGenericMethodParameters(MethodInfo method, MethodBuilder methodBuilder)
        {
            var typeParameters = method.GetGenericArguments();
            var typeParameterBuilders = methodBuilder
                .DefineGenericParameters(typeParameters.Select(tp => tp.Name).ToArray())
                .ToDictionary(b => b.Name);
            SetGenericTypeConstraints(typeParameterBuilders, typeParameters);

            var returnType = method.ReturnType;
            methodBuilder.SetReturnType(
                returnType.IsGenericMethodParameter ? typeParameterBuilders[returnType.Name] : returnType);
            var parameters = method.GetParameters();
            methodBuilder.SetParameters(
                parameters
                    .Select(p => p.ParameterType)
                    .Select(t => t.IsGenericMethodParameter ? typeParameterBuilders[t.Name] : t)
                    .ToArray());
        }

        private static void SetGenericTypeConstraints(
            IDictionary<string, GenericTypeParameterBuilder> builders,
            IEnumerable<Type> genericArguments)
        {
            foreach (var genericArgument in genericArguments)
            {
                var builder = builders[genericArgument.Name];
                builder.SetGenericParameterAttributes(genericArgument.GenericParameterAttributes);

                var typeConstraints = genericArgument.GetGenericParameterConstraints();
                var baseTypeConstraint = typeConstraints.FirstOrDefault(c => !c.IsInterface);
                if (baseTypeConstraint != null)
                {
                    builder.SetBaseTypeConstraint(baseTypeConstraint);
                }

                var interfaceConstraints = typeConstraints.Where(c => c.IsInterface).ToArray();
                if (interfaceConstraints.Length > 0)
                {
                    builder.SetInterfaceConstraints(interfaceConstraints);
                }
            }
        }

        private static string GetUniqueName(string name, IDictionary<string, int> nameConflicts)
        {
            nameConflicts.TryGetValue(name, out var count);
            nameConflicts[name] = ++count;
            if (count > 1)
            {
                name += $"_{count}";
            }

            return name;
        }

        private static void EmitLoadCallableMethod(ILGenerator ilGenerator, MethodInfo caller)
        {
            if (caller.IsGenericMethodDefinition)
            {
                // Make generic method from definition, using caller's generic type parameters
                var genericArguments = caller.GetGenericArguments();
                ilGenerator.EmitNewInitArray(
                    typeof(Type),
                    genericArguments.Length,
                    (g, i) => g.EmitLoadType(genericArguments[i]));
                ilGenerator.Emit(OpCodes.Callvirt, MakeGenericMethodMethod);
            }
        }

        private Type DefineProxyType(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                throw new NotSupportedException($"Cannot define a proxy type for generic type definition '{type.FullName}'.");
            }

            var typeName = this.GetTypeUniqueName(type);
            var proxyTypeBuilder = this.moduleBuilder.DefineType(
                $"{typeName}_DynamicSubtype",
                TypeAttributes.Public | TypeAttributes.Class,
                type);

            DefineProxyConstructors(type, proxyTypeBuilder);

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var targetMethods = methods.Where(m => m.IsPointcut());
            var buildingData = new Dictionary<MethodInfo, MethodBuildingData>();
            var nameConflicts = new Dictionary<string, int>();
            foreach (var method in targetMethods)
            {
                var uniqueName = GetUniqueName(method.Name, nameConflicts);
                var methodBuildingData = DefineProxyMethod(proxyTypeBuilder, method, uniqueName);
                buildingData[method] = methodBuildingData;
            }

            var proxyType = proxyTypeBuilder.CreateType();
            if (proxyType == null)
            {
                throw new InvalidOperationException($"Cannot create dynamic subtype for type '{type.FullName}'.");
            }

            foreach (var (method, data) in buildingData)
            {
                var field = proxyType.GetField(data.MethodFieldName, BindingFlags.Static | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException($"Field {data.MethodFieldName} not found.");
                field.SetValue(null, method);

                field = proxyType.GetField(data.ExecutorFieldName, BindingFlags.Static | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException($"Field {data.ExecutorFieldName} not found.");
                var executor = proxyType.GetMethod(data.ExecutorName, BindingFlags.Instance | BindingFlags.NonPublic);
                field.SetValue(null, executor);
            }

            return proxyType;
        }

        private string GetTypeUniqueName(Type type)
        {
            var typeName = type.FullName ?? type.Name;
            var index = typeName.IndexOf(',');
            if (index > 0)
            {
                typeName = typeName[..index];
            }

            return GetUniqueName(typeName, this.typeNameConflicts);
        }

        private class MethodBuildingData
        {
            public MethodBuildingData(
                string methodFieldName,
                string executorName,
                string executorFieldName)
            {
                this.MethodFieldName = methodFieldName;
                this.ExecutorName = executorName;
                this.ExecutorFieldName = executorFieldName;
            }

            public string MethodFieldName { get; private set; }

            public string ExecutorName { get; private set; }

            public string ExecutorFieldName { get; private set; }
        }
    }
}
