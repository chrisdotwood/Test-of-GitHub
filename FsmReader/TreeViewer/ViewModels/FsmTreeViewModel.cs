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

		public TreenodeViewModel SelectedItem {
			get { return (TreenodeViewModel)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(TreenodeViewModel), typeof(FsmTreeViewModel),
			new UIPropertyMetadata() {
				DefaultValue = null,
				PropertyChangedCallback = new PropertyChangedCallback(SelectedItemPropertyChanged)
			});

		public ICommand SaveAsCommand {
			get;
			private set;
		}

		private static void SelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			Console.WriteLine();
			//FsmTreeView tv = (FsmTreeView)sender;
			//tv.RaiseEvent(new RoutedEventArgs(FsmTreeView.SelectedItemChangedEvent));
		}

		//public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
		//    "SelectedItemChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FsmTreeView));

		//public event RoutedEventHandler SelectedItemChanged {
		//    add {
		//        AddHandler(SelectedItemChangedEvent, value);
		//    }
		//    remove {
		//        RemoveHandler(SelectedItemChangedEvent, value);
		//    }
		//}

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



		public TreenodeViewModel RootNode {
			get { return (TreenodeViewModel)GetValue(RootNodeProperty); }
			set { SetValue(RootNodeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RootNode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RootNodeProperty =
			DependencyProperty.Register("RootNode", typeof(TreenodeViewModel), typeof(FsmTreeViewModel), new UIPropertyMetadata(null));

		//private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
		//    if (e.NewValue == null) {
		//        Tree.ItemsSource = null;
		//    } else {
		//        Tree.ItemsSource = new ObservableCollection<TreenodeView> { new TreenodeView((Treenode)e.NewValue, null) };
		//    }
		//}


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
