using CloneBehave;
using CloneExtensions;
using FastDeepCloner;
using NClone;
using Nuclex.Cloning;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	[TestFixture]
	public sealed class VariantPerformanceTests
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

		// safe class
		public class C3
		{
			public int V1 { get; set; }

			public string V2 { get; set; }
		}

		[Test]
		[Explicit("Manual")]
		public void Test_Construct_Variants()
		{
			var c = new C1
			{
				V1 = 1, O = new object(), V2 = "xxx"
			};
			var c1 = new C1Complex
			{
				C1 = c,
				Guid = Guid.NewGuid(),
				O = new object(),
				V1 = 42,
				V2 = "some test string",
				Array = new[]
				{
					1,
					2,
					3,
					4,
					5,
					6,
					7,
					8,
					9,
					10
				}
			};
			const int CountWarm = 100;
			const int CountReal = 10000;
			// warm up
			DoTest(CountWarm, "Manual", () => ManualDeepClone(c1));
			DoTest(CountWarm, "Deep Safe", () => c1.DeepClone());
			DoTest(CountWarm, "CloneExtensions", () => c1.GetClone());
			DoTest(CountWarm, "NClone", () => Clone.ObjectGraph(c1));
			DoTest(CountWarm, "Clone.Behave", () => new CloneEngine().Clone(c1));
			DoTest(CountWarm, "GeorgeCloney", () => GeorgeCloney.CloneExtension.DeepCloneWithoutSerialization(c1));
			DoTest(CountWarm, "FastDeepCloner", () => DeepCloner.Clone(c1, FieldType.FieldInfo));
			DoTest(CountWarm, "DesertOctopus", () => DesertOctopus.ObjectCloner.Clone(c1));
			DoTest(CountWarm, "Binary Formatter", () => CloneViaFormatter(c1));

			UnityEngine.Debug.Log("----------------");
			DoTest(CountReal, "Manual", () => ManualDeepClone(c1));
			DoTest(CountReal, "Deep Safe", () => c1.DeepClone());
			DoTest(CountReal, "CloneExtensions", () => c1.GetClone());
			DoTest(CountReal, "NClone", () => Clone.ObjectGraph(c1));
			DoTest(CountReal, "Clone.Behave", () => new CloneEngine().Clone(c1));
			DoTest(CountReal, "GeorgeCloney", () => GeorgeCloney.CloneExtension.DeepCloneWithoutSerialization(c1));
			DoTest(CountReal, "FastDeepCloner", () => DeepCloner.Clone(c1, FieldType.FieldInfo));
			DoTest(CountReal, "DesertOctopus", () => DesertOctopus.ObjectCloner.Clone(c1));
			// binary is slow, reduce
			DoTest(CountReal / 10, "Binary Formatter", () => CloneViaFormatter(c1));
		}

		[Test]
		[Explicit("Manual")]
		public void Test_Construct_Safe_Class_Variants()
		{
			// Warmup
			var c1 = new C3
			{
				V1 = 1, V2 = "xxx"
			};
			for (var i = 0; i < 1000; i++)
			{
				c1.GetClone();
			}

			for (var i = 0; i < 1000; i++)
			{
				c1.DeepClone();
			}

			// Test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 10000000; i++)
			{
				c1.DeepClone();
			}

			UnityEngine.Debug.Log("Deep: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 10000000; i++)
			{
				c1.GetClone();
			}

			UnityEngine.Debug.Log("Clone Extensions: " + sw.ElapsedMilliseconds);
			sw.Restart();
		}

		[Test]
		[Explicit("Manual")]
		public void Test_Shallow_Variants()
		{
			var c = new C1
			{
				V1 = 1, O = new object(), V2 = "xxx"
			};
			var c1 = new C1Complex
			{
				C1 = c,
				Guid = Guid.NewGuid(),
				O = new object(),
				V1 = 42,
				V2 = "some test string",
				Array = new[]
				{
					1,
					2,
					3,
					4,
					5,
					6,
					7,
					8,
					9,
					10
				}
			};
			const int CountWarm = 1000;
			const int CountReal = 10000000;

			// warm up
			DoTest(CountWarm, "Manual External", () => ManualShallowClone(c1));
			DoTest(CountWarm, "Auto Internal", () => c1.Clone());
			DoTest(CountWarm, "Shallow Safe", () => c1.ShallowClone());
			DoTest(CountWarm, "Nuclex.Cloning", () => ReflectionCloner.ShallowFieldClone(c1));
			DoTest(CountWarm, "Clone.Extensions", () => c1.GetClone(CloningFlags.Shallow));

			UnityEngine.Debug.Log("------------------");
			DoTest(CountReal, "Manual External", () => ManualShallowClone(c1));
			DoTest(CountReal, "Auto Internal", () => c1.Clone());
			DoTest(CountReal, "Shallow Safe", () => c1.ShallowClone());
			DoTest(CountReal / 10, "Nuclex.Cloning", () => ReflectionCloner.ShallowFieldClone(c1));
			DoTest(CountReal, "Clone.Extensions", () => c1.GetClone(CloningFlags.Shallow));
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
	}
}
