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
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting;

namespace Force.DeepCloner.Helpers
{
	/// <summary>
	///     Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
	/// </summary>
	internal static class DeepClonerSafeTypes
	{
		internal static readonly ConcurrentDictionary<Type, bool> KnownTypes = new ConcurrentDictionary<Type, bool>();

		static DeepClonerSafeTypes()
		{
			foreach (
				var x in
				new[]
				{
					typeof(byte),
					typeof(short),
					typeof(ushort),
					typeof(int),
					typeof(uint),
					typeof(long),
					typeof(ulong),
					typeof(float),
					typeof(double),
					typeof(decimal),
					typeof(char),
					typeof(string),
					typeof(bool),
					typeof(DateTime),
					typeof(IntPtr),
					typeof(UIntPtr),
					typeof(Guid),
					// do not clone such native type
					Type.GetType("System.RuntimeType"),
					Type.GetType("System.RuntimeTypeHandle"),
					typeof(DBNull)
				})
			{
				KnownTypes.TryAdd(x, true);
			}
		}

		private static bool CanReturnSameObject(Type type, HashSet<Type> processingTypes)
		{
			bool isSafe;
			if (KnownTypes.TryGetValue(type, out isSafe))
			{
				return isSafe;
			}

			// enums are safe
			// pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
			if (type.IsEnum() || type.IsPointer)
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			// do not do anything with remoting. it is very dangerous to clone, bcs it relate to deep core of framework
			if (type.FullName.StartsWith("System.Runtime.Remoting.") &&
			    type.Assembly == typeof(CustomErrorsModes).Assembly)
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			if (type.FullName.StartsWith("System.Reflection.") && type.Assembly == typeof(PropertyInfo).Assembly)
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			// this types are serious native resources, it is better not to clone it
			if (type.IsSubclassOf(typeof(CriticalFinalizerObject)))
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			// Better not to do anything with COM
			if (type.IsCOMObject)
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			// classes are always unsafe (we should copy it fully to count references)
			if (!type.IsValueType())
			{
				KnownTypes.TryAdd(type, false);
				return false;
			}

			if (processingTypes == null)
			{
				processingTypes = new HashSet<Type>();
			}

			// structs cannot have a loops, but check it anyway
			processingTypes.Add(type);

			var fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				fi.AddRange(tp.GetAllFields());
				tp = tp.BaseType();
			} while (tp != null);

			foreach (var fieldInfo in fi)
			{
				// type loop
				var fieldType = fieldInfo.FieldType;
				if (processingTypes.Contains(fieldType))
				{
					continue;
				}

				// not safe and not not safe. we need to go deeper
				if (!CanReturnSameObject(fieldType, processingTypes))
				{
					KnownTypes.TryAdd(type, false);
					return false;
				}
			}

			KnownTypes.TryAdd(type, true);
			return true;
		}

		public static bool CanReturnSameObject(Type type)
		{
			return CanReturnSameObject(type, null);
		}
	}
}
