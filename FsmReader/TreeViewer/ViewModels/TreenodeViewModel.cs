using System.ComponentModel;
using FsmReader;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using SmartWeakEvent;
using System.Windows;
using System;
using System.IO;

namespace TreeViewer {
	public class TreenodeViewModel : ViewModelBase {
		private ObservableCollection<TreenodeViewModel> children = new ObservableCollection<TreenodeViewModel>();
		private Treenode Treenode { get; set; }
		public TreenodeViewModel Parent { get; set; }
		public TreenodeViewModel(Treenode node, TreenodeViewModel parent) {
			this.Treenode = node;
			// TODO Ensure that TreenodeViews are destroyed when the Treenodes are in existance but the containing TreeView isn't
			this.Treenode.PropertyChanged += new PropertyChangedEventHandler(Treenode_PropertyChanged);
			this.Parent = parent;
			foreach (Treenode n in Treenode.Children.Cast<Treenode>()) {
				children.Add(new TreenodeViewModel(n, this));
			}
		}

		~TreenodeViewModel() {
			if (this.Treenode != null) {
				this.Treenode.PropertyChanged -= Treenode_PropertyChanged;
			}
		}

		public static Treenode GetTreenode(TreenodeViewModel vm) {
			return vm.Treenode;
		}

		// TODO Remove this duplication of code from Treenode.cs
		public static TreenodeViewModel NodeFromPath(string path, TreenodeViewModel relativeTo) {
			if (path == null) throw new ArgumentException("path");
			if (relativeTo == null) throw new ArgumentException("relativeTo");

			string[] parts = path.Split(new char[] { '/' });

			if (parts.Length < 1 || parts[1] != relativeTo.Title) {
				// The root node of the search differs
				return null;
			}

			TreenodeViewModel node = relativeTo;

			// Start on 2 because of the leading forward slash giving an empty string and the starting nodes being the same
			for (int p = 2; p < parts.Length && node != null; p++) {
				node = node.Children.FirstOrDefault(s => s.Title == parts[p]);
			}
			return node;
		}

		void Treenode_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case "Flags":
				case "FlagsExtended":
				case "DataType": propertyChangedEvent.Raise(this, new PropertyChangedEventArgs("IconPath")); break;
				case "Data":
				case "DataAsString":
				case "DataAsDouble": propertyChangedEvent.Raise(this, new PropertyChangedEventArgs("DataAsString")); break;
			}
		}

		public ObservableCollection<TreenodeViewModel> Children {
			get {
				return children;
			}
		}

		public string Title {
			get {
				return Treenode.Title;
			}
			set {
				Treenode.Title = value;
			}
		}

		public string DataAsString {
			get {
				return Treenode.DataAsString;
			}
			set {
				Treenode.DataAsString = value;
				propertyChangedEvent.Raise(this, new PropertyChangedEventArgs("DataAsString"));
			}
		}

		public Flags Flags {
			get {
				return Treenode.Flags;
			}
			set {
				Treenode.Flags = value;
			}
		}

		public FlagsExtended FlagsExtended {
			get {
				return Treenode.FlagsExtended;
			}
			set {
				Treenode.FlagsExtended = value;
			}
		}

		public DataType DataType {
			get {
				return Treenode.DataType;
			}
			set {
				Treenode.DataType = value;
			}
		}


		private bool isSelected;
		public bool IsSelected {
			get { 
				return isSelected; 
			}
			set {
				if (value != isSelected) {
					isSelected = value;
					FirePropertyChanged("IsSelected");
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
					propertyChangedEvent.Raise(this, new PropertyChangedEventArgs("IsExpanded"));
				}
			}
		}

		public string IconPath {
			get {
				if (Treenode.DataType == DataType.Object) {
					return "/Images/Object.png";
				} else if ((Treenode.Flags & Flags.CppFunc) == Flags.CppFunc) {
					return "/Images/CPP.png";
				} else if ((Treenode.FlagsExtended & FlagsExtended.DLLFunc) == FlagsExtended.DLLFunc) {
					return "/Images/DLL.png";
				} else if ((Treenode.FlagsExtended & FlagsExtended.GlobalCPPFunc) == FlagsExtended.GlobalCPPFunc) {
					return "/Images/GlobalCPP.png";
				} else if ((Treenode.FlagsExtended & FlagsExtended.Flexscript) == FlagsExtended.Flexscript) {
					return "/Images/Flexscript.png";
				} else {
					if (Treenode.NodeChildren.FirstOrDefault() != null) {
						return "/Images/Folder.png";
					} else {
						return "/Images/Default.png";
					}
				}
			}
		}

		#region INotifyPropertyChanged Members

		FastSmartWeakEvent<PropertyChangedEventHandler> propertyChangedEvent = new FastSmartWeakEvent<PropertyChangedEventHandler>();

		public event PropertyChangedEventHandler PropertyChanged {
			add {
				propertyChangedEvent.Add(value);
			}
			remove {
				propertyChangedEvent.Remove(value);
			}
		}

		#endregion

		public string FullPath {
			get {
				return Treenode.FullPath;
			}
		}

		public double DataAsDouble {
			get {
				return Treenode.DataAsDouble;
			}
			set {
				Treenode.DataAsDouble = value;
			}
		}

		internal bool RepresentsNode(FsmReader.Treenode node) {
			return Treenode == node;
		}

		internal static void Write(TreenodeViewModel rootNode, FileStream fs) {
			throw new NotImplementedException();
		}
	}
}