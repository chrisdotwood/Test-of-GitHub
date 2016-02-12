using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Diff;

namespace ComparisonTools {
	public class TextDiffTool {
		private readonly int DiffTimeout = 5000;
		private readonly string invalidOutputMessage = "Invalid diff output";

		private string diffToolPath;

		public TextDiffTool(string diffPath) {
			diffToolPath = diffPath;

			if (!File.Exists(diffToolPath)) {
				throw new ArgumentException("The specified file does not exist: " + diffToolPath);
			}
		}

		public List<Change> Diff(string filePath1, string filePath2) {
			try {
				// `-b' Ignore changes in amount of white space
				// `-E' Ignore changes due to tab expansion
				string args = "-b -E \"" + filePath1 + "\" \"" + filePath2 + "\"";

				Process proc = new Process();
				proc.StartInfo.FileName = diffToolPath;
				proc.StartInfo.Arguments = args;

				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.UseShellExecute = false;

				proc.Start();

				if (!proc.WaitForExit(DiffTimeout)) {
					throw new Exception("Diff tool timed out comparing " + filePath1 + " with " + filePath2);
				}

				List<Change> changes = ProcessDiffOutput(proc.StandardOutput);
				return changes;
			} catch (Exception ex) {
				if (ex.Message == invalidOutputMessage) throw;
				else throw new Exception(invalidOutputMessage, ex);
			}
		}

		private List<Change> ProcessDiffOutput(StreamReader sr) {
			List<Change> changes = new List<Change>();

			while (!sr.EndOfStream) {
				string command = sr.ReadLine();

				Change change = ProcessCommand(command);

				int numLines;
				if (change.Type == ChangeType.Add || change.Type == ChangeType.Change) {
					numLines = (change.EndPosition2 - change.StartPosition2) + 1;

					change.TextAdded = ReadChangeText(sr, numLines);
				}

				// Remove the --- between added and removed text
				if (change.Type == ChangeType.Change) {
					if (sr.EndOfStream) throw new Exception(invalidOutputMessage);
					string seperator = sr.ReadLine();

					if (seperator != "---") throw new Exception(invalidOutputMessage);
				}

				if (change.Type == ChangeType.Remove || change.Type == ChangeType.Change) {
					numLines = (change.EndPosition1 - change.StartPosition1) + 1;

					change.TextDeleted = ReadChangeText(sr, numLines);
				}

				changes.Add(change);
			}

			return changes;
		}

		private string ReadChangeText(StreamReader sr, int numLines) {
			string text = "";
			while (numLines-- > 0) {
				if (sr.EndOfStream) throw new Exception(invalidOutputMessage);

				string line = sr.ReadLine();
				if (line.Length < 2) throw new Exception(invalidOutputMessage);

				text += line.Substring(2); // Remove the leading "> " or "< "
			}
			return text;
		}

		private Change ProcessCommand(string command) {
			Change change = new Change();

			int commandIndex = -1;
			for (int i = 0; i < command.Length; i++) {
				if (char.IsLetter(command[i])) {
					commandIndex = i;
				}
			}
			if (commandIndex == -1 || command.Length <= commandIndex) throw new Exception(invalidOutputMessage);

			switch (command[commandIndex]) {
				case 'd': change.Type = ChangeType.Remove; break;
				case 'c': change.Type = ChangeType.Change; break;
				case 'a': change.Type = ChangeType.Add; break;
				default: throw new Exception(invalidOutputMessage);
			}

			string left = command.Substring(0, commandIndex);
			string right = command.Substring(commandIndex + 1);

			ProcessRange(left, out change.StartPosition1, out change.EndPosition1);
			ProcessRange(right, out change.StartPosition2, out change.EndPosition2);

			return change;
		}

		private void ProcessRange(string str, out int start, out int end) {
			if (str.Contains(',')) {
				// This range is more than one line
				start = int.Parse(str.Split(new char[] { ',' })[0]);
				end = int.Parse(str.Split(new char[] { ',' })[1]);
			} else {
				// This range is only one line
				start = end = int.Parse(str);
			}
		}
	}
}
