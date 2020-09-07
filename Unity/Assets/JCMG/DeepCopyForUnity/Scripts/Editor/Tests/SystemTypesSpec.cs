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
using System.Globalization;
using System.Text;
using Force.DeepCloner;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	[TestFixture(false)]
	public class SystemTypesSpec : BaseTest
	{
		public SystemTypesSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void StandardTypes_Should_Be_Cloned()
		{
			var b = new StringBuilder();
			b.Append("test1");
			var cloned = b.DeepClone();
			Assert.That(cloned.ToString(), Is.EqualTo("test1"));
			var arr = new[] { 1, 2, 3 };
			var enumerator = arr.GetEnumerator();
			enumerator.MoveNext();
			var enumCloned = enumerator.DeepClone();
			enumerator.MoveNext();
			Assert.That(enumCloned.Current, Is.EqualTo(1));
		}

		[Test]
		public void Funcs_Should_Be_Cloned()
		{
			var closure = new[] { "123" };
			Func<int, string> f = x => closure[0] + x.ToString(CultureInfo.InvariantCulture);
			var df = f.DeepClone();
			var cf = f.ShallowClone();
			closure[0] = "xxx";
			Assert.That(f(3), Is.EqualTo("xxx3"));
			// we clone delegate together with a closure
			Assert.That(df(3), Is.EqualTo("1233"));
			Assert.That(cf(3), Is.EqualTo("xxx3"));
		}

		public class EventHandlerTest1
		{
			public event Action<int> Event;

			public int Call(int x)
			{
				if (Event != null)
				{
					Event(x);
					return Event.GetInvocationList().Length;
				}

				return 0;
			}
		}

		[Test(Description = "Some libraries notifies about problems with this types")]
		public void Events_Should_Be_Cloned()
		{
			var eht = new EventHandlerTest1();
			var summ = new int[1];
			Action<int> a1 = x => summ[0] += x;
			Action<int> a2 = x => summ[0] += x;
			eht.Event += a1;
			eht.Event += a2;
			eht.Call(1);
			Assert.That(summ[0], Is.EqualTo(2));
			var clone = eht.DeepClone();
			clone.Call(1);
			// do not call
			Assert.That(summ[0], Is.EqualTo(2));
			eht.Event -= a1;
			eht.Event -= a2;
			Assert.That(eht.Call(1), Is.EqualTo(0)); // 0
			Assert.That(summ[0], Is.EqualTo(2)); // nothing to increment
			Assert.That(clone.Call(1), Is.EqualTo(2));
		}
	}
}
