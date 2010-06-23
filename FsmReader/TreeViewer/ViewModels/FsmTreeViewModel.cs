using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FsmReader;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;

namespace TreeViewer.ViewModels {
	public class FsmTreeViewModel : ViewModelBase {
		public FsmTreeViewModel() {
			SaveAsCommand = new RelayCommand(SaveAsCommandBinding_Executed);
		}

		private TreenodeViewModel selectedItem;
		public TreenodeViewModel SelectedItem {
			get { 
				return selectedItem; 
			}
			set {
				if (selectedItem != value) {
					selectedItem = value;
					FirePropertyChanged("SelectedItem");
				}
			}
		}


		public ICommand SaveAsCommand {
			get;
			private set;
		}

		private static void SelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			Console.WriteLine();
			//FsmTreeView tv = (FsmTreeView)sender;
			//tv.RaiseEvent(new RoutedEventArgs(FsmTreeView.SelectedItemChangedEvent));
		}

		public void SelectNode(Treenode target) {
			if (RootNode == null) return;

			Stack<Treenode> ancestors = new Stack<Treenode>();

			Treenode parent = target;
			while (parent != null) {
				ancestors.Push(parent);
				parent = parent.Parent;
			}

			TreenodeViewModel item = null;

			while (ancestors.Count > 0) {
				Treenode node = ancestors.Pop();

				ObservableCollection<TreenodeViewModel> items;
				if (item == null) {
					items = new ObservableCollection<TreenodeViewModel>() { RootNode };
				} else {
					items = item.Children;
				}

				foreach (TreenodeViewModel n in items) {
					if (n.RepresentsNode(node)) {
						item = n;
						if (item.Parent != null) {
							item.Parent.IsExpanded = true;
						}
						break;
					}
				}
				if (item == null) {
					throw new Exception("Cannot find node in tree");
				}
			}
			item.IsSelected = true;
			SelectedItem = item;
		}

		/// <summary>
		/// This is an <see cref="System.Collections.Generic.ObjectModel.ObservableCollection"/> containing only one item, the RootNode of this tree. This is
		/// used only as the item source when databinding a TreeView.
		/// </summary>
		public ObservableCollection<TreenodeViewModel> RootContainer {
			get {
				return rootContainer;
			}
		}

		private ObservableCollection<TreenodeViewModel> rootContainer = new ObservableCollection<TreenodeViewModel>();
		private TreenodeViewModel rootNode;
		public TreenodeViewModel RootNode {
			get {
				return rootNode;
			}
			set {
				if (rootNode != value) {
					if (rootContainer.Contains(rootNode)) {
						rootContainer.Remove(rootNode);
					}
					rootNode = value;
					rootContainer.Add(rootNode);

					FirePropertyChanged("RootNode");
				}
			}
		}

		#region Command Bindings

		private void SaveAsCommandBinding_Executed(object sender) {
			if (SelectedItem == null) {
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Tree (*.t)|*.t|Flexsim Model (*.fsm)|*fsm|All Files (*.*)|*.*";
			bool? result = sfd.ShowDialog();
			if (result.HasValue && result.Value) {
				try {
					using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create)) {
						TreenodeViewModel.Write(SelectedItem, fs);
					}
				} catch (Exception ex) {
					MessageBox.Show("An error occurred whilst saving the file:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		#endregion
	}
}
