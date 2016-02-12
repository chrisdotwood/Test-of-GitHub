using FsmReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFile {
	class Program {
		static void Main(string[] args) {
			Treenode root = null;

			using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\objectwithchildtypes.t", FileMode.Open)) {
				// TODO Validate preamble
				// Skip the first 0x48 bytes
				stream.Position = 0x48;

				using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress)) {
					//using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\doublenode.t.unzipped", FileMode.Open)) {
					root = Treenode.Read(zipStream);
				}
			}
		}
	}
}
