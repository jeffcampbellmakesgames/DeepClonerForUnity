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
using System.Security;
using UnityEngine.Scripting;

namespace JCMG.DeepCopyForUnity
{
	/// <summary>
	///     Extensions for object cloning
	/// </summary>
	[Preserve]
	public static class DeepClonerExtensions
	{
		static DeepClonerExtensions()
		{
			if (!PermissionCheck())
			{
				throw new SecurityException(
					"DeepCloner should have enough permissions to run. Grant FullTrust or Reflection permission.");
			}
		}

		/// <summary>
		///     Performs deep (full) copy of object and related graph
		/// </summary>
		public static T DeepClone<T>(this T obj)
		{
			return DeepClonerGenerator.CloneObject(obj);
		}

		/// <summary>
		///     Performs deep (full) copy of object and related graph to existing object
		/// </summary>
		/// <returns> existing filled object </returns>
		/// <remarks> Method is valid only for classes, classes should be descendants in reality, not in declaration </remarks>
		public static TTo DeepCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo)
			where TTo : class, TFrom
		{
			return (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, true);
		}

		/// <summary>
		///     Performs shallow copy of object to existing object
		/// </summary>
		/// <returns> existing filled object </returns>
		/// <remarks> Method is valid only for classes, classes should be descendants in reality, not in declaration </remarks>
		public static TTo ShallowCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo)
			where TTo : class, TFrom
		{
			return (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, false);
		}

		/// <summary>
		///     Performs shallow (only new object returned, without cloning of dependencies) copy of object
		/// </summary>
		public static T ShallowClone<T>(this T obj)
		{
			return ShallowClonerGenerator.CloneObject(obj);
		}

		private static bool PermissionCheck()
		{
			// best way to check required permission: execute something and receive exception
			// .net security policy is weird for normal usage
			try
			{
				new object().ShallowClone();
			}
			catch (VerificationException)
			{
				return false;
			}
			catch (MemberAccessException)
			{
				return false;
			}

			return true;
		}
	}
}
