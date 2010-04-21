using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FsmReader;
using System.IO;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit.Highlighting;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window {
		public Window1() {
			InitializeComponent();
		}

		Treenode left = null;
		Treenode right = null;
		DepthFirstSearch dpt;

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			leftText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");

			OpenFileDialog ofd = new OpenFileDialog();
			bool? result = ofd.ShowDialog();
			if (result.HasValue && result.Value == true) {
				using (FileStream stream = new FileStream(ofd.FileName, FileMode.Open)) {
					left = Treenode.Read(stream);

					leftTree.DataContext = left;
					dpt = new DepthFirstSearch(left);
				}
			} else {
				Close();
				return;
			}

			//result = ofd.ShowDialog();
			//if (result.HasValue && result.Value == true) {
			//    using (FileStream stream = new FileStream(ofd.FileName, FileMode.Open)) {
			//        right = Treenode.Read(stream);

			//        rightTree.DataContext = right;
			//    }
			//} else {
			//    Close();
			//    return;
			//}
		}

		private void leftTree_SelectedItemChanged(object sender, RoutedEventArgs e) {
			Treenode node = ((FsmTreeView)sender).SelectedItem;
			leftText.Text = node.DataAsString();
			leftTreePath.Text = node.FullPath;

			//select lines 3 to 5
			if (leftText.Document.LineCount > 5) {
				int start = leftText.Document.GetLineByNumber(3).Offset;
				int end = leftText.Document.GetLineByNumber(5).EndOffset;
				leftText.Select(start, end - start);
			}
		}

		private void rightTree_SelectedItemChanged(object sender, RoutedEventArgs e) {
			
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			TreeView view = leftTree.tree;
			
			TreeViewItem model = (TreeViewItem)view.Items[view.Items.Count - 1];
			
			TreeViewItem target = (TreeViewItem)model.Items[model.Items.Count - 1];
			target.BringIntoView();
			target.IsSelected = true;
		}

		Treenode cppNode = null;
		
		private void NextCppNode_Click(object sender, RoutedEventArgs e) {
			cppNode = dpt.FindNode(s => (s.flags & Flags.CPPFUNC) == Flags.CPPFUNC);
			//cppNode = Treenode.FindNodeWithFlags(left, cppNode, Flags.CPPFUNC);
			
			if (cppNode != null) {
				leftTree.SelectNode(cppNode);
				//leftText.Text = cppNode.DataAsString();
			}
		}

		private void leftTreePath_MouseUp(object sender, MouseButtonEventArgs e) {
			//((TreeViewItem)leftTree.tree.SelectedItem).BringIntoView();
		}
	}
}
