using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;

namespace FsmReader {
	#region Enums

	public enum DataType {
		None = 0,
		Float = 1,
		ByteBlock = 2,
		PointerCoupling = 3,
		Object = 4,
		Particle = 5
	}
	[Flags]
	public enum Flags {
		Expanded = 0x01,
		HasOwner = 0x02,
		CppFunc = 0x04,
		Selected = 0x08,
		HideConnectors = 0x10,
		HideLabel = 0x20,
		ExtendedFlags = 0x40,
		ExtendedFlagsA = 0x80
	}

	[Flags]
	public enum FlagsExtended {
		ShowObject = 0x00000001,
		Selected = 0x00000002,
		FlexScript = 0x00000004,
		Null = 0x00000008,

		FunctionDisabled = 0x00000010,
		Keyword = 0x00000020,
		StateLocked = 0x00000040, // first used for port state-change masking
		Hidden = 0x00000080, // prevent user viewing

		Protected = 0x00000100, // prevent user editing
		HideShape = 0x00000200,
		ODTDerivative = 0x00000400,
		HideBase = 0x00000800,

		HideContent = 0x00001000,
		StatStag = 0x00002000,
		IndexCache = 0x00004000,
		MaintainArray = 0x00008000,

		DLLFunc = 0x00010000,
		CustomDisplay = 0x00020000,
		GlobalCPPFunc = 0x00040000,
		ExecutingNow = 0x00080000
	}

	#endregion

	public class Treenode : Composite {
		byte Version { get; set; }
		public Flags Flags { get; set; }
		public string Title {
			get;
			set;
		}
		private object data;

		public DataType DataType { get; set; }
		public uint Branch { get; set; }
		public FlagsExtended ExtendedFlags { get; set; }
		public uint IndexCache { get; set; }
		public uint CppType { get; set; }

		public uint Size { get; set; }

		//public List<Treenode> dataChildren = new List<Treenode>();
		public Collection<Treenode> NodeChildren {
			get;
			private set;
		}

		public Treenode() {
			NodeChildren = new Collection<Treenode>();
		}

		public override ReadOnlyCollection<Composite> Children {
			get {
				IList<Composite> c = NodeChildren.ToList<Composite>();
				return new ReadOnlyCollection<Composite>(c);
			}
		}

		public Treenode Parent { get; set; }

		public string DataAsString() {
			if (data != null) {
				return data.ToString();
			} else {
				return "";
			}
		}

		public string FullPath {
			get {
				string path = "/" + Title;

				Treenode up = Parent;
				while (up != null) {
					path = "/" + up.Title + path;
					up = up.Parent;
				}
				return path;
			}
		}

		public Treenode this[int index] {
			get {
				if (index >= NodeChildren.Count) return null;
				return NodeChildren[index];
			}
		}

		public Treenode this[string childName] {
			get {
				return NodeChildren.FirstOrDefault(s => s.Title == childName);
			}
		}

		/// <summary>
		/// Find a node from a path relative to a specified starting node.
		/// </summary>
		/// <param name="path">The path relative to the relativeTo node of the node to be returned.</param>
		/// <param name="relativeTo">The starting node for the search.</param>
		/// <returns>The Treenode if it is found, null otherwise.</returns>
		public static Treenode NodeFromPath(string path, Treenode relativeTo) {
			if (path == null) throw new ArgumentException("path");
			if(relativeTo == null) throw new ArgumentException("relativeTo");

			string[] parts = path.Split(new char[] { '/' });

			if (parts.Length < 1 || parts[1] != relativeTo.Title) {
				// The root node of the search differs
				return null;
			}

			Treenode node = relativeTo;

			// Start on 2 because of the leading forward slash giving an empty string and the starting nodes being the same
			for (int p = 2; p < parts.Length && node != null; p++) {
				node = node[parts[p]];
			}
			return node;
		}

		#region Read

		public static Treenode Read(Stream stream) {
			BinaryReader reader = new BinaryReader(stream);

			Treenode ret = new Treenode();

			ret.Version = reader.ReadByte();
			ret.Flags = (Flags)Enum.ToObject(typeof(DataType), reader.ReadByte());

			int len = reader.ReadInt32();
			if (len > 0) {
				ret.Title = new string(reader.ReadChars((int)len)).Replace("\0", "");
			}

			ret.DataType = (DataType)Enum.ToObject(typeof(DataType), reader.ReadUInt32());

			ret.Branch = reader.ReadUInt32();

			if ((ret.Flags & Flags.ExtendedFlags) == Flags.ExtendedFlags) {
				ret.ExtendedFlags = (FlagsExtended)Enum.ToObject(typeof(FlagsExtended), reader.ReadUInt32());
			}

			if ((ret.ExtendedFlags & FlagsExtended.ODTDerivative) == FlagsExtended.ODTDerivative) {
				ret.CppType = reader.ReadUInt32();
			}

			if ((ret.ExtendedFlags & FlagsExtended.IndexCache) == FlagsExtended.IndexCache) {
				ret.IndexCache = reader.ReadUInt32();
			}

			if (ret.DataType == DataType.ByteBlock) {
				len = reader.ReadInt32();
				if (len > 0) {
					byte[] buf = new byte[len];
					reader.Read(buf, 0, len);

					ret.data = Encoding.UTF8.GetString(buf).Replace("\0", "");
				}
			}

			if (ret.DataType == DataType.Float) {
				ret.data = reader.ReadDouble();
			}

			if (ret.DataType == DataType.PointerCoupling) {
				ret.data = reader.ReadUInt32();
			}

			if (ret.Branch > 0) {
				Treenode dataChild = Read(stream);

				uint dataNodesToRead = dataChild.Size;

				while (dataNodesToRead-- > 0) {
					Treenode node = Read(stream);
					node.Parent = ret;

					if (ret.DataType == DataType.Object) {
						//ret.dataChildren.Add(node);
						ret.NodeChildren.Add(node);
					} else {
						ret.NodeChildren.Add(node);
					}
				}
			}

			if (ret.DataType == DataType.Object) {
				Treenode dataChild = Read(stream);

				uint nodesToRead = dataChild.Size;

				while (nodesToRead-- > 0) {
					Treenode node = Read(stream);
					node.Parent = ret;

					if (ret.Branch > 0) {
						//ret.dataChildren.Add(node);
						ret.NodeChildren.Add(node);
					} else {
						ret.NodeChildren.Add(node);
					}
				}
			} else if (ret.DataType == DataType.Particle) {
				Console.WriteLine("ERROR: Unknown datatype: " + ret.DataType.ToString() + " at " + stream.Position);
			}

			if ((ret.Flags & Flags.HasOwner) != Flags.HasOwner) {
				ret.Size = reader.ReadUInt32();
			}
			return ret;
		}

		#endregion

	}
}
