using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FsmReader {
	public class DepthFirstSearch : IVisitor {
		private Treenode root;

		private Func<Treenode, bool> predicate;
		private Treenode result;
		private Treenode lastResult;
        private Stack<int> stack = new Stack<int>();

		public DepthFirstSearch(Treenode root) {
			this.root = root;
			Reset();
		}

		/// <summary>
		/// The number of nodes that have been visited so far during this search.
		/// </summary>
		public int VisitCount {
			get;
			set;
		}

		private void Reset() {
			VisitCount = 0;
			lastResult = null;
			result = null;
		}

		public Treenode FindNode(Func<Treenode, bool> predicate) {
			this.predicate = predicate;
			result = null;

			root.Accept(this, stack);

			lastResult = result;
			return result;
		}

		#region IVisitor Members

		public bool VisitEnter(Composite composite) {
			VisitCount++;

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
