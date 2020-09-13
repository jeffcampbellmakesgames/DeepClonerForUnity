﻿/*

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

using CloneExtensions;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	[TestFixture]
	public class CloneExtensionsTests
	{
		public class C1
		{
			public int X { get; set; }
		}

		public class C2 : C1
		{
			public new int X { get; set; }
		}

		public class C3 : C1
		{
			public int Y { get; set; }
		}

		public class C4
		{
			public C1 A { get; set; }

			public C1 B { get; set; }
		}

		public struct S5
		{
			public C1 A { get; set; }

			public C1 B { get; set; }
		}

		public class C6
		{
			public C6 A { get; set; }
		}

		[Test]
		[Ignore("Not works")]
		public void Ensure_Parent_Prop_Clone()
		{
			var c2 = new C2();
			var c1 = c2 as C1;
			c2.X = 2;
			c1.X = 1;
			var c2Clone = c2.GetClone();
			Assert.That(c2Clone.X, Is.EqualTo(1));
			Assert.That((c2Clone as C1).X, Is.EqualTo(1));
		}

		[Test]
		public void Ensure_Parent_Prop_Clone2()
		{
			var c3 = new C3();
			c3.Y = 2;
			c3.X = 1;
			var c3Clone = c3.GetClone();
			Assert.That(c3Clone.Y, Is.EqualTo(2));
			Assert.That(c3Clone.X, Is.EqualTo(1));
		}

		[Test]
		[Ignore("Not works")]
		public void Ensure_Cast_To_Parent_Clone()
		{
			var c3 = new C3();
			var c1 = c3 as C1;
			c1.X = 1;
			c3.Y = 2;
			var c1Clone = c1.GetClone();
			Assert.That(c1Clone.X, Is.EqualTo(1));
			Assert.That((c1Clone as C3).Y, Is.EqualTo(2));
		}

		[Test]
		public void Class_Should_Be_Deep_Copied()
		{
			var c4 = new C4();
			var c1 = new C1
			{
				X = 1
			};
			c4.A = c1;
			c4.B = c1;
			var c4Clone = c4.GetClone();
			c1.X = 2;
			Assert.That(c4Clone.A.X, Is.EqualTo(1));
			Assert.That(c4Clone.B.X, Is.EqualTo(1));
			Assert.That(c4Clone.A, Is.EqualTo(c4Clone.B));
		}

		[Test]
		public void Struct_Should_Be_Deep_Copied()
		{
			var s5 = new S5();
			var c1 = new C1
			{
				X = 1
			};
			s5.A = c1;
			s5.B = c1;
			var c4Clone = s5.GetClone();
			c1.X = 2;
			Assert.That(c4Clone.A.X, Is.EqualTo(1));
			Assert.That(c4Clone.B.X, Is.EqualTo(1));
			Assert.That(c4Clone.A, Is.EqualTo(c4Clone.B));
		}

		[Test]
		public void Array_Should_Be_Deep_Copied()
		{
			var c1 = new C1
			{
				X = 1
			};
			var a = new[]
			{
				c1,
				c1
			};

			var aClone = a.GetClone();
			Assert.That(aClone[0].X, Is.EqualTo(1));
			Assert.That(aClone[1].X, Is.EqualTo(1));
			Assert.That(aClone[0], Is.EqualTo(aClone[1]));
		}

		[Test]
		public void CrossRef_Should_be_Cloned()
		{
			var c = new C6();
			c.A = c;

			var aClone = c.GetClone();
			Assert.That(aClone.A, Is.EqualTo(aClone));
			Assert.That(aClone.A, Is.Not.EqualTo(c));
		}
	}
}
