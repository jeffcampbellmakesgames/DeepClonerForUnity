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
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class ReflectionHelper
	{
		public static bool IsEnum(this Type t)
		{
			return t.IsEnum;
		}

		public static bool IsValueType(this Type t)
		{
			return t.IsValueType;
		}

		public static bool IsClass(this Type t)
		{
			return t.IsClass;
		}

		public static Type BaseType(this Type t)
		{
			return t.BaseType;
		}

		public static FieldInfo[] GetAllFields(this Type t)
		{
			return t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		}

		public static PropertyInfo[] GetPublicProperties(this Type t)
		{
			return t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		}

		public static FieldInfo[] GetDeclaredFields(this Type t)
		{
			return t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
		}

		public static ConstructorInfo[] GetPrivateConstructors(this Type t)
		{
			return t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public static ConstructorInfo[] GetPublicConstructors(this Type t)
		{
			return t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
		}

		public static MethodInfo GetPrivateMethod(this Type t, string methodName)
		{
			return t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public static MethodInfo GetMethod(this Type t, string methodName)
		{
			return t.GetMethod(methodName);
		}

		public static MethodInfo GetPrivateStaticMethod(this Type t, string methodName)
		{
			return t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
		}

		public static FieldInfo GetPrivateField(this Type t, string fieldName)
		{
			return t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public static Type[] GenericArguments(this Type t)
		{
			return t.GetGenericArguments();
		}
	}
}
