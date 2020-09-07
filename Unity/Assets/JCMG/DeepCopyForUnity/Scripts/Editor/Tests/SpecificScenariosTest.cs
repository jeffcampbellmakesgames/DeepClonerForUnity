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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Force.DeepCloner;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	[TestFixture(false)]
	public class SpecificScenariosTest : BaseTest
	{
		public SpecificScenariosTest(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void Test_ExpressionTree_OrderBy1()
		{
			var q = Enumerable.Range(1, 5).Reverse().AsQueryable().OrderBy(x => x);
			var q2 = q.DeepClone();
			Assert.That(q2.ToArray()[0], Is.EqualTo(1));
			Assert.That(q.ToArray().Length, Is.EqualTo(5));
		}

		[Test]
		public void Test_ExpressionTree_OrderBy2()
		{
			var l = new List<int> { 2, 1, 3, 4, 5 }.Select(y => new Tuple<int, string>(y, y.ToString(CultureInfo.InvariantCulture)));
			var q = l.AsQueryable().OrderBy(x => x.Item1);
			var q2 = q.DeepClone();
			Assert.That(q2.ToArray()[0].Item1, Is.EqualTo(1));
			Assert.That(q.ToArray().Length, Is.EqualTo(5));
		}

		[Test]
		public void Lazy_Clone()
		{
			var lazy = new LazyClass();
			var clone = lazy.DeepClone();
			var v = LazyClass.Counter;
			Assert.That(clone.GetValue(), Is.EqualTo((v + 1).ToString(CultureInfo.InvariantCulture)));
			Assert.That(lazy.GetValue(), Is.EqualTo((v + 2).ToString(CultureInfo.InvariantCulture)));
		}

		public class LazyClass
		{
			public static int Counter;

			private readonly LazyRef<object> _lazyValue = new LazyRef<object>(() => (object)(++Counter).ToString(CultureInfo.InvariantCulture));

			public string GetValue()
			{
				return _lazyValue.Value.ToString();
			}
		}

		[Test]
		public void GenericComparer_Clone()
		{
			var comparer = new TestComparer();
			comparer.DeepClone();
		}

		[Test]
		public void Closure_Clone()
		{
			int a = 0;
			Func<int> f = () => ++a;
			var fCopy = f.DeepClone();
			Assert.That(f(), Is.EqualTo(1));
			Assert.That(fCopy(), Is.EqualTo(1));
			Assert.That(a, Is.EqualTo(1));
		}

		private class TestComparer : Comparer<int>
		{
			// make object unsafe to work
			private object _fieldX = new object();

			public override int Compare(int x, int y)
			{
				return x.CompareTo(y);
			}
		}

		public class KnownFolders
		{
			[Guid("8BE2D872-86AA-4d47-B776-32CCA40C7018"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			internal interface IKnownFolderManager
			{
				void FolderIdFromCsidl(int csidl, [Out] out Guid knownFolderID);

				void FolderIdToCsidl([In] [MarshalAs(UnmanagedType.LPStruct)] Guid id, [Out] out int csidl);

				void GetFolderIds();
			}

			[ComImport, Guid("4df0c730-df9d-4ae3-9153-aa6b82e9795a")]
			internal class KnownFolderManager
			{
				// make object unsafe to work
#pragma warning disable 169
				private object _fieldX;
#pragma warning restore 169
			}
		}

		public sealed class LazyRef<T>
		{
			private Func<T> _initializer;
			private T _value;

			/// <summary>
			///     This API supports the Entity Framework Core infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			public T Value
			{
				get
				{
					if (_initializer != null)
					{
						_value = _initializer();
						_initializer = null;
					}
					return _value;
				}
				set
				{
					_value = value;
					_initializer = null;
				}
			}

			public LazyRef(Func<T> initializer)
			{
				_initializer = initializer;
			}
		}
	}
}
