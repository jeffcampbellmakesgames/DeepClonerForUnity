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

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace JCMG.DeepCopyForUnity.PlayModeTests
{
	[TestFixture]
	public class ArrayTests
	{
		[Test]
		public void IntArray_Should_Be_Cloned()
		{
			var arr = new[]
			{
				1,
				2,
				3
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(3));
			CollectionAssert.AreEqual(arr, cloned);
		}

		[Test]
		public void StringArray_Should_Be_Cloned()
		{
			var arr = new[]
			{
				"1",
				"2",
				"3"
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(3));
			CollectionAssert.AreEqual(arr, cloned);
		}

		[Test]
		public void StringArray_Should_Be_Cloned_Two_Arrays()
		{
			// checking that cached object correctly clones arrays of different length
			var arr = new[]
			{
				"111111111111111111111",
				"2",
				"3"
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(3));
			CollectionAssert.AreEqual(arr, cloned);
			// strings should not be copied
			Assert.That(ReferenceEquals(arr[1], cloned[1]), Is.True);

			arr = new[]
			{
				"1",
				"2",
				"3",
				"4"
			};
			cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(4));
			CollectionAssert.AreEqual(arr, cloned);

			arr = new string[0];
			cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(0));

			if (1.Equals(1))
			{
				arr = null;
			}

			Assert.That(arr.DeepClone(), Is.Null);
		}

		[Test]
		public void StringArray_Casted_As_Object_Should_Be_Cloned()
		{
			// checking that cached object correctly clones arrays of different length
			var arr = (object)new[]
			{
				"1",
				"2",
				"3"
			};
			var cloned = arr.DeepClone() as string[];
			Assert.That(cloned.Length, Is.EqualTo(3));
			CollectionAssert.AreEqual((string[])arr, cloned);
			// strings should not be copied
			Assert.That(ReferenceEquals(((string[])arr)[1], cloned[1]), Is.True);
		}

		[Test]
		public void ByteArray_Should_Be_Cloned()
		{
			// checking that cached object correctly clones arrays of different length
			var arr = Encoding.ASCII.GetBytes("test");
			var cloned = arr.DeepClone();
			CollectionAssert.AreEqual(arr, cloned);

			arr = Encoding.ASCII.GetBytes(
				"test testtest testtest testtest testtest testtest testtest testtest testtest testtest testtest testtest testtest testte");
			cloned = arr.DeepClone();
			CollectionAssert.AreEqual(arr, cloned);
		}

		public class C1
		{
			public C1(int x)
			{
				X = x;
			}

			public int X { get; set; }
		}

		[Test]
		public void ClassArray_Should_Be_Cloned()
		{
			var arr = new[]
			{
				new C1(1),
				new C1(2)
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0].X, Is.EqualTo(1));
			Assert.That(cloned[1].X, Is.EqualTo(2));
			Assert.That(cloned[0], Is.Not.EqualTo(arr[0]));
			Assert.That(cloned[1], Is.Not.EqualTo(arr[1]));
		}

		public struct S1
		{
			public S1(int x)
			{
				X = x;
			}

			public int X;
		}

		public struct S2
		{
			public C1 C;
		}

		[Test]
		public void StructArray_Should_Be_Cloned()
		{
			var arr = new[]
			{
				new S1(1),
				new S1(2)
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0].X, Is.EqualTo(1));
			Assert.That(cloned[1].X, Is.EqualTo(2));
		}

		[Test]
		public void StructArray_With_Class_Should_Be_Cloned()
		{
			var arr = new[]
			{
				new S2
				{
					C = new C1(1)
				},
				new S2
				{
					C = new C1(2)
				}
			};

			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0].C.X, Is.EqualTo(1));
			Assert.That(cloned[1].C.X, Is.EqualTo(2));
			Assert.That(cloned[0].C, Is.Not.EqualTo(arr[0].C));
			Assert.That(cloned[1].C, Is.Not.EqualTo(arr[1].C));
		}

		[Test]
		public void NullArray_hould_Be_Cloned()
		{
			var arr = new C1[]
			{
				null,
				null
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0], Is.Null);
			Assert.That(cloned[1], Is.Null);
		}

		[Test]
		public void NullAsArray_hould_Be_Cloned()
		{
			var arr = (int[])null;
			// ReSharper disable ExpressionIsAlwaysNull
			var cloned = arr.DeepClone();
			// ReSharper restore ExpressionIsAlwaysNull
			Assert.That(cloned, Is.Null);
		}

		[Test]
		public void IntList_Should_Be_Cloned()
		{
			// TODO: better performance for this type
			var arr = new List<int>
			{
				1, 2, 3
			};
			var cloned = arr.DeepClone();
			Assert.That(cloned.Count, Is.EqualTo(3));
			Assert.That(cloned[0], Is.EqualTo(1));
			Assert.That(cloned[1], Is.EqualTo(2));
			Assert.That(cloned[2], Is.EqualTo(3));
		}

		[Test]
		public void Dictionary_Should_Be_Cloned()
		{
			// TODO: better performance for this type
			var d = new Dictionary<string, decimal>();
			d["a"] = 1;
			d["b"] = 2;
			var cloned = d.DeepClone();
			Assert.That(cloned.Count, Is.EqualTo(2));
			Assert.That(cloned["a"], Is.EqualTo(1));
			Assert.That(cloned["b"], Is.EqualTo(2));
		}

		[Test]
		public void Array_Of_Same_Arrays_Should_Be_Cloned()
		{
			var c1 = new[]
			{
				1,
				2,
				3
			};
			var arr = new[]
			{
				c1,
				c1,
				c1,
				c1,
				c1
			};
			var cloned = arr.DeepClone();

			Assert.That(cloned.Length, Is.EqualTo(5));
			// lot of objects for checking reference dictionary optimization
			Assert.That(ReferenceEquals(arr[0], cloned[0]), Is.False);
			Assert.That(ReferenceEquals(cloned[0], cloned[1]), Is.True);
			Assert.That(ReferenceEquals(cloned[1], cloned[2]), Is.True);
			Assert.That(ReferenceEquals(cloned[1], cloned[3]), Is.True);
			Assert.That(ReferenceEquals(cloned[1], cloned[4]), Is.True);
		}

		public class AC
		{
			public int[] A { get; set; }

			public int[] B { get; set; }
		}

		[Test]
		public void Class_With_Same_Arrays_Should_Be_Cloned()
		{
			var ac = new AC();
			ac.A = ac.B = new int[3];
			var clone = ac.DeepClone();
			Assert.That(ReferenceEquals(ac.A, clone.A), Is.False);
			Assert.That(ReferenceEquals(clone.A, clone.B), Is.True);
		}

		[Test]
		public void Class_With_Null_Array_hould_Be_Cloned()
		{
			var ac = new AC();
			var cloned = ac.DeepClone();
			Assert.That(cloned.A, Is.Null);
			Assert.That(cloned.B, Is.Null);
		}

		[Test]
		public void MultiDim_Array_Should_Be_Cloned()
		{
			var arr = new int[2, 2];
			arr[0, 0] = 1;
			arr[0, 1] = 2;
			arr[1, 0] = 3;
			arr[1, 1] = 4;
			var clone = arr.DeepClone();
			Assert.That(ReferenceEquals(arr, clone), Is.False);
			Assert.That(clone[0, 0], Is.EqualTo(1));
			Assert.That(clone[0, 1], Is.EqualTo(2));
			Assert.That(clone[1, 0], Is.EqualTo(3));
			Assert.That(clone[1, 1], Is.EqualTo(4));
		}

		[Test]
		public void MultiDim_Array_Should_Be_Cloned2()
		{
			var arr = new int[2, 2, 1];
			arr[0, 0, 0] = 1;
			arr[0, 1, 0] = 2;
			arr[1, 0, 0] = 3;
			arr[1, 1, 0] = 4;
			var clone = arr.DeepClone();
			Assert.That(ReferenceEquals(arr, clone), Is.False);
			Assert.That(clone[0, 0, 0], Is.EqualTo(1));
			Assert.That(clone[0, 1, 0], Is.EqualTo(2));
			Assert.That(clone[1, 0, 0], Is.EqualTo(3));
			Assert.That(clone[1, 1, 0], Is.EqualTo(4));
		}

		[Test]
		public void MultiDim_Array_Of_Classes_Should_Be_Cloned()
		{
			var arr = new AC[2, 2];
			arr[0, 0] = arr[1, 1] = new AC();
			var clone = arr.DeepClone();
			Assert.That(clone[0, 0], Is.Not.Null);
			Assert.That(clone[1, 1], Is.Not.Null);
			Assert.That(clone[1, 1], Is.EqualTo(clone[0, 0]));
			Assert.That(clone[1, 1], Is.Not.EqualTo(arr[0, 0]));
		}

		[Test]
		public void NonZero_Based_Array_Should_Be_Cloned()
		{
			var arr = Array.CreateInstance(
				typeof(int),
				new[]
				{
					2
				},
				new[]
				{
					1
				});

			arr.SetValue(1, 1);
			arr.SetValue(2, 2);
			var clone = arr.DeepClone();
			Assert.That(clone.GetValue(1), Is.EqualTo(1));
			Assert.That(clone.GetValue(2), Is.EqualTo(2));
		}

		[Test]
		public void NonZero_Based_MultiDim_Array_Should_Be_Cloned()
		{
			var arr = Array.CreateInstance(
				typeof(int),
				new[]
				{
					2,
					2
				},
				new[]
				{
					1,
					1
				});

			arr.SetValue(1, 1, 1);
			arr.SetValue(2, 2, 2);
			var clone = arr.DeepClone();
			Assert.That(clone.GetValue(1, 1), Is.EqualTo(1));
			Assert.That(clone.GetValue(2, 2), Is.EqualTo(2));
		}

		[Test]
		public void Array_As_Generic_Array_Should_Be_Cloned()
		{
			var arr = new[]
			{
				1,
				2,
				3
			};
			var genArr = (Array)arr;
			var clone = (int[])genArr.DeepClone();
			Assert.That(clone.Length, Is.EqualTo(3));
			Assert.That(clone[0], Is.EqualTo(1));
			Assert.That(clone[1], Is.EqualTo(2));
			Assert.That(clone[2], Is.EqualTo(3));
		}

		[Test]
		public void Array_As_IEnumerable_Should_Be_Cloned()
		{
			var arr = new[]
			{
				1,
				2,
				3
			};
			var genArr = (IEnumerable<int>)arr;
			var clone = (int[])genArr.DeepClone();
			// ReSharper disable PossibleMultipleEnumeration
			Assert.That(clone.Length, Is.EqualTo(3));
			Assert.That(clone[0], Is.EqualTo(1));
			Assert.That(clone[1], Is.EqualTo(2));
			Assert.That(clone[2], Is.EqualTo(3));
			// ReSharper restore PossibleMultipleEnumeration
		}
	}
}
