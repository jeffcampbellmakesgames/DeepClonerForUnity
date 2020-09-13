/*

MIT License

Copyright (c) 2020 Jeff Campbell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.Scripting;

namespace JCMG.DeepCopyForUnity
{
	[Preserve]
	internal static class DeepClonerExprGenerator
	{
		private static readonly ConcurrentDictionary<FieldInfo, bool> _readonlyFields =
			new ConcurrentDictionary<FieldInfo, bool>();

		private static FieldInfo _attributesFieldInfo = typeof(FieldInfo).GetPrivateField("m_fieldAttributes");

		internal static object GenerateClonerInternal(Type realType, bool asObject)
		{
			return GenerateProcessMethod(realType, asObject && realType.IsValueType());
		}

		internal static void ForceSetField(FieldInfo field, object obj, object value)
		{
			field.SetValue(obj, value);
		}

		private static object GenerateProcessMethod(Type type, bool unboxStruct)
		{
			if (type.IsArray)
			{
				return GenerateProcessArrayMethod(type);
			}

			if (type.FullName != null && type.FullName.StartsWith("System.Tuple`"))
			{
				// if not safe type it is no guarantee that some type will contain reference to
				// this tuple. In usual way, we're creating new object, setting reference for it
				// and filling data. For tuple, we will fill data before creating object
				// (in constructor arguments)
				var genericArguments = type.GenericArguments();
				// current tuples contain only 8 arguments, but may be in future...
				// we'll write code that works with it
				if (genericArguments.Length < 10 && genericArguments.All(DeepClonerSafeTypes.CanReturnSameObject))
				{
					return GenerateProcessTupleMethod(type);
				}
			}

			var methodType = unboxStruct || type.IsClass() ? typeof(object) : type;

			var expressionList = new List<Expression>();

			var from = Expression.Parameter(methodType);
			var fromLocal = from;
			var toLocal = Expression.Variable(type);
			var state = Expression.Parameter(typeof(DeepCloneState));

			if (!type.IsValueType())
			{
				var methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");

				// to = (T)from.MemberwiseClone()
				expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));

				fromLocal = Expression.Variable(type);
				// fromLocal = (T)from
				expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));

				// added from -> to binding to ensure reference loop handling
				// structs cannot loop here
				// state.AddKnownRef(from, to)
				expressionList.Add(
					Expression.Call(
						state,
						typeof(DeepCloneState).GetMethod("AddKnownRef"),
						from,
						toLocal));
			}
			else
			{
				if (unboxStruct)
				{
					// toLocal = (T)from;
					expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
					fromLocal = Expression.Variable(type);
					// fromLocal = toLocal; // structs, it is ok to copy
					expressionList.Add(Expression.Assign(fromLocal, toLocal));
				}
				else
				{
					// toLocal = from
					expressionList.Add(Expression.Assign(toLocal, from));
				}
			}

			var fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				// don't do anything with this dark magic!
				if (tp == typeof(ContextBoundObject))
				{
					break;
				}

				fi.AddRange(tp.GetDeclaredFields());
				tp = tp.BaseType();
			} while (tp != null);

			foreach (var fieldInfo in fi)
			{
				if (!DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
				{
					var methodInfo = fieldInfo.FieldType.IsValueType()
						? typeof(DeepClonerGenerator).GetStaticMethod("CloneStructInternal")
							.MakeGenericMethod(fieldInfo.FieldType)
						: typeof(DeepClonerGenerator).GetStaticMethod("CloneClassInternal");

					var get = Expression.Field(fromLocal, fieldInfo);

					// toLocal.Field = Clone...Internal(fromLocal.Field)
					var call = (Expression)Expression.Call(methodInfo, get, state);
					if (!fieldInfo.FieldType.IsValueType())
					{
						call = Expression.Convert(call, fieldInfo.FieldType);
					}

					// should handle specially
					// todo: think about optimization, but it rare case
					var isReadonly = _readonlyFields.GetOrAdd(fieldInfo, f => f.IsInitOnly);
					if (isReadonly)
					{
						var setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
						expressionList.Add(
							Expression.Call(
								setMethod,
								Expression.Constant(fieldInfo),
								Expression.Convert(toLocal, typeof(object)),
								Expression.Convert(call, typeof(object))));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
					}
				}
			}

			expressionList.Add(Expression.Convert(toLocal, methodType));

			var funcType = typeof(Func<,,>).MakeGenericType(methodType, typeof(DeepCloneState), methodType);

			var blockParams = new List<ParameterExpression>();
			if (from != fromLocal)
			{
				blockParams.Add(fromLocal);
			}

			blockParams.Add(toLocal);

			return Expression.Lambda(
					funcType,
					Expression.Block(blockParams, expressionList),
					from,
					state)
				.Compile();
		}

		private static object GenerateProcessArrayMethod(Type type)
		{
			var elementType = type.GetElementType();
			var rank = type.GetArrayRank();

			MethodInfo methodInfo;

			// multidim or not zero-based arrays
			if (rank != 1 || type != elementType.MakeArrayType())
			{
				if (rank == 2 && type == elementType.MakeArrayType())
				{
					// small optimization for 2 dim arrays
					methodInfo = typeof(DeepClonerGenerator).GetStaticMethod("Clone2DimArrayInternal")
						.MakeGenericMethod(elementType);
				}
				else
				{
					methodInfo = typeof(DeepClonerGenerator).GetStaticMethod("CloneAbstractArrayInternal");
				}
			}
			else
			{
				var methodName = "Clone1DimArrayClassInternal";
				if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
				{
					methodName = "Clone1DimArraySafeInternal";
				}
				else if (elementType.IsValueType())
				{
					methodName = "Clone1DimArrayStructInternal";
				}

				methodInfo = typeof(DeepClonerGenerator).GetStaticMethod(methodName).MakeGenericMethod(elementType);
			}

			var from = Expression.Parameter(typeof(object));
			var state = Expression.Parameter(typeof(DeepCloneState));
			var call = Expression.Call(methodInfo, Expression.Convert(from, type), state);

			var funcType = typeof(Func<,,>).MakeGenericType(typeof(object), typeof(DeepCloneState), typeof(object));

			return Expression.Lambda(
					funcType,
					call,
					from,
					state)
				.Compile();
		}

		private static object GenerateProcessTupleMethod(Type type)
		{
			var from = Expression.Parameter(typeof(object));
			var state = Expression.Parameter(typeof(DeepCloneState));

			var local = Expression.Variable(type);
			var assign = Expression.Assign(local, Expression.Convert(from, type));

			var funcType = typeof(Func<object, DeepCloneState, object>);

			var tupleLength = type.GenericArguments().Length;

			var constructor = Expression.Assign(
				local,
				Expression.New(
					type.GetPublicConstructors().First(x => x.GetParameters().Length == tupleLength),
					type.GetPublicProperties()
						.OrderBy(x => x.Name)
						.Where(x => x.CanRead && x.Name.StartsWith("Item") && char.IsDigit(x.Name[4]))
						.Select(x => Expression.Property(local, x.Name))));

			return Expression.Lambda(
					funcType,
					Expression.Block(
						new[]
						{
							local
						},
						assign,
						constructor,
						Expression.Call(
							state,
							typeof(DeepCloneState).GetMethod("AddKnownRef"),
							from,
							local),
						from),
					from,
					state)
				.Compile();
		}
	}
}
