using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FsmReader {
	public static class BinaryReaderExtensions {
		public const int MaxStringLength = 10000;

		/// <summary>
		/// Read a null terminated string from this BinaryReader.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="length">The length of the string in bytes including the null terminator</param>
		/// <returns>The string minus the null at the end</returns>
		public static string ReadNullTerminatedString(this BinaryReader reader, int length) {
			if (length < 0 || length > MaxStringLength) {
				Console.WriteLine("String is suspiciously long " + length + " bytes");
			}

			string nullTerminatedString = new string(reader.ReadChars(length));

			if (!nullTerminatedString.EndsWith("\0")) {
				throw new InvalidDataException("Null terminated string doesn't end with a 0x0");
			}

			return nullTerminatedString.Substring(0, nullTerminatedString.Length - 1);
		}
	}
}
