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
using FsmReader;
using System.Timers;
using System.Windows.Threading;

namespace TreeViewer {
	public class SearchViewModel : DependencyObject, INotifyPropertyChanged {
		private BackgroundWorker searchWorker;
		private DepthFirstSearch searchDpt;
		private Timer progressUpdateTimer = new Timer(1000);
		private Func<Treenode, bool> predicate;

		// TODO See if INotifyPropertyChanged can be removed
		public event PropertyChangedEventHandler PropertyChanged;

		public SearchViewModel(Treenode root) {
			RootNode = root;

			searchWorker = new BackgroundWorker() {
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = false
			};
			searchWorker.DoWork += new DoWorkEventHandler(SearchWorker_DoWork);
			searchWorker.ProgressChanged += new ProgressChangedEventHandler(SearchWorker_ProgressChanged);
			searchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(searchWorker_RunWorkerCompleted);

			progressUpdateTimer.Elapsed += new ElapsedEventHandler(ProgressUpdateTimer_Elapsed);

			SearchCommand = new _SearchCommand(this);

			searchDpt = new DepthFirstSearch(root);
			predicate = ((_SearchCommand) SearchCommand).SearchPredicate;
		}

		public ICommand                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        SearchCommand {
			get;
			private set;
		}

		private void ProgressUpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
			// TODO Remove hard coded node total test code
			if (searchWorker.IsBusy) {
				searchWorker.ReportProgress((100 * searchDpt.VisitCount) / 1000000);
			} else {
				progressUpdateTimer.Stop();
			}
		}

		private static void ResultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			Console.WriteLine();
		}

		#region Search Worker Event Handlers

		private void SearchWorker_DoWork(object sender, DoWorkEventArgs e) {
			progressUpdateTimer.Start();
			e.Result = searchDpt.FindNode(predicate);
		}

		private void searchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			progressUpdateTimer.Stop();

			this.Dispatcher.BeginInvoke(new Action(() => {
				ProgressPercentage = (100 * searchDpt.VisitCount) / 1000000;
				Result = e.Result as Treenode;
			}), null);

			// Update the state of the search button
			CommandManager.InvalidateRequerySuggested();
		}

		private void SearchWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			this.Dispatcher.BeginInvoke(new Action(() => {
				ProgressPercentage = e.ProgressPercentage;
			}), null);
		}

		#endregion

		#region Dependency Properties

		public double ProgressPercentage {
			get { return (double)GetValue(ProgressPercentageProperty); }
			set { SetValue(ProgressPercentageProperty, value); }
		}

		public static readonly DependencyProperty ProgressPercentageProperty =
			DependencyProperty.Register("ProgressPercentage", typeof(double), typeof(SearchViewModel), new UIPropertyMetadata(0.0));

		public Treenode Result {
			get { return (Treenode)GetValue(ResultProperty); }
			set { SetValue(ResultProperty, value); }
		}

		public static readonly DependencyProperty ResultProperty =
			DependencyProperty.Register("Result", typeof(Treenode), typeof(SearchViewModel), new UIPropertyMetadata() {
				DefaultValue = null,
				PropertyChangedCallback = new PropertyChangedCallback(ResultChanged)
			});

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

		#region Private Command Classes

		private class _SearchCommand : ICommand {
			public event EventHandler CanExecuteChanged;
			private SearchViewModel svm;
			private bool lastCanExecute;

			public _SearchCommand(SearchViewModel parent) {
				svm = parent;
			}


			// These are copies of the state of the dependency properties at the time
			// the search was started. This is to prevent them from being accessed
			// from the wrong thread.
			private bool currentFindAllDataTypes;
			private bool currentFindAllFlags;
			private DataType currentDataType;
			private Flags currentFlags;
			private FlagsExtended currentFlagsExtended;

			public bool CanExecute(object parameter) {
				bool canExecute = !svm.searchWorker.IsBusy && svm.RootNode != null;

				if (canExecute != lastCanExecute) {
					lastCanExecute = canExecute;
					if (CanExecuteChanged != null) {
						CanExecuteChanged(this, null);
					}
				}
				return canExecute;
			}

			public void Execute(object parameter) {
				currentFindAllFlags = svm.FindAllFlags;
				currentFindAllDataTypes = svm.FindAllDataTypes;
				currentDataType = svm.DataType;
				currentFlags = svm.Flags;
				currentFlagsExtended = svm.FlagsExtended;

				svm.searchWorker.RunWorkerAsync();
			}

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
		}

		#endregion
	}
}
