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

			//using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\cppcodenode.t", FileMode.Open)) {
			using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\Dose version 8.0.0 minus SSCandC.fsm", FileMode.Open)) {
				root = Treenode.Read(stream);

				PrintToFile(root);
			}
		}

		static void PrintToFile(Treenode node) {
			using (Stream fs = File.Create(@"C:\users\chris.wood\desktop\new folder\output.txt")) {
				using (StreamWriter writer = new StreamWriter(fs)) {
					PrintToFile(writer, new List<Treenode>() { node }, 0);
				}
			}
		}

		static void PrintToFile(StreamWriter writer, IEnumerable<Treenode> nodes, int depth) {
			foreach (Treenode n in nodes) {
				writer.Write(n.FullPath);

				if(n.DataType == DataType.Float) {
					writer.Write(n.DataAsDouble.ToString());
				}

				writer.WriteLine();

				if (n.NodeChildren.FirstOrDefault() != null) {
					PrintToFile(writer, n.NodeChildren, depth + 1);
				}
				if (n.DataChildren.FirstOrDefault() != null) {
					PrintToFile(writer, n.DataChildren, depth + 1);
				}
			}
		}
	}
}
