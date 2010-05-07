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
using System.Windows.Shapes;
using FsmReader;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for SearchDialog.xaml
	/// </summary>
	public partial class SearchDialog : Window {
		public SearchDialog() {
			InitializeComponent();
		}

		public SearchView view {
			get;
			set;
		}

		public SearchDialog(Treenode root) {
			InitializeComponent();

			RootNode = root;
			view = new SearchView(new DepthFirstSearch(root), SearchPredicate);
			this.DataContext = view;
		}

		public delegate void FoundNodeDelegate(Treenode node);

		public event FoundNodeDelegate FoundNode;

		#region Dependency Properties

		public Treenode RootNode {
			get { return (Treenode)GetValue(RootNodeProperty); }
			set { SetValue(RootNodeProperty, value); }
		}

		public static readonly DependencyProperty RootNodeProperty =
			DependencyProperty.Register("RootNode", typeof(Treenode), typeof(SearchDialog), new UIPropertyMetadata(null));

		public DataType DataType {
			get { return (DataType)GetValue(DataTypeProperty); }
			set { SetValue(DataTypeProperty, value); }
		}

		public static readonly DependencyProperty DataTypeProperty =
			DependencyProperty.Register("DataType", typeof(DataType), typeof(SearchDialog), new UIPropertyMetadata(DataType.None));
		
		public Flags Flags {
			get { return (Flags)GetValue(FlagsProperty); }
			set { SetValue(FlagsProperty, value); }
		}

		public static readonly DependencyProperty FlagsProperty =
			DependencyProperty.Register("Flags", typeof(Flags), typeof(SearchDialog), new UIPropertyMetadata(Flags.None));

		public FlagsExtended FlagsExtended {
			get { return (FlagsExtended)GetValue(FlagsExtendedProperty); }
			set { SetValue(FlagsExtendedProperty, value); }
		}

		public static readonly DependencyProperty FlagsExtendedProperty =
			DependencyProperty.Register("FlagsExtended", typeof(FlagsExtended), typeof(SearchDialog), new UIPropertyMetadata(FlagsExtended.None));

		public bool FindAllDataTypes {
			get { return (bool)GetValue(FindAllDataTypesProperty); }
			set { SetValue(FindAllDataTypesProperty, value); }
		}

		public static readonly DependencyProperty FindAllDataTypesProperty =
			DependencyProperty.Register("FindAllDataTypes", typeof(bool), typeof(SearchDialog), new UIPropertyMetadata(true));

		public bool FindAllFlags {
			get { return (bool)GetValue(FindAllFlagsProperty); }
			set { SetValue(FindAllFlagsProperty, value); }
		}

		public static readonly DependencyProperty FindAllFlagsProperty =
			DependencyProperty.Register("FindAllFlags", typeof(bool), typeof(SearchDialog), new UIPropertyMetadata(true));

		#endregion

		// These are copies of the state of the dependency properties at the time
		// the search was started. This is to prevent them from being accessed
		// from the wrong thread.
		private bool currentFindAllDataTypes;
		private bool currentFindAllFlags;
		private DataType currentDataType;
		private Flags currentFlags;
		private FlagsExtended currentFlagsExtended;

		/// <summary>
		/// This function evaluates Treenodes during a search in accordance with the criteria specified on this form.
		/// </summary>
		/// <param name="node">The Treenode that is currently being evaluated.</param>
		/// <returns>True if the Treenode matches the criteria specified on this form, false otherwise.</returns>
		public bool SearchPredicate(Treenode node) {
			if (!currentFindAllDataTypes && node.DataType != currentDataType) return false;

			if (!currentFindAllFlags) {
				if ((node.Flags & currentFlags) != currentFlags) return false;
				if ((node.FlagsExtended & currentFlagsExtended) != currentFlagsExtended) return false;
			}
			return true;
		}

		private void SearchCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.Handled = true;
			e.CanExecute = RootNode != null && !view.IsWorkerBusy;
		}

		private void SearchCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
			currentFindAllFlags = FindAllFlags;
			currentFindAllDataTypes = FindAllDataTypes;
			currentDataType = DataType;
			currentFlags = Flags;
			currentFlagsExtended = FlagsExtended;

			view.FindNext();
		}
	}
}
