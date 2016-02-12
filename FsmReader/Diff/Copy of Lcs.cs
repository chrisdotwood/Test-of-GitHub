using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diff {
	public class Lcs {
		private string[][] table = null;
		private Change lastChange = null;

		List<Change> changeList = new List<Change>();

		public List<Change> Diff(DiffDocument d1, DiffDocument d2) {
			table = new string[d1.Length][];

			for (int i = 0; i < d1.Length; i++) {
				table[i] = new string[d2.Length];
			}

			FindLcs(d1, d2);

			Diff(d1, d2, d1.Length - 1, d2.Length - 1);

			if (lastChange != null) {
				changeList.Add(lastChange);
			}

			table = null;
			return changeList;
		}

		private void Diff(DiffDocument r, DiffDocument c, int i, int j) {
			if (i > 0 && j > 0 && r[i] == c[j]) {
				Diff(r, c, i - 1, j - 1);
			} else {
				if (j > 0 && (i == 0 || table[i][j - 1].Length >= table[i - 1][j].Length)) {
					Diff(r, c, i, j - 1);

					if (lastChange != null && lastChange.Type != ChangeType.Add) {
						if (lastChange.EndPosition1 >= i - 1) {
							lastChange.Type = ChangeType.Change;
							lastChange.StartPosition1 = i;
						} else {
							if (lastChange != null) {
								changeList.Add(lastChange);
							}

							lastChange = new Change() { Type = ChangeType.Add, StartPosition1 = i, StartPosition2 = j };
						}
					} else if (lastChange == null || lastChange.EndPosition2 != -1 && j - lastChange.EndPosition2 > 1) {
						if (lastChange != null) {
							changeList.Add(lastChange);
						}

						lastChange = new Change() { Type = ChangeType.Add, StartPosition1 = i, StartPosition2 = j };
					}
					lastChange.TextAdded += c[j] + Environment.NewLine;
				} else if (i > 0 && (j == 0 || table[i][j - 1].Length < table[i - 1][j].Length)) {
					Diff(r, c, i - 1, j);

					if (lastChange != null && lastChange.Type != ChangeType.Remove) {
						if (lastChange.EndPosition2 >= j - 1) {
							lastChange.Type = ChangeType.Change;
							lastChange.StartPosition2 = j;
						} else {
							if (lastChange != null) {
								changeList.Add(lastChange);
							}

							lastChange = new Change() { Type = ChangeType.Remove, StartPosition1 = i };
							lastChange.StartPosition2 = j;
						}
					} else if (lastChange == null || lastChange.EndPosition1 != -1 && i - lastChange.EndPosition1 > 1) {
						if (lastChange != null) {
							changeList.Add(lastChange);
						}

						lastChange = new Change() { Type = ChangeType.Remove, StartPosition1 = i, StartPosition2 = j };
					}
					lastChange.TextDeleted += r[i] + Environment.NewLine;
				}

				if (lastChange != null) {
					lastChange.EndPosition1 = i;
					lastChange.EndPosition2 = j;
				}
			}
		}

		/// <summary>
		/// Find the longest common subsequence of lines across two documents. 
		/// </summary>
		/// <param name="d1">The original document.</param>
		/// <param name="d2">The new document.</param>
		/// <returns>A string containing the longest common subsequence of identical lines across two documents</returns>
		private string FindLcs(DiffDocument d1, DiffDocument d2) {
			if (d2.Length == 0 || d1.Length == 0) return string.Empty;

			if (table[d1.Length - 1][d2.Length - 1] != null) {
				return table[d1.Length - 1][d2.Length - 1];
			}

			string lcs;
			if (d1[d1.Length - 1] == (d2[d2.Length - 1])) {
				// Suppose that two sequences both end in the same element. To find their LCS, shorten each sequence by removing the last element, 
				// find the LCS of the shortened sequences, and to that LCS append the removed element.
				lcs = FindLcs(d1.Substring(0, d1.Length - 1), d2.Substring(0, d2.Length - 1)) + d1[d1.Length - 1];
			} else {
				// Suppose that the two sequences X and Y do not end in the same symbol. Then the LCS of X and Y is the longest sequence of LCS(Xn,Ym-1) and LCS(Xn-1,Ym).
				lcs = Longest(FindLcs(d1, d2.Substring(0, d2.Length - 1)), FindLcs(d1.Substring(0, d1.Length - 1), d2));
			}

			table[d1.Length - 1][d2.Length - 1] = lcs;
			return lcs;
		}

		#region Static Utility Methods

		/// <summary>
		/// Returns the longest of the two string s1 and s2.
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		private static string Longest(string s1, string s2) {
			return s1.Length > s2.Length ? s1 : s2;
		}

		#endregion
	}
}
