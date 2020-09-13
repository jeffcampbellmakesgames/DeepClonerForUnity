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
using UnityEngine.Scripting;

namespace JCMG.DeepCopyForUnity
{
	[Preserve]
	internal static class ShallowClonerGenerator
	{
		public static T CloneObject<T>(T obj)
		{
			// this is faster than typeof(T).IsValueType
			if (obj is ValueType)
			{
				if (typeof(T) == obj.GetType())
				{
					return obj;
				}

				// we're here so, we clone value type obj as object type T
				// so, we need to copy it, bcs we have a reference, not real object.
				return (T)ShallowObjectCloner.CloneObject(obj);
			}

			if (ReferenceEquals(obj, null))
			{
				return (T)(object)null;
			}

			if (DeepClonerSafeTypes.CanReturnSameObject(obj.GetType()))
			{
				return obj;
			}

			return (T)ShallowObjectCloner.CloneObject(obj);
		}
	}
}
