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
using System.ComponentModel;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window {
		public Window1() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
		}

		//private void leftTree_SelectedItemChanged(object sender, RoutedEventArgs e) {
		//    TreenodeView node = ((FsmTreeView)sender).SelectedItem;
		//    if (node == null) {
		//        leftText.Text = "";
		//        leftTreePath.Text = "No Node Selected";

		//    } else {
		//        leftText.Text = node.Treenode.DataAsString();
		//        leftTreePath.Text = node.Treenode.FullPath;

		//        //select lines 3 to 5
		//        if (leftText.Document.LineCount > 5) {
		//            int start = leftText.Document.GetLineByNumber(3).Offset;
		//            int end = leftText.Document.GetLineByNumber(5).EndOffset;
		//            leftText.Select(start, end - start);
		//        }
		//    }
		//}

		Treenode cppNode = null;
		
		//private void NextCppNode_Click(object sender, RoutedEventArgs e) {
		//    cppNode = dpt.FindNode(s => (s.Flags & Flags.CppFunc) == Flags.CppFunc);
		//    //cppNode = Treenode.FindNodeWithFlags(left, cppNode, Flags.CPPFUNC);
			
		//    if (cppNode != null) {
		//        leftTree.SelectNode(cppNode);
		//    } 
		//}

		private void leftTreePath_MouseUp(object sender, MouseButtonEventArgs e) {
			//((TreeViewItem)leftTree.tree.SelectedItem).BringIntoView();
		}
	}
}
