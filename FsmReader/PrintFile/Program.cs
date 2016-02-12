using FsmReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFile {
	class Program {
		static void Main(string[] args) {
			Treenode root = null;

			using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\stringnode.t.unzipped", FileMode.Open)) {
			//using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\doublenode.t.unzipped", FileMode.Open)) {
					root = Treenode.Read(stream);
			}

		}
	}
}
