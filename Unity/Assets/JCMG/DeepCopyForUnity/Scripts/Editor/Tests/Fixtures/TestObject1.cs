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

namespace JCMG.DeepCopyForUnity.Editor.Tests
{
	public class TestObject1
	{
		public byte Byte { get; set; }

		public short Short { get; set; }

		public ushort UShort { get; set; }

		public int Int { get; set; }

		public uint UInt { get; set; }

		public long Long { get; set; }

		public ulong ULong { get; set; }

		public float Float { get; set; }

		public double Double { get; set; }

		public decimal Decimal { get; set; }

		public char Char { get; set; }

		public string String { get; set; }

		public DateTime DateTime { get; set; }

		public bool Bool { get; set; }

		public IntPtr IntPtr { get; set; }

		public UIntPtr UIntPtr { get; set; }

		public AttributeTargets Enum { get; set; }
	}
}
