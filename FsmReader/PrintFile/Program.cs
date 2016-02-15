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
			Treenode root;

			using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\SORG-002-05-C-004_Saker.fsm", FileMode.Open)) {
				// TODO Validate preamble
				// Skip the first 0x48 bytes
				stream.Position = 0x48;

				using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress)) {
					//using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\doublenode.t.unzipped", FileMode.Open)) {
					root = Treenode.Read(zipStream);

					PrintToFile(root);
				}
			}


		}

		static void PrintToFile(Treenode node) {
			using(Stream fs = File.Create(@"C:\users\chris.wood\desktop\new folder\output.txt")) {
				using (StreamWriter writer = new StreamWriter(fs)) {
					PrintToFile(writer, new List<Treenode>() { node }, 0);
				}
			}
		}

		static void PrintToFile(StreamWriter writer, IEnumerable<Treenode> nodes, int depth) {
			foreach (Treenode n in nodes) {
				for (int i = depth; i > 0; i--) { 
					writer.Write("-");
				}
				writer.WriteLine(" " + n.Title);
				
				if(n.NodeChildren.Count > 0) {
					PrintToFile(writer, n.NodeChildren, depth + 1);
				}
				if(n.DataChildren.Count > 0) {
					PrintToFile(writer, n.DataChildren, depth + 1);
				}
			}
		}
	}
}
