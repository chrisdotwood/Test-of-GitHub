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

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for FsmTreeView.xaml
	/// </summary>
	public partial class FsmTreeView : UserControl {
		public FsmTreeView() {
			InitializeComponent();
		}

		public Treenode SelectedItem {
			get { return (Treenode)GetValue(SelectedItemProperty); }
			private set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(Treenode), typeof(FsmTreeView),
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
			if (tree.Items.Count == 0) return;

			Stack<Treenode> ancestors = new Stack<Treenode>();

			Treenode parent = target;
			while (parent != null) {
				ancestors.Push(parent);
				parent = parent.parent;
			}

			TreenodeView item = null;

			while (ancestors.Count > 0) {
				Treenode node = ancestors.Pop();

				ObservableCollection<TreenodeView> items;
				if (item == null) {
					items = new ObservableCollection<TreenodeView>() { (TreenodeView)tree.Items[0] };
				} else {
					items = item.Children;
				}

				foreach (TreenodeView n in items) {
					if (n.Treenode == node) {
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
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
			tree.ItemsSource = new ObservableCollection<TreenodeView> { new TreenodeView((Treenode)e.NewValue, null) };
		}

		private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (tree.SelectedItem == null) {
				SelectedItem = null;
			} else {
				SelectedItem = ((TreenodeView)tree.SelectedItem).Treenode;
			}
		}
	}

	public class TreenodeView : INotifyPropertyChanged {
		private ObservableCollection<TreenodeView> children = new ObservableCollection<TreenodeView>();

		public Treenode Treenode { get; set; }

		public TreenodeView Parent { get; set; }
		
		public TreenodeView(Treenode node, TreenodeView parent) {
			this.Treenode = node;
			this.Parent = parent;

			foreach (Treenode n in Treenode.NodeChildren.Cast<Treenode>()) {
				children.Add(new TreenodeView(n, this));
			}
		}

		public ObservableCollection<TreenodeView> Children {
			get {
				return children;
			}
		}

		public string Title {
			get {
				return Treenode.Title;
			}
		}

		public string DataAsString {
			get {
				string str = Treenode.DataAsString();
				int eol = str.IndexOf('\n');

				if (eol < 0) {
					return str;
				} else {
					return str.Substring(0, eol);
				}
			}
		}

		private bool isSelected = false;
		public bool IsSelected {
			get {
				return isSelected;
			}
			set {
				if (value != isSelected) {
					isSelected = value;
					if (PropertyChanged != null) {
						PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
					}
				}
			}
		}

		private bool isExpanded = false;
		public bool IsExpanded {
			get {
				return isExpanded;
			}
			set {
				if (value != isExpanded) {
					isExpanded = value;
					if (PropertyChanged != null) {
						PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded"));
					}
				}
			}
		}

		public string IconPath {
			get {
				if (Treenode.datatype == DataType.OBJECT) {
					return "Images/Object.png";
				} else if ((Treenode.flags & Flags.CPPFUNC) == Flags.CPPFUNC) {
					return "Images/CPP.png";
				} else if ((Treenode.flagsEx & FlagsExtended.DLLFUNC) == FlagsExtended.DLLFUNC) {
					return "Images/DLL.png";
				} else if ((Treenode.flagsEx & FlagsExtended.GLOBALCPPFUNC) == FlagsExtended.GLOBALCPPFUNC) {
					return "Images/GlobalCPP.png";
				} else if((Treenode.flagsEx & FlagsExtended.FLEXSCRIPT) == FlagsExtended.FLEXSCRIPT) {
					return "Images/Flexscript.png";
				} else {
					if(Treenode.NodeChildren.Count > 0) {
						return "Images/Folder.png";
					} else {
						return "Images/Default.png";
					}
				}
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

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
