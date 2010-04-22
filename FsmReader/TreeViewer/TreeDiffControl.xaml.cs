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
using System.ComponentModel;
using System.IO;
using FsmReader;
using ICSharpCode.AvalonEdit.Highlighting;
using Diff;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for TreeDiffControl.xaml
	/// </summary>
	public partial class TreeDiffControl : UserControl {
		public TreeDiffControl() {
			InitializeComponent();

			LeftCodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");
			RightCodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");

			loadFileWorker.DoWork += new DoWorkEventHandler(loadFileWorker_DoWork);
			loadFileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadFileWorker_RunWorkerCompleted);

			loadFileWorker2.DoWork += new DoWorkEventHandler(loadFileWorker_DoWork);
			loadFileWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadFileWorker_RunWorkerCompleted);

		}

		private class LoadFileWorkerArgument {
			public string Path;
			public FsmTreeView Target;
		}

		BackgroundWorker loadFileWorker = new BackgroundWorker();
		BackgroundWorker loadFileWorker2 = new BackgroundWorker();
		
		void loadFileWorker_DoWork(object sender, DoWorkEventArgs e) {
			LoadFileWorkerArgument arg = (LoadFileWorkerArgument) e.Argument;

			using (FileStream stream = new FileStream(arg.Path, FileMode.Open)) {
				e.Result = Treenode.Read(stream);
			}
			this.Dispatcher.BeginInvoke(new Action(() => {
				arg.Target.DataContext = e.Result;
			}), null);
		}

		void loadFileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Error != null) {
				MessageBox.Show("An error occurred whilst loading file: " + e.Error.Message, "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
		}

		private void LeftFsmTree_SelectedItemChanged(object sender, RoutedEventArgs e) {
			TreenodeView node = ((FsmTreeView)sender).SelectedItem;
			if (node == null) {
				LeftCodeText.Text = "";
				LeftTreePath.Text = "No Node Selected";
			} else {
				LeftCodeText.Text = node.Treenode.DataAsString();
				LeftTreePath.Text = node.Treenode.FullPath;

				Treenode rightRoot = (Treenode)RightFsmTree.tree.DataContext;
				Treenode rightNode = Treenode.NodeFromPath(node.Treenode.FullPath, rightRoot);
				if (rightNode != null) {
					RightFsmTree.SelectNode(rightNode);
					
					DiffCode();
				}
			}
		}

		private void DiffCode() {
		//	new Lcs().Diff(new DiffDocument(LeftCodeText.Text), new DiffDocument(RightCodeText.Text));

		}

		private void RightFsmTree_SelectedItemChanged(object sender, RoutedEventArgs e) {
			TreenodeView node = ((FsmTreeView)sender).SelectedItem;
			if (node == null) {
				RightCodeText.Text = "";
				RightTreePath.Text = "No Node Selected";
			} else {
				RightCodeText.Text = node.Treenode.DataAsString();
				RightTreePath.Text = node.Treenode.FullPath;
			}
		}

		private void OpenCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = e.Handled = true;
		}

		private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
			OpenTwoFilesDialog ofd = new OpenTwoFilesDialog();
			bool? result = ofd.ShowDialog();
			if (result.HasValue && result.Value == true) {
				loadFileWorker.RunWorkerAsync( new LoadFileWorkerArgument() {
					Path = ofd.LeftPath,
					Target = LeftFsmTree
				});
				loadFileWorker2.RunWorkerAsync(new LoadFileWorkerArgument() {
					Path = ofd.RightPath,
					Target = RightFsmTree
				});
			}
			e.Handled = true;
		}
	}
}
