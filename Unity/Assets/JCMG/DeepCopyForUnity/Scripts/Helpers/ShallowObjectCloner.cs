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
using System.Linq.Expressions;
using UnityEngine.Scripting;

namespace JCMG.DeepCopyForUnity
{
	/// <summary>
	///     Internal class but due implementation restriction should be public
	/// </summary>
	[Preserve]
	public abstract class ShallowObjectCloner
	{
		private class ShallowSafeObjectCloner : ShallowObjectCloner
		{
			private Func<object, object> CLONE_FUNC;

			public ShallowSafeObjectCloner()
			{
				var methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");
				var p = Expression.Parameter(typeof(object));
				var mce = Expression.Call(p, methodInfo);

				CLONE_FUNC = Expression.Lambda<Func<object, object>>(mce, p).Compile();
			}

			protected override object DoCloneObject(object obj)
			{
				return CLONE_FUNC(obj);
			}
		}

		private static readonly ShallowObjectCloner INSTANCE;

		static ShallowObjectCloner()
		{
			INSTANCE = new ShallowSafeObjectCloner();
		}

		/// <summary>
		///     Abstract method for real object cloning
		/// </summary>
		protected abstract object DoCloneObject(object obj);

		/// <summary>
		///     Performs real shallow object clone
		/// </summary>
		public static object CloneObject(object obj)
		{
			return INSTANCE.DoCloneObject(obj);
		}
	}
}
