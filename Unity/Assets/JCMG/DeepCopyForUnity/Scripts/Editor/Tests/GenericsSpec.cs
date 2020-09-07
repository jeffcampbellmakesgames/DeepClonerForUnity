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
using Force.DeepCloner;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	[TestFixture(false)]
	public class GenericsSpec : BaseTest
	{
		public GenericsSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void Tuple_Should_Be_Cloned()
		{
			var c = new Tuple<int, int>(1, 2).DeepClone();
			Assert.That(c.Item1, Is.EqualTo(1));
			Assert.That(c.Item2, Is.EqualTo(2));

			c = new Tuple<int, int>(1, 2).ShallowClone();
			Assert.That(c.Item1, Is.EqualTo(1));
			Assert.That(c.Item2, Is.EqualTo(2));

			var cc = new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7).DeepClone();
			Assert.That(cc.Item7, Is.EqualTo(7));

			var tuple = new Tuple<int, Generic<object>>(1, new Generic<object>());
			tuple.Item2.Value = tuple;
			var ccc = tuple.DeepClone();
			Assert.That(ccc, Is.EqualTo(ccc.Item2.Value));
		}

		[Test]
		public void Generic_Should_Be_Cloned()
		{
			var c = new Generic<int>();
			c.Value = 12;
			Assert.That(c.DeepClone().Value, Is.EqualTo(12));

			var c2 = new Generic<object>();
			c2.Value = 12;
			Assert.That(c2.DeepClone().Value, Is.EqualTo(12));
		}

		public class C1
		{
			public int X { get; set; }
		}

		public class C2 : C1
		{
			public int Y { get; set; }
		}

		public class Generic<T>
		{
			public T Value { get; set; }
		}

		[Test]
		public void Tuple_Should_Be_Cloned_With_Inheritance_And_Same_Object()
		{
			var c2 = new C2 { X = 1, Y = 2 };
			var c = new Tuple<C1, C2>(c2, c2).DeepClone();
			var cs = new Tuple<C1, C2>(c2, c2).ShallowClone();
			c2.X = 42;
			c2.Y = 42;

			// Deep clone, objects should be distinct from c2, but equal to each other
			Assert.That(c.Item1.X, Is.EqualTo(1));
			Assert.That(c.Item2.Y, Is.EqualTo(2));
			Assert.That(c.Item2, Is.EqualTo(c.Item1));

			// Shallow clone, objects should be the same as c2
			Assert.That(cs.Item1.X, Is.EqualTo(42));
			Assert.That(cs.Item2.Y, Is.EqualTo(42));
			Assert.That(cs.Item2, Is.EqualTo(cs.Item1));
		}
	}
}
