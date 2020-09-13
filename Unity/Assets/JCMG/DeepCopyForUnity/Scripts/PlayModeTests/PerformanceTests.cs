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
using System.Diagnostics;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.PlayModeTests
{
	[TestFixture]
	public class PerformanceTests
	{
		[Serializable]
		public class C1
		{
			public int V1 { get; set; }

			public string V2 { get; set; }

			public object O { get; set; }

			public C1 Clone()
			{
				return (C1)MemberwiseClone();
			}
		}

		[Serializable]
		public class C1Complex
		{
			public int V1 { get; set; }

			public string V2 { get; set; }

			public object O { get; set; }

			public Guid? Guid { get; set; }

			public C1 C1 { get; set; }

			public int[] Array { get; set; }

			public C1Complex Clone()
			{
				return (C1Complex)MemberwiseClone();
			}
		}

		public class C2 : C1
		{
		}

		private C1 ManualDeepClone(C1 x)
		{
			var y = new C1();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.O = new object();
			return y;
		}

		private C1Complex ManualDeepClone(C1Complex x)
		{
			var y = new C1Complex();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.C1 = ManualDeepClone(x.C1);
			y.O = new object();
			y.Array = new int[x.Array.Length];
			y.Guid = x.Guid;
			Array.Copy(
				x.Array,
				0,
				y.Array,
				0,
				x.Array.Length);
			return y;
		}


		private void DoTest(int count, string name, Action action)
		{
			var minCount = double.MaxValue;
			var iterCount = 5;
			while (iterCount-- > 0)
			{
				var sw = new Stopwatch();
				sw.Start();

				for (var i = 0; i < count; i++)
				{
					action();
				}

				var elapsed = sw.ElapsedMilliseconds;
				minCount = Math.Min(minCount, elapsed);
			}

			UnityEngine.Debug.Log(name + ": " + 1000 * 1000 * minCount / count + " ns");
		}

		[Test]
		[Explicit("Manual")]
		public void Test_Construct_Variants1()
		{
			// we cache cloners for type, so, this only variant with separate run
			var c1 = new C1
			{
				V1 = 1, O = new object(), V2 = "xxx"
			};
			// warm up
			for (var i = 0; i < 1000; i++)
			{
				c1.DeepClone();
			}

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 10000000; i++) // 10 million
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.Log("Deep: " + sw.ElapsedMilliseconds);
			sw.Restart();
		}

		[Test]
		[Explicit("Manual")]
		public void Test_Parent_Casting_Variants()
		{
			var c2 = new C2();
			var c1 = c2 as C1;
			// warm up
			for (var i = 0; i < 1000; i++)
			{
				c1.DeepClone();
			}

			for (var i = 0; i < 1000; i++)
			{
				c2.DeepClone();
			}

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100000; i++)
			{
				c2.DeepClone();
			}

			UnityEngine.Debug.Log("Direct: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 100000; i++)
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.Log("Parent: " + sw.ElapsedMilliseconds);
		}

		public struct S1
		{
			public C1 C;
		}
	}
}
