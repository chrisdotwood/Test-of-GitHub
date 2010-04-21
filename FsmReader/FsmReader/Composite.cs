using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FsmReader {
	public abstract class Composite {
		public abstract List<Composite> Children {
			get;
		}

		public bool Accept2(IVisitor visitor, Queue<Composite> queue) {
			Console.WriteLine("-------- Accept ------");
			foreach (Composite c in queue) {
				Console.WriteLine(((Treenode)c).Title);
			}
			Console.WriteLine("----------------------");

			if (visitor.VisitEnter(this)) {

				foreach (Composite c in Children) {
					if (!c.Accept2(visitor, queue)) {
						break;
					} else {
						queue.Enqueue(this);
					}
				}

			}
			bool ret = visitor.VisitExit(this);
			return ret;
		}

		static int acceptCount;

		public bool Accept(IVisitor visitor, ref Stack<Composite> stack) {
			Stack<Composite> inStack = new Stack<Composite>();
			acceptCount = 0;
			
			// Reverse the stack
			Stack<Composite> resumeStack = new Stack<Composite>(stack.AsEnumerable());
			// Remove root element
			if (resumeStack.Count > 0) resumeStack.Pop();

			bool result = Accept(visitor, inStack, resumeStack);

			stack = inStack;

			Console.WriteLine("--------------------------------");
			foreach (Composite c in stack) {
				Console.WriteLine(((Treenode)c).Title);
			}
			Console.WriteLine("--------------------------------");

			Console.WriteLine("AcceptCount = " + acceptCount);
			return result;
		}

		private bool Accept(IVisitor visitor, Stack<Composite> inStack, Stack<Composite> resumeStack) {
			acceptCount++;
			if (visitor.VisitEnter(this)) {
				inStack.Push(this);

				int firstChild = 0;

				if (resumeStack.Count > 0) {
					Composite top = resumeStack.Pop();
					if (resumeStack.Count == 0) {
						// Top was the result last time
						firstChild = Children.IndexOf(top) + 1;
					} else {
						top.Accept(visitor, inStack, resumeStack);
						firstChild = int.MaxValue;
					}
				}

				for (int i = firstChild; i < Children.Count; i++) {
					Composite c = Children[i];
					if (!c.Accept(visitor, inStack, resumeStack)) {
						break;
					}
				}
			}

			bool ret = visitor.VisitExit(this);
			if (ret) {
				inStack.Pop();
			}
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
