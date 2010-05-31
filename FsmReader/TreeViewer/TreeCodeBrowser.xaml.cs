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
using ICSharpCode.AvalonEdit.Highlighting;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for TreeCodeBrowser.xaml
	/// </summary>
	public partial class TreeCodeBrowser : UserControl {
		public TreenodeView RootNode {
			get { return (TreenodeView)GetValue(RootNodeProperty); }
			set { SetValue(RootNodeProperty, value); }
		}

		public TreenodeView CurrentNode {
			get;
			set;
		}

		// Using a DependencyProperty as the backing store for RootNode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RootNodeProperty =
			DependencyProperty.Register("RootNode", typeof(TreenodeView), typeof(TreeCodeBrowser),
			new UIPropertyMetadata() {
				DefaultValue = null,
				PropertyChangedCallback = new PropertyChangedCallback(RootNodePropertyChanged)
			});

		private static void RootNodePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			TreeCodeBrowser browser = (TreeCodeBrowser)sender;

			browser.FsmTree.DataContext = args.NewValue;
		}

		public TreeCodeBrowser() {
			InitializeComponent();

			CodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");
		}

		private void FsmTree_SelectedItemChanged(object sender, RoutedEventArgs e) {
			TreenodeView node = ((FsmTreeView)sender).SelectedItem;
			
			if (node == null) {
				//TreePath.Text = "No Node Selected";
				CurrentNode = null;
			} else {
				//TreePath.Text = node.FullPath;
				CurrentNode = node;
				//select lines 3 to 5
				//if (Tree.Document.LineCount > 5) {
				//    int start = Tree.Document.GetLineByNumber(3).Offset;
				//    int end = Tree.Document.GetLineByNumber(5).EndOffset;
				//    Tree.Select(start, end - start);
				//}
			}
		}

		private void CodeText_TextChanged(object sender, EventArgs e) {
			CurrentNode.DataAsString = CodeText.Text;
		}
	}
}
