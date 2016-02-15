using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using FsmReader;
using System.Windows;
using Diff;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.ObjectModel;
using ComparisonTools;
using System.IO.Compression;

namespace TreeViewer.ViewModels {
    public class TreeDiffControlViewModel : ViewModelBase {
        BackgroundWorker loadFileWorker = new BackgroundWorker();
        BackgroundWorker loadFileWorker2 = new BackgroundWorker();

        public static RoutedCommand MatchScrolling = new RoutedCommand();

        FsmTreeViewModel leftFsmTree, rightFsmTree;

        public TreeDiffControlViewModel(FsmTreeViewModel l, FsmTreeViewModel r) {
            leftFsmTree = l;
            rightFsmTree = r;

            CommandBinding matchScrollingBinding = new CommandBinding(MatchScrolling,
                new ExecutedRoutedEventHandler(MatchScrollingCommandBinding_Executed),
                new CanExecuteRoutedEventHandler(MatchScrollingCommandBinding_CanExecute));

            CommandManager.RegisterClassCommandBinding(typeof(TreeDiffControl), matchScrollingBinding);

            loadFileWorker.DoWork += new DoWorkEventHandler(loadFileWorker_DoWork);
            loadFileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadFileWorker_RunWorkerCompleted);

            loadFileWorker2.DoWork += new DoWorkEventHandler(loadFileWorker_DoWork);
            loadFileWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadFileWorker_RunWorkerCompleted);

