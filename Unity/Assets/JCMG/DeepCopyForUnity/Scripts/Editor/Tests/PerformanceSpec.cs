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

//using CloneBehave;
//using CloneExtensions;
//using FastDeepCloner;
//using NClone;
//using Nuclex.Cloning;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Force.DeepCloner;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	[TestFixture]
	public class PerformanceSpec
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

		private class C2 : C1
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

		private C1Complex ManualDeepClone(C1Complex x)
		{
			var y = new C1Complex();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.C1 = ManualDeepClone(x.C1);
			y.O = new object();
			y.Array = new int[x.Array.Length];
			y.Guid = x.Guid;
			Array.Copy(x.Array, 0, y.Array, 0, x.Array.Length);
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
			double minCount = double.MaxValue;
			int iterCount = 5;
			while (iterCount-- > 0)
			{
				var sw = new Stopwatch();
				sw.Start();

				for (var i = 0; i < count; i++) action();
				var elapsed = sw.ElapsedMilliseconds;
				minCount = Math.Min(minCount, elapsed);
			}

			Console.WriteLine(name + ": " + (1000 * 1000 * minCount / count) + " ns");
			GC.Collect(2, GCCollectionMode.Forced, true, true);
		}

		[Test, Explicit("Manual")]
		public void Test_Construct_Variants()
		{
			var c = new C1 { V1 = 1, O = new object(), V2 = "xxx" };
			var c1 = new C1Complex { C1 = c, Guid = Guid.NewGuid(), O = new object(), V1 = 42, V2 = "some test string", Array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } };
			const int CountWarm = 1000;
			const int CountReal = 100000;
			// warm up
			DoTest(CountWarm, "Manual", () => ManualDeepClone(c1));
			BaseTest.SwitchTo(false);
			DoTest(CountWarm, "Deep Unsafe", () => c1.DeepClone());
			BaseTest.SwitchTo(true);
			DoTest(CountWarm, "Deep Safe", () => c1.DeepClone());
			//DoTest(CountWarm, "CloneExtensions", () => c1.GetClone());
			//DoTest(CountWarm, "NClone", () => Clone.ObjectGraph(c1));
			//DoTest(CountWarm, "Clone.Behave", () => new CloneEngine().Clone(c1));
			//DoTest(CountWarm, "GeorgeCloney", () => GeorgeCloney.CloneExtension.DeepCloneWithoutSerialization(c1));
			//DoTest(CountWarm, "FastDeepCloner", () => FastDeepCloner.DeepCloner.Clone(c1, FieldType.FieldInfo));
			//DoTest(CountWarm, "DesertOctopus", () => DesertOctopus.ObjectCloner.Clone(c1));
			DoTest(CountWarm, "Binary Formatter", () => CloneViaFormatter(c1));

			Console.WriteLine("----------------");
			DoTest(CountReal, "Manual", () => ManualDeepClone(c1));
			BaseTest.SwitchTo(false);
			DoTest(CountReal, "Deep Unsafe", () => c1.DeepClone());
			BaseTest.SwitchTo(true);
			DoTest(CountReal, "Deep Safe", () => c1.DeepClone());
			//DoTest(CountReal, "CloneExtensions", () => c1.GetClone());
			//DoTest(CountReal, "NClone", () => Clone.ObjectGraph(c1));
			//DoTest(CountReal, "Clone.Behave", () => new CloneEngine().Clone(c1));
			//DoTest(CountReal, "GeorgeCloney", () => GeorgeCloney.CloneExtension.DeepCloneWithoutSerialization(c1));
			//DoTest(CountReal, "FastDeepCloner", () => FastDeepCloner.DeepCloner.Clone(c1, FieldType.FieldInfo));
			//DoTest(CountReal, "DesertOctopus", () => DesertOctopus.ObjectCloner.Clone(c1));
			// binary is slow, reduce
			DoTest(CountReal / 10, "Binary Formatter", () => CloneViaFormatter(c1));
		}

		[Test, Explicit("Manual")]
		public void Test_Construct_Variants1()
		{
			// we cache cloners for type, so, this only variant with separate run
			BaseTest.SwitchTo(false);
			var c1 = new C1 { V1 = 1, O = new object(), V2 = "xxx" };
			// warm up
			for (var i = 0; i < 1000; i++) c1.DeepClone();
			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 10000000; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
			sw.Restart();
		}

		[Test, Explicit("Manual")]
		[TestCase(false)]
		[TestCase(true)]
		public void Test_Construct_Safe_Class_Variants(bool isSafe)
		{
			// we cache cloners for type, so, this only variant with separate run
			BaseTest.SwitchTo(isSafe);
			var c1 = new C3 { V1 = 1, V2 = "xxx" };
			//for (var i = 0; i < 1000; i++) c1.GetClone();
			for (var i = 0; i < 1000; i++) c1.DeepClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 10000000; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
			sw.Restart();

			//for (var i = 0; i < 10000000; i++) c1.GetClone();
			Console.WriteLine("Clone Extensions: " + sw.ElapsedMilliseconds);
			sw.Restart();
		}

		[Test, Explicit("Manual")]
		public void Test_Parent_Casting_Variants()
		{
			var c2 = new C2();
			var c1 = c2 as C1;
			// warm up
			for (var i = 0; i < 1000; i++) c1.DeepClone();
			for (var i = 0; i < 1000; i++) c2.DeepClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100000; i++) c2.DeepClone();
			Console.WriteLine("Direct: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 100000; i++) c1.DeepClone();
			Console.WriteLine("Parent: " + sw.ElapsedMilliseconds);
		}

		public struct S1
		{
			public C1 C;
		}

		[Test, Explicit("Manual")]
		public void Test_Array_Of_Structs_With_Class()
		{
			var c1 = new S1[100000];
			// warm up
			for (var i = 0; i < 2; i++) c1.DeepClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
		}

		[Test, Explicit("Manual")]
		public void Test_Shallow_Variants()
		{
			var c = new C1 { V1 = 1, O = new object(), V2 = "xxx" };
			var c1 = new C1Complex { C1 = c, Guid = Guid.NewGuid(), O = new object(), V1 = 42, V2 = "some test string", Array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } };
			const int CountWarm = 1000;
			const int CountReal = 10000000;

			// warm up
			DoTest(CountWarm, "Manual External", () => ManualShallowClone(c1));
			DoTest(CountWarm, "Auto Internal", () => c1.Clone());
			BaseTest.SwitchTo(false);
			DoTest(CountWarm, "Shallow Unsafe", () => c1.ShallowClone());
			BaseTest.SwitchTo(true);
			DoTest(CountWarm, "Shallow Safe", () => c1.ShallowClone());
			//DoTest(CountWarm, "Nuclex.Cloning", () => ReflectionCloner.ShallowFieldClone(c1));
			//DoTest(CountWarm, "Clone.Extensions", () => c1.GetClone(CloningFlags.Shallow));

			Console.WriteLine("------------------");
			DoTest(CountReal, "Manual External", () => ManualShallowClone(c1));
			DoTest(CountReal, "Auto Internal", () => c1.Clone());
			BaseTest.SwitchTo(false);
			DoTest(CountReal, "Shallow Unsafe", () => c1.ShallowClone());
			BaseTest.SwitchTo(true);
			DoTest(CountReal, "Shallow Safe", () => c1.ShallowClone());
			//DoTest(CountReal / 10, "Nuclex.Cloning", () => ReflectionCloner.ShallowFieldClone(c1));
			//DoTest(CountReal, "Clone.Extensions", () => c1.GetClone(CloningFlags.Shallow));
		}

		public class TreeNode
		{
			public TreeNode Item1 { get; set; }

			public TreeNode Item2 { get; set; }
		}

		[Test, Explicit("Manual")]
		public void Test_Deep_Tree()
		{
			// we cache cloners for type, so, this only variant with separate run
			BaseTest.SwitchTo(false);

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
			for (var i = 0; i < 1000; i++) c1.DeepClone();
			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100000; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
			sw.Restart();
		}

		[Test]
		public void Test_Tuples()
		{
			var c = new Tuple<int, string, bool>(42, "test", false);
			const int CountWarm = 1000;
			const int CountReal = 500000;
			// warm up
			DoTest(CountWarm, "Manual", () => new Tuple<int, string, bool>(c.Item1, c.Item2, c.Item3));
			BaseTest.SwitchTo(false);
			DoTest(CountWarm, "Deep Unsafe", () => c.DeepClone());
			BaseTest.SwitchTo(true);
			DoTest(CountWarm, "Deep Safe", () => c.DeepClone());

			Console.WriteLine("----------------");
			DoTest(CountReal, "Manual", () => new Tuple<int, string, bool>(c.Item1, c.Item2, c.Item3));
			BaseTest.SwitchTo(false);
			DoTest(CountReal, "Deep Unsafe", () => c.DeepClone());
			BaseTest.SwitchTo(true);
			DoTest(CountReal, "Deep Safe", () => c.DeepClone());
		}
	}
}
