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
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for FsmTreeView.xaml
	/// </summary>
	public partial class FsmTreeView : UserControl {
		public FsmTreeView() {
			InitializeComponent();
		}

		public TreenodeView SelectedItem {
			get { return (TreenodeView)GetValue(SelectedItemProperty); }
			private set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(TreenodeView), typeof(FsmTreeView),
			new UIPropertyMetadata() {
				DefaultValue = null,
				PropertyChangedCallback = new PropertyChangedCallback(SelectedItemPropertyChanged)
			});

		private static void SelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			FsmTreeView tv = (FsmTreeView)sender;
			tv.RaiseEvent(new RoutedEventArgs(FsmTreeView.SelectedItemChangedEvent));
		}

		public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
			"SelectedItemChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FsmTreeView));

		public event RoutedEventHandler SelectedItemChanged {
			add {
				AddHandler(SelectedItemChangedEvent, value);
			}
			remove {
				RemoveHandler(SelectedItemChangedEvent, value);
			}
		}

		public void SelectNode(Treenode target) {
			if (Tree.Items.Count == 0) return;

			Stack<Treenode> ancestors = new Stack<Treenode>();

			Treenode parent = target;
			while (parent != null) {
				ancestors.Push(parent);
				parent = parent.Parent;
			}

			TreenodeView item = null;

			while (ancestors.Count > 0) {
				Treenode node = ancestors.Pop();

				ObservableCollection<TreenodeView> items;
				if (item == null) {
					items = new ObservableCollection<TreenodeView>() { (TreenodeView)Tree.Items[0] };
				} else {
					items = item.Children;
				}

				foreach (TreenodeView n in items) {
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

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if (e.NewValue == null) {
				Tree.ItemsSource = null;
			} else {
				Tree.ItemsSource = new ObservableCollection<TreenodeView> { new TreenodeView((Treenode)e.NewValue, null) };
			}
		}

		private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (Tree.SelectedItem == null) {
				SelectedItem = null;
			} else {
				SelectedItem = Tree.SelectedItem as TreenodeView;
			}
		}

		#region Command Bindings

		private void SaveAsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.Handled = true;
			e.CanExecute = SelectedItem != null;
		}

		private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SelectedItem == null) {
				e.Handled = true;
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Tree (*.t)|*.t|Flexsim Model (*.fsm)|*fsm|All Files (*.*)|*.*";
			bool? result = sfd.ShowDialog();
			if (result.HasValue && result.Value) {
				try {
					using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create)) {
						TreenodeView.Write(SelectedItem, fs);
					}
				} catch (Exception ex) {
					MessageBox.Show("An error occurred whilst saving the file:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			e.Handled = true;
		}

		#endregion
	}


	public class BooleanToVisibilityConverter : IValueConverter {
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if ((bool)value) {
				return Visibility.Visible;
			} else {
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
