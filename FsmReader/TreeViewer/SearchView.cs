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
	public class SearchView : DependencyObject, INotifyPropertyChanged {
		
		private BackgroundWorker searchWorker;
		private DepthFirstSearch searchDpt;
		private Timer progressUpdateTimer = new Timer(1000);
		private Func<Treenode, bool> predicate;

		public SearchView(DepthFirstSearch dpt, Func<Treenode, bool> searchPredicate) {
			searchDpt = dpt;
			predicate = searchPredicate;

			searchWorker = new BackgroundWorker() {
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = false
			};
			searchWorker.DoWork += new DoWorkEventHandler(SearchWorker_DoWork);
			searchWorker.ProgressChanged += new ProgressChangedEventHandler(SearchWorker_ProgressChanged);
			searchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(searchWorker_RunWorkerCompleted);
			
			progressUpdateTimer.Elapsed += new ElapsedEventHandler(ProgressUpdateTimer_Elapsed);
		}

		// TODO Think about replacing the current search mechanism with a command.
		public bool IsWorkerBusy {
			get {
				return searchWorker.IsBusy;
			}
		}

		public void FindNext() {
			searchWorker.RunWorkerAsync();
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
			progressUpdateTimer.Start();
			e.Result = searchDpt.FindNode(predicate);
		}

		private void searchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			progressUpdateTimer.Stop();

			this.Dispatcher.BeginInvoke(new Action(() => {
				ProgressPercentage = (100 * searchDpt.VisitCount) / 1000000;
				Result = e.Result as Treenode;
			}), null);
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
			DependencyProperty.Register("ProgressPercentage", typeof(double), typeof(SearchView), new UIPropertyMetadata(0.0));

		public Treenode Result {
			get { return (Treenode)GetValue(ResultProperty); }
			set { SetValue(ResultProperty, value); }
		}

		public static readonly DependencyProperty ResultProperty =
			DependencyProperty.Register("Result", typeof(Treenode), typeof(SearchView), new UIPropertyMetadata() {
				DefaultValue = null,
				PropertyChangedCallback = new PropertyChangedCallback(ResultChanged)
			});

		#endregion

		private static void ResultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			Console.WriteLine();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