            SaveCommand = new RelayCommand(SaveCommandBinding_Executed);
            MergeToolCommand = new RelayCommand(MergeToolCommand_Executed);
            SearchCommand = new RelayCommand(SearchCommandBinding_Executed, new Predicate<object>((s) => LeftFsmTree != null));
            OpenCommand = new RelayCommand(OpenCommandBinding_Executed);
			NextDifferenceCommand = new RelayCommand(NextDifferenceCommandBinding_Executed, (s) => LeftFsmTree != null && RightFsmTree != null);
        }

        #region Load File Worker

		public FsmTreeViewModel RightFsmTree {
            get { 
				return rightFsmTree; 
			}
            set {
				if (rightFsmTree != value) {
					rightFsmTree = value;
					FirePropertyChanged("RightFsmTree");
				}
			}
        }

		public FsmTreeViewModel LeftFsmTree {
			get {
				return leftFsmTree;
			}
			set {
				if (leftFsmTree != value) {
					leftFsmTree = value;
					FirePropertyChanged("LeftFsmTree");
				}
			}
		}

        private class LoadFileWorkerArgument {
            public string Path;
            public FsmTreeViewModel Target;
        }

        void loadFileWorker_DoWork(object sender, DoWorkEventArgs e) {
            LoadFileWorkerArgument arg = (LoadFileWorkerArgument)e.Argument;

            // Free the memory from the existing trees
            UIAction(new Action(() => {
                if (arg.Target.RootNode != null) {
                    arg.Target.RootNode = null;
                    System.GC.Collect();
                }
            }));

            try {
                Treenode root = null;

                using (FileStream stream = new FileStream(arg.Path, FileMode.Open, FileAccess.Read)) {
					// TODO Validate preamble
					// Skip the first 0x48 bytes
					stream.Position = 0x48;

					using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress)) {
						//using (FileStream stream = new FileStream(@"C:\users\chris.wood\desktop\new folder\doublenode.t.unzipped", FileMode.Open)) {
						root = Treenode.Read(zipStream);
					}

                }

                if (root != null) {
                    UIAction(new Action(() => {
                        e.Result = new TreenodeViewModel(root, null);

                        arg.Target.RootNode = (TreenodeViewModel)e.Result;
                    }));
                }
            } catch (Exception ex) {
                UIAction(new Action(() => {
                    MessageBox.Show("Error loading file " + arg.Path + ": " + ex.Message, "Error Loading File", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }

        void loadFileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                MessageBox.Show("An error occurred whilst loading file: " + e.Error.Message, "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        #endregion

        #region Command Binding Event Handlers

        private void SearchCommandBinding_Executed(object sender) {
            SearchView d = new SearchView(LeftFsmTree.RootNode);
            d.ShowDialog();
        }

        private void MatchScrollingCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = e.Handled = true;
        }

        private void MatchScrollingCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            Console.WriteLine();
        }

        private void OpenCommandBinding_Executed(object sender) {
            OpenTwoFilesDialog ofd = new OpenTwoFilesDialog();
            bool? result = ofd.ShowDialog();
            if (result.HasValue && result.Value == true) {
                loadFileWorker.RunWorkerAsync(new LoadFileWorkerArgument() {
                    Path = ofd.LeftPath,
                    Target = leftFsmTree
                });

                LeftFilePath = ofd.LeftPath;
                loadFileWorker2.RunWorkerAsync(new LoadFileWorkerArgument() {
                    Path = ofd.RightPath,
                    Target = rightFsmTree
                });
                RightFilePath = ofd.RightPath;
            }
        }

        #region Properties

		private string leftFilePath;
        public string LeftFilePath {
			get {
				return leftFilePath;
			}
            set {
				if (leftFilePath != value) {
					leftFilePath = value;
					FirePropertyChanged("LeftFilePath");
				}
			}
        }

		private string rightFilePath;
		public string RightFilePath {
			get {
				return rightFilePath;
			}
			set {
				if (rightFilePath != value) {
					rightFilePath = value;
					FirePropertyChanged("RightFilePath");
				}
			}
		}

        #endregion

        public ICommand OpenCommand {
            get;
            private set;
        }

        public ICommand SaveCommand {
            get;
            private set;
        }

		public ICommand NextDifferenceCommand {
			get;
			private set;
		}

        public ICommand MergeToolCommand {
            get;
            private set;
        }

        public ICommand SearchCommand {
            get;
            private set;
        }


        private void SaveCommandBinding_Executed(object sender) {
            SaveFileDialog sfd = new SaveFileDialog();
            bool? result = sfd.ShowDialog();
            if (result.HasValue && result.Value) {
                // Add root node DP
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create)) {
                    TreenodeViewModel.Write(LeftFsmTree.RootNode, fs);
                }

            }
        }

        private void MergeToolCommand_Executed(object sender) {
            string mergeToolPath = TreeViewer.Properties.Settings.Default.MergeToolPath;

            if (!System.IO.File.Exists(mergeToolPath)) {
                MessageBox.Show("The merge tool is not configured", "Merge Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Write each of the code windows to a temporary file
            string leftTempPath = System.IO.Path.GetTempFileName() + ".cpp";
            string rightTempPath = System.IO.Path.GetTempFileName() + ".cpp";

            try {
                using (StreamWriter sw = new StreamWriter(new FileStream(leftTempPath, FileMode.OpenOrCreate))) {
                    sw.Write(leftFsmTree.SelectedItem.DataAsString);
                    sw.Close();
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(rightTempPath, FileMode.OpenOrCreate))) {
                    sw.Write(rightFsmTree.SelectedItem.DataAsString);
                    sw.Close();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                MessageBox.Show("Unable to write to temporary files: " + ex.Message, "Merge Tool", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Start the merge tool
            try {
                Process proc = new Process();

                proc.StartInfo.FileName = mergeToolPath;
                proc.StartInfo.Arguments = "\"" + leftTempPath + "\" " + "\"" + rightTempPath + "\"";
                proc.EnableRaisingEvents = true;
                proc.Exited += new EventHandler(MergeTool_Exited);

                proc.Start();

                MergeToolState state = new MergeToolState() {
                    LeftPath = leftTempPath,
                    RightPath = rightTempPath,
                    LeftTreenode = leftFsmTree.SelectedItem,
                    RightTreenode = rightFsmTree.SelectedItem
                };
                mergeToolEdits.Add(proc.Id, state);

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                MessageBox.Show("Unable to open merge tool: " + ex.Message, "Merge Tool", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        #endregion

        #region Text Editor Event Handlers

        //private void LeftCodeScrolled(object sender, ScrollChangedEventArgs args) {
        //    RightCodeText.ScrollToVerticalOffset(args.VerticalOffset);
        //}

        //private void LeftCodeText_TextChanged(object sender, EventArgs e) {
        //    HandleTextChanged(LeftFsmTree, LeftCodeText);
        //}

        //private void RightCodeText_TextChanged(object sender, EventArgs e) {
        //    HandleTextChanged(RightFsmTree, RightCodeText);
        //}

        #endregion

        #region Utility Methods

        //private void HandleTextChanged(FsmTreeView tree, TextEditor text) {
        //    if (tree.SelectedItem == null) return;

        //    TreenodeViewModel node = tree.SelectedItem;
        //    if (node.DataType == DataType.ByteBlock) {
        //        node.DataAsString = text.Text;
        //    } else if (node.DataType == DataType.Float) {
        //        double val;
        //        if (double.TryParse(text.Text, out val)) {

        //            text.Background = Brushes.White;
        //            node.DataAsDouble = val;
        //        } else {
        //            text.Background = Brushes.Maroon;
        //        }
        //    }
        //}

        private void DiffCode() {
            List<Change> changes = new Lcs().Diff(new DiffDocument(leftFsmTree.SelectedItem.DataAsString), new DiffDocument(rightFsmTree.SelectedItem.DataAsString));

            foreach (Change c in changes) {
                //if (c.Type == ChangeType.Remove) {
                //    int start = LeftCodeText.Document.GetLineByNumber(c.StartPosition1).Offset;
                //    int end = LeftCodeText.Document.GetLineByNumber(c.EndPosition1).Offset;
                //    LeftCodeText.Select(start, end - start);


                //    break;
                //}
                //Console.WriteLine(c.ToString());
                //Console.WriteLine("----------------------------------------------------------------------------");

            }
        }

        #endregion

		private DepthFirstSearch nextDiffSearch;
       
		private void NextDifferenceCommandBinding_Executed(object sender) {
			if (nextDiffSearch == null) {
				nextDiffSearch = new DepthFirstSearch(TreenodeViewModel.GetTreenode(LeftFsmTree.RootNode));
				
			}
			bool result = false;
			nextDiffSearch.FindNode((s) => {
				Treenode other = Treenode.NodeFromPath(s.FullPath, TreenodeViewModel.GetTreenode(RightFsmTree.RootNode));
				
				result = other == null || (other.DataType == DataType.ByteBlock && s.DataType == DataType.ByteBlock && other.DataAsString != s.DataAsString);

				if (result) {
					LeftFsmTree.SelectNode(s);
					if (other != null) {
						RightFsmTree.SelectNode(other);
						
						try {
							TextDiffTool diff = new TextDiffTool(Properties.Settings.Default.DiffToolPath);

							string tmpl = Path.GetTempFileName();
							string tmpr = Path.GetTempFileName();

							using (StreamWriter sw = new StreamWriter(tmpl)) {
								sw.WriteLine(LeftFsmTree.SelectedItem.DataAsString);
								sw.Flush();
							}
							using (StreamWriter sw = new StreamWriter(tmpr)) {
								sw.WriteLine(RightFsmTree.SelectedItem.DataAsString);
								sw.Flush();
							}

							List<Change> changes = diff.Diff(tmpl, tmpr);
						} catch (Exception ex) {
							MessageBox.Show("Error generating diff: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						}
					}
				}

				return result;
			});
			if (result == false) {
				MessageBox.Show("No more differences found", "No Differences", MessageBoxButton.OK);
				nextDiffSearch = null;
			}
		}
		
        Dictionary<int, MergeToolState> mergeToolEdits = new Dictionary<int, MergeToolState>();

        class MergeToolState {
            public string LeftPath;
            public string RightPath;

            public TreenodeViewModel LeftTreenode;
            public TreenodeViewModel RightTreenode;
        }


        private void MergeTool_Exited(object sender, EventArgs e) {
            Process proc = (Process)sender;

            if (mergeToolEdits.ContainsKey(proc.Id)) {
                MergeToolState state = mergeToolEdits[proc.Id];

                // Check to see if the text has been changed, if so replace it in the tree
                try {
                    string leftEdited = ReadTextFile(state.LeftPath);
                    if (leftEdited != state.LeftTreenode.DataAsString) {
                        state.LeftTreenode.DataAsString = leftEdited;
                    }
                    string rightEdited = ReadTextFile(state.RightPath);
                    if (rightEdited != state.RightTreenode.DataAsString) {
                        state.RightTreenode.DataAsString = rightEdited;
                    }
                } catch (Exception ex) {
                    MessageBox.Show("An error has occurred when loading the changes: " + ex.Message, "Merge Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string ReadTextFile(string path) {
            using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open))) {
                return sr.ReadToEnd();
            }
        }

	}
}
