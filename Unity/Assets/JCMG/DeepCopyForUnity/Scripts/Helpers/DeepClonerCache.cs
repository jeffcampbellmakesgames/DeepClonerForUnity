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
using UnityEngine.Scripting;

namespace JCMG.DeepCopyForUnity
{
	[Preserve]
	internal static class DeepClonerCache
	{
		private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object> _typeCacheDeepTo = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object> _typeCacheShallowTo = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object>
			_structAsObjectCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache =
			new ConcurrentDictionary<Tuple<Type, Type>, object>();

		public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
		{
			// return _typeCache.GetOrAdd(type, x => adder(x));

			// this implementation is slightly faster than getoradd
			object value;
			if (_typeCache.TryGetValue(type, out value))
			{
				return value;
			}

			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = _typeCache.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		public static object GetOrAddDeepClassTo<T>(Type type, Func<Type, T> adder)
		{
			object value;
			if (_typeCacheDeepTo.TryGetValue(type, out value))
			{
				return value;
			}

			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = _typeCacheDeepTo.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		public static object GetOrAddShallowClassTo<T>(Type type, Func<Type, T> adder)
		{
			object value;
			if (_typeCacheShallowTo.TryGetValue(type, out value))
			{
				return value;
			}

			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = _typeCacheShallowTo.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
		{
			// return _typeCache.GetOrAdd(type, x => adder(x));

			// this implementation is slightly faster than getoradd
			object value;
			if (_structAsObjectCache.TryGetValue(type, out value))
			{
				return value;
			}

			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = _structAsObjectCache.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
		{
			return (T)_typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), tuple => adder(tuple.Item1, tuple.Item2));
		}

		/// <summary>
		///     This method can be used when we switch between safe / unsafe variants (for testing)
		/// </summary>
		public static void ClearCache()
		{
			_typeCache.Clear();
			_typeCacheDeepTo.Clear();
			_typeCacheShallowTo.Clear();
			_structAsObjectCache.Clear();
			_typeConvertCache.Clear();
		}
	}
}
