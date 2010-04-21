using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace FsmReader {
	public abstract class Composite {
		public abstract ReadOnlyCollection<Composite> Children {
			get;
		}

		static int acceptCount;

		public bool Accept(IVisitor visitor, ref Stack<int> stack) {
			Stack<int> inStack = new Stack<int>();
			acceptCount = 0;
			
			// Reverse the stack
			Stack<int> resumeStack = new Stack<int>(stack.AsEnumerable());
			// Remove root element
			if (resumeStack.Count > 0) resumeStack.Pop();

			bool result = Accept(visitor, inStack, resumeStack);

			stack = inStack;

			Console.WriteLine("--------------------------------");
			foreach (int c in stack) {
				Console.WriteLine(c);
			}
			Console.WriteLine("--------------------------------");

			Console.WriteLine("Accept Count = " + acceptCount);
			return result;
		}

		private bool Accept(IVisitor visitor, Stack<int> inStack, Stack<int> resumeStack) {
			acceptCount++;
			if (visitor.VisitEnter(this)) {

				int firstChild = 0;

				if (resumeStack.Count > 0) {
					firstChild = resumeStack.Pop();
					if (resumeStack.Count == 0) firstChild++;
				}

				for (int i = firstChild; i < Children.Count; i++) {
					inStack.Push(i);

					Composite c = Children[i];
					if (!c.Accept(visitor, inStack, resumeStack)) {
						break;
					} else {
						inStack.Pop();
					}
				}
			}

			bool ret = visitor.VisitExit(this);
			return ret;
		}

		//private bool Accept(IVisitor visitor, Stack<Composite> inStack, Stack<Composite> resumeStack) {
		//    acceptCount++;

		//    inStack.Push(this);

		//    if (resumeStack.Count > 0) {
		//        Treenode top = (Treenode)resumeStack.Pop();
		//        if (resumeStack.Count == 0) {
		//            // This is the node that we found last time
		//            int nextIndex = int.MaxValue;
		//            for (int i = 0; i < Children.Length; i++) {
		//                if (Children[i] == top) {
		//                    nextIndex = i + 1;
		//                    break;
		//                }
		//            }
		//            for (int i = nextIndex; i < Children.Length; i++) {
		//                Children[i].Accept(visitor, inStack, resumeStack);
		//            }
		//        } else {
		//            Children.First(s => s == top).Accept(visitor, inStack, resumeStack);
		//        }
		//    } else {
		//        if (visitor.VisitEnter(this)) {
		//            foreach (Composite c in Children) {
		//                if (!c.Accept(visitor, inStack, resumeStack)) {
		//                    break;
		//                }
		//            }
		//        }
		//    }

		//    bool ret = visitor.VisitExit(this);
		//    if (ret) {
		//        inStack.Pop();
		//    }
		//    return ret;
		//}
	}
}
