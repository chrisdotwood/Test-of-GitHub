using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FsmReader {
	public class DepthFirstSearch : IVisitor {
		private Treenode root;

		public DepthFirstSearch(Treenode root) {
			this.root = root;
		}

		private Func<Treenode, bool> predicate;
		private Treenode result;
		private Treenode lastResult;

		public Treenode FindNode(Func<Treenode, bool> predicate) {
			this.predicate = predicate;
			result = null;
			Stack<int> stack = new Stack<int>();

			root.Accept(this, ref stack);

			Console.WriteLine(result.FullPath);

			lastResult = result;
			return result;
		}

		#region IVisitor Members


		public bool VisitEnter(Composite composite) {
			Treenode node = (Treenode)composite;
			if (predicate(node) && node != lastResult) {
				lastResult = result = node;
			}
			return true;
		}

		public bool VisitExit(Composite composite) {
			bool val = result == null;

			return val;
		}

		#endregion
	}
}
