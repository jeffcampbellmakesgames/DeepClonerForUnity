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

using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.PlayModeTests
{
	[TestFixture]
	public class LoopCheckTests
	{
		public class C1
		{
			public int F { get; set; }

			public C1 A { get; set; }
		}

		[Test]
		public void SimpleLoop_Should_Be_Handled()
		{
			var c1 = new C1();
			var c2 = new C1();
			c1.F = 1;
			c2.F = 2;
			c1.A = c2;
			c1.A.A = c1;
			var cloned = c1.DeepClone();

			Assert.That(cloned.A, Is.Not.Null);
			Assert.That(cloned.A.A.F, Is.EqualTo(cloned.F));
			Assert.That(cloned.A.A, Is.EqualTo(cloned));
		}

		[Test]
		public void Object_Own_Loop_Should_Be_Handled()
		{
			var c1 = new C1();
			c1.F = 1;
			c1.A = c1;
			var cloned = c1.DeepClone();

			Assert.That(cloned.A, Is.Not.Null);
			Assert.That(cloned.A.F, Is.EqualTo(cloned.F));
			Assert.That(cloned.A, Is.EqualTo(cloned));
		}

		[Test]
		public void Array_Of_Same_Objects_Should_Be_Cloned()
		{
			var c1 = new C1();
			var arr = new[]
			{
				c1,
				c1,
				c1
			};
			c1.F = 1;
			var cloned = arr.DeepClone();

			Assert.That(cloned.Length, Is.EqualTo(3));
			Assert.That(cloned[0], Is.EqualTo(cloned[1]));
			Assert.That(cloned[1], Is.EqualTo(cloned[2]));
		}
	}
}
