// <copyright file="ILGeneratorExtensions.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SidekickNet.Utilities.Reflection
{
    /// <summary>
    /// Extension methods for <see cref="ILGenerator"/>.
    /// </summary>
    public static class ILGeneratorExtensions
    {
        private static readonly OpCode[] LdargOpCodes =
        {
            OpCodes.Ldarg_0,
            OpCodes.Ldarg_1,
            OpCodes.Ldarg_2,
            OpCodes.Ldarg_3,
        };

        private static readonly OpCode[] LdlocOpCodes =
        {
            OpCodes.Ldloc_0,
            OpCodes.Ldloc_1,
            OpCodes.Ldloc_2,
            OpCodes.Ldloc_3,
        };

        private static readonly OpCode[] StlocOpCodes =
        {
            OpCodes.Stloc_0,
            OpCodes.Stloc_1,
            OpCodes.Stloc_2,
            OpCodes.Stloc_3,
        };

        private static readonly MethodInfo GetTypeFromHandleMethod =
            typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// Emits the instruction to load a <see cref="int"/> value.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="number">The <see cref="int"/> value to load.</param>
        public static void EmitLoadInt32(this ILGenerator ilGenerator, int number)
        {
            if (number <= 8)
            {
                var opCode = number switch
                {
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => throw new Exception(),
                };
                ilGenerator.Emit(opCode);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldc_I4, number);
            }
        }

        /// <summary>
        /// Emits the instruction to load a field.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="index">The index of the argument to load.</param>
        public static void EmitLoadArgument(this ILGenerator ilGenerator, int index)
        {
            if (index < LdargOpCodes.Length)
            {
                ilGenerator.Emit(LdargOpCodes[index]);
            }
            else if (index <= 0xff)
            {
                ilGenerator.Emit(OpCodes.Ldarg_S, (byte)index);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldarg, (short)index);
            }
        }

        /// <summary>
        /// Emits the instruction to load a local variable.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="index">The index of the local variable to load.</param>
        public static void EmitLoadLocal(this ILGenerator ilGenerator, int index)
        {
            if (index < LdlocOpCodes.Length)
            {
                ilGenerator.Emit(LdlocOpCodes[index]);
            }
            else if (index <= 0xff)
            {
                ilGenerator.Emit(OpCodes.Ldloc_S, (byte)index);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldloc, (short)index);
            }
        }

        /// <summary>
        /// Emits the instruction to load a local variable.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="local">The <see cref="LocalBuilder"/> that indicates which local variable to load.</param>
        public static void EmitLoadLocal(this ILGenerator ilGenerator, LocalBuilder local)
        {
            ilGenerator.EmitLoadLocal(local.LocalIndex);
        }

        /// <summary>
        /// Emits the instruction to store the value on top of the stack in a local variable.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="index">The index of the local variable.</param>
        public static void EmitStoreLocal(this ILGenerator ilGenerator, int index)
        {
            if (index < StlocOpCodes.Length)
            {
                ilGenerator.Emit(StlocOpCodes[index]);
            }
            else if (index <= 0xff)
            {
                ilGenerator.Emit(OpCodes.Stloc_S, (byte)index);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Stloc, (short)index);
            }
        }

        /// <summary>
        /// Emits the instruction to store the value on top of the stack in a local variable.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="local">The <see cref="LocalBuilder"/> that indicates which local variable to store value in.</param>
        public static void EmitStoreLocal(this ILGenerator ilGenerator, LocalBuilder local)
        {
            ilGenerator.EmitStoreLocal(local.LocalIndex);
        }

        /// <summary>
        /// Emits the instruction to load a field.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="field">The field to load.</param>
        public static void EmitLoadField(this ILGenerator ilGenerator, FieldInfo field)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
        }

        /// <summary>
        /// Emits the instruction to load an array element.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="field">The array field.</param>
        /// <param name="index">The index of the element to load.</param>
        public static void EmitLoadArrayFieldElement(this ILGenerator ilGenerator, FieldInfo field, int index)
        {
            ilGenerator.EmitLoadField(field);
            ilGenerator.EmitLoadInt32(index);
            ilGenerator.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Emits the instruction to create a new zero-based, one-dimensional array.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="elementType">The type of array elements.</param>
        /// <param name="size">The size of the array.</param>
        public static void EmitNewArray(this ILGenerator ilGenerator, Type elementType, int size)
        {
            ilGenerator.EmitLoadInt32(size);
            ilGenerator.Emit(OpCodes.Newarr, elementType);
        }

        /// <summary>
        /// Emits the instruction to replace the array element at a given index with the object ref value.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="index">The index of the array element.</param>
        /// <param name="emitter">Emits the object ref value.</param>
        public static void EmitSetArrayElement(
            this ILGenerator ilGenerator,
            int index,
            Action<ILGenerator, int> emitter)
        {
            ilGenerator.Emit(OpCodes.Dup);
            ilGenerator.EmitLoadInt32(index);
            emitter(ilGenerator, index);
            ilGenerator.Emit(OpCodes.Stelem_Ref);
        }

        /// <summary>
        /// Emits the instruction to create and initialize a new zero-based, one-dimensional array.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="elementType">The type of array elements.</param>
        /// <param name="size">The size of the array.</param>
        /// <param name="emitter">Emits the object ref value for each element.</param>
        public static void EmitNewInitArray(
            this ILGenerator ilGenerator,
            Type elementType,
            int size,
            Action<ILGenerator, int> emitter)
        {
            ilGenerator.EmitNewArray(elementType, size);
            for (var i = 0; i < size; ++i)
            {
                ilGenerator.EmitSetArrayElement(i, emitter);
            }
        }

        /// <summary>
        /// Emits the instruction to load a <see cref="Type"/>.
        /// </summary>
        /// <param name="ilGenerator">The generator of IL instructions.</param>
        /// <param name="type">The <see cref="Type"/> to load.</param>
        public static void EmitLoadType(this ILGenerator ilGenerator, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        }
    }
}
