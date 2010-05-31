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
	public class SearchViewModel : ViewModelBase {
		private BackgroundWorker searchWorker;
		private DepthFirstSearch searchDpt;
		private Timer progressUpdateTimer = new Timer(1000);
		private Func<Treenode, bool> predicate;

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
			predicate = ((_SearchCommand)SearchCommand).SearchPredicate;
		}

		public ICommand SearchCommand {
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

		#region Search Worker Event Handlers

		private void SearchWorker_DoWork(object sender, DoWorkEventArgs e) {
			// Update the state of the search button
			SearchCommand.CanExecute(null);

			progressUpdateTimer.Start();
			e.Result = searchDpt.FindNode(predicate);
		}

		private void searchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			progressUpdateTimer.Stop();

			ProgressPercentage = (100 * searchDpt.VisitCount) / 1000000;
			Result = e.Result as Treenode;

			// Update the state of the search button
			SearchCommand.CanExecute(null);
		}

		private void SearchWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			ProgressPercentage = e.ProgressPercentage;
		}

		#endregion

		#region Properties

		private double progressPercentage;
		public double ProgressPercentage {
			get {
				return progressPercentage;
			}
			set {
				progressPercentage = value;
				OnPropertyChanged("ProgressPercentage");
			}
		}

		private Treenode result;
		public Treenode Result {
			get {
				return result;
			}
			set {
				result = value;
				OnPropertyChanged("Result");
				CommandManager.InvalidateRequerySuggested();
			}
		}

		public Treenode RootNode {
			get;
			set;
		}
		public DataType DataType {
			get;
			set;
		}

		public Flags Flags {
			get;
			set;
		}

		public FlagsExtended FlagsExtended {
			get;
			set;
		}

		private bool findAllDataTypes = true;
		public bool FindAllDataTypes {
			get {
				return findAllDataTypes;
			}
			set {
				findAllDataTypes = value;
				OnPropertyChanged("FindAllDataTypes");
			}
		}

		public bool FindAllFlags {
			get;
			set;
		}

		#endregion

		#region Private Command Classes

		private class _SearchCommand : ICommand {
			public event EventHandler CanExecuteChanged;
			private SearchViewModel svm;
			private bool lastCanExecute;

			public _SearchCommand(SearchViewModel parent) {
				svm = parent;
			}

			public bool CanExecute(object parameter) {
				bool canExecute = !svm.searchWorker.IsBusy && svm.RootNode != null;

				if (canExecute != lastCanExecute) {
					lastCanExecute = canExecute;
					if (CanExecuteChanged != null) {
						svm.UIAction(() => CanExecuteChanged(this, new EventArgs()));
					}
				}
				return canExecute;
			}

			public void Execute(object parameter) {
				svm.FindAllFlags = svm.FindAllFlags;
				svm.FindAllDataTypes = svm.FindAllDataTypes;
				svm.DataType = svm.DataType;
				svm.Flags = svm.Flags;
				svm.FlagsExtended = svm.FlagsExtended;

				svm.searchWorker.RunWorkerAsync();
			}

			/// <summary>
			/// This function evaluates Treenodes during a search in accordance with the criteria specified on this form.
			/// </summary>
			/// <param name="node">The Treenode that is currently being evaluated.</param>
			/// <returns>True if the Treenode matches the criteria specified on this form, false otherwise.</returns>
			public bool SearchPredicate(Treenode node) {
				if (!svm.FindAllDataTypes && node.DataType != svm.DataType) return false;

				if (!svm.FindAllFlags) {
					if ((node.Flags & svm.Flags) != svm.Flags) return false;
					if ((node.FlagsExtended & svm.FlagsExtended) != svm.FlagsExtended) return false;
				}
				return true;
			}
		}

		#endregion
	}
}
