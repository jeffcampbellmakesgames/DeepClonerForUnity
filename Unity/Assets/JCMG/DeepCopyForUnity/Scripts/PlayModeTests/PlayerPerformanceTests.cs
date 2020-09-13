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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.PlayModeTests
{
	[TestFixture]
	public class PlayerPerformanceTests
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

		// safe class
		public class C3
		{
			public int V1 { get; set; }

			public string V2 { get; set; }
		}

		private C1 ManualDeepClone(C1 x)
		{
			var y = new C1();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.O = new object();
			return y;
		}

		private const int WARMUP_COUNT = 100;
		private const int TEST_COUNT   = 100000; // 100K

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

		private C1 ManualShallowClone(C1 x)
		{
			var y = new C1();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.O = x.O;
			return y;
		}

		private C1Complex ManualShallowClone(C1Complex x)
		{
			var y = new C1Complex();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.O = x.O;
			y.Guid = x.Guid;
			y.Array = x.Array;
			y.C1 = x.C1;
			return y;
		}

		private T CloneViaFormatter<T>(T obj)
		{
			var bf = new BinaryFormatter();
			var ms = new MemoryStream();
			bf.Serialize(ms, obj);
			ms.Seek(0, SeekOrigin.Begin);
			return (T)bf.Deserialize(ms);
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
		[Ignore("Skip For now")]
		public void Test_Construct_Variants1()
		{
			// we cache cloners for type, so, this only variant with separate run
			var c1 = new C1
			{
				V1 = 1, O = new object(), V2 = "xxx"
			};
			// warm up
			for (var i = 0; i < WARMUP_COUNT; i++)
			{
				c1.DeepClone();
			}

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < TEST_COUNT; i++)
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.LogFormat("Deep: {0}ms", sw.ElapsedMilliseconds);
		}

		[Test]
		[Ignore("Skip For now")]
		public void Test_Parent_Casting_Variants()
		{
			var c2 = new C2();
			var c1 = c2 as C1;
			// warm up
			for (var i = 0; i < WARMUP_COUNT; i++)
			{
				c1.DeepClone();
			}

			for (var i = 0; i < WARMUP_COUNT; i++)
			{
				c2.DeepClone();
			}

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < TEST_COUNT; i++)
			{
				c2.DeepClone();
			}

			UnityEngine.Debug.LogFormat("Direct: {0}ms", sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < TEST_COUNT; i++)
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.LogFormat("Parent: {0}ms", sw.ElapsedMilliseconds);
		}

		public struct S1
		{
			public C1 C;
		}

		[Test]
		public void Test_Array_Of_Structs_With_Class()
		{
			var c1 = new S1[1000];
			// warm up
			for (var i = 0; i < WARMUP_COUNT; i++)
			{
				c1.DeepClone();
			}

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < TEST_COUNT; i++)
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.LogFormat("Deep: {0}ms", sw.ElapsedMilliseconds);
		}

		public class TreeNode
		{
			public TreeNode Item1 { get; set; }

			public TreeNode Item2 { get; set; }
		}

		[Test]
		[Ignore("Skip For now")]
		public void Test_Deep_Tree()
		{
			var c1 = new TreeNode();

			c1.Item1 = new TreeNode();
			c1.Item2 = new TreeNode();
			c1.Item1.Item1 = c1;
			c1.Item1.Item2 = c1.Item2;
			c1.Item2 = new TreeNode();
			c1.Item2.Item1 = new TreeNode();
			c1.Item2.Item2 = new TreeNode();
			c1.Item2.Item1.Item1 = new TreeNode();
			c1.Item2.Item1.Item2 = new TreeNode();
			c1.Item2.Item1.Item1.Item1 = new TreeNode();
			c1.Item2.Item1.Item1.Item2 = c1;

			var elem = c1.Item2.Item1.Item1.Item1;
			for (var i = 0; i < 100; i++)
			{
				elem.Item1 = new TreeNode();
				elem.Item2 = new TreeNode();
				elem.Item1.Item1 = elem.Item2;
				elem.Item1.Item2 = new TreeNode();
				elem.Item2.Item2 = new TreeNode();
				elem = elem.Item2.Item2;
			}


			var clone = c1.DeepClone();
			Assert.That(ReferenceEquals(clone, clone.Item2.Item1.Item1.Item2), Is.True);
			//return;

			// warm up
			for (var i = 0; i < 2; i++)
			{
				c1.DeepClone();
			}

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100; i++)
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.LogFormat("Deep: {0}ms", sw.ElapsedMilliseconds);
			sw.Restart();
		}

		[Test]
		[Ignore("Skip For now")]
		public void Test_Tuples()
		{
			try
			{

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
			var c = new Tuple<int, string, bool>(42, "test", false);
			// warm up
			DoTest(WARMUP_COUNT, "Manual", () => new Tuple<int, string, bool>(c.Item1, c.Item2, c.Item3));
			DoTest(WARMUP_COUNT, "Deep Safe", () => c.DeepClone());

			UnityEngine.Debug.Log("----------------");
			DoTest(TEST_COUNT, "Manual", () => new Tuple<int, string, bool>(c.Item1, c.Item2, c.Item3));
			DoTest(TEST_COUNT, "Deep Safe", () => c.DeepClone());
		}
	}
}
