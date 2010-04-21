using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FsmReader;
using System.IO;

namespace FsmReaderTest {
	class Program {
		static void Main(string[] args) {
			DateTime start = DateTime.Now;
			Treenode root = Treenode.Read(new FileStream("large_model.fsm", FileMode.Open));
			Console.WriteLine("Read file in " + (DateTime.Now - start).ToString());

			Console.WriteLine();
		}
	}
}
