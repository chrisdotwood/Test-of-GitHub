using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace FsmReader {
	#region Enums

	public enum DataType {
		FLOAT = 1,
		BYTEBLOCK = 2,
		POINTERCOUPLING = 3,
		OBJECT = 4,
		PARTICLE = 5
	}
	[Flags]
	public enum Flags {
		EXPANDED = 0x01,
		HASOWNER = 0x02,
		CPPFUNC = 0x04,
		SELECTED = 0x08,
		HIDECONNECTORS = 0x10,
		HIDELABEL = 0x20,
		EXTENDEDFLAGS = 0x40,
		EXTENDEDFLAGS_A = 0x80
	}

	[Flags]
	public enum FlagsExtended {
		SHOWOBJECT = 0x00000001,
		SELECTED = 0x00000002,
		FLEXSCRIPT = 0x00000004,
		NULL = 0x00000008,

		FUNCTIONDISABLED = 0x00000010,
		KEYWORD = 0x00000020,
		STATELOCKED = 0x00000040, // first used for port state-change masking
		HIDDEN = 0x00000080, // prevent user viewing

		PROTECTED = 0x00000100, // prevent user editing
		HIDESHAPE = 0x00000200,
		ODTDERIVATIVE = 0x00000400,
		HIDEBASE = 0x00000800,

		HIDECONTENT = 0x00001000,
		STATSTAG = 0x00002000,
		INDEXCACHE = 0x00004000,
		MAINTAINARRAY = 0x00008000,

		DLLFUNC = 0x00010000,
		CUSTOMDISPLAY = 0x00020000,
		GLOBALCPPFUNC = 0x00040000,
		EXECUTINGNOW = 0x00080000
	}

	#endregion

	public class Treenode : Composite {
		byte version;
		public Flags flags;
		public string Title {
			get;
			set;
		}
		public object data;

		public DataType datatype;
		public uint branch;
		public FlagsExtended flagsEx;
		public uint indexCache;
		public uint cppType;

		public uint size;

		public List<Treenode> dataChildren = new List<Treenode>();
		public List<Treenode> NodeChildren {
			get;
			private set;
		}

		public Treenode() {
			NodeChildren = new List<Treenode>();
		}

		public override List<Composite> Children {
			get {
				return NodeChildren.Cast<Composite>().ToList();
			}
		}

		public Treenode parent;

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

				Treenode up = parent;
				while (up != null) {
					path = "/" + up.Title + path;
					up = up.parent;
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
			string[] parts = path.Split(new char[] { '/' });

			Treenode node = relativeTo;

			for (int p = 0; p < parts.Length && node != null; p++) {
				node = relativeTo[parts[p]];
			}
			return node;
		}

		#region Read

		public static Treenode Read(Stream stream) {
			BinaryReader reader = new BinaryReader(stream);

			Treenode ret = new Treenode();

			ret.version = reader.ReadByte();
			ret.flags = (Flags)Enum.ToObject(typeof(DataType), reader.ReadByte());

			int len = reader.ReadInt32();
			if (len > 0) {
				ret.Title = new string(reader.ReadChars((int)len)).Replace("\0", "");
			}

			ret.datatype = (DataType)Enum.ToObject(typeof(DataType), reader.ReadUInt32());

			ret.branch = reader.ReadUInt32();

			if ((ret.flags & Flags.EXTENDEDFLAGS) == Flags.EXTENDEDFLAGS) {
				ret.flagsEx = (FlagsExtended)Enum.ToObject(typeof(FlagsExtended), reader.ReadUInt32());
			}

			if ((ret.flagsEx & FlagsExtended.ODTDERIVATIVE) == FlagsExtended.ODTDERIVATIVE) {
				ret.cppType = reader.ReadUInt32();
			}

			if ((ret.flagsEx & FlagsExtended.INDEXCACHE) == FlagsExtended.INDEXCACHE) {
				ret.indexCache = reader.ReadUInt32();
			}

			if (ret.datatype == DataType.BYTEBLOCK) {
				len = reader.ReadInt32();
				if (len > 0) {
					byte[] buf = new byte[len];
					reader.Read(buf, 0, len);

					ret.data = Encoding.UTF8.GetString(buf).Replace("\0", "");
				}
			}

			if (ret.datatype == DataType.FLOAT) {
				ret.data = reader.ReadDouble();
			}

			if (ret.datatype == DataType.POINTERCOUPLING) {
				ret.data = reader.ReadUInt32();
			}

			if (ret.branch > 0) {
				Treenode dataChild = Read(stream);

				uint dataNodesToRead = dataChild.size;

				while (dataNodesToRead-- > 0) {
					Treenode node = Read(stream);
					node.parent = ret;

					if (ret.datatype == DataType.OBJECT) {
						//ret.dataChildren.Add(node);
						ret.NodeChildren.Add(node);
					} else {
						ret.NodeChildren.Add(node);
					}
				}
			}

			if (ret.datatype == DataType.OBJECT) {
				Treenode dataChild = Read(stream);

				uint nodesToRead = dataChild.size;

				while (nodesToRead-- > 0) {
					Treenode node = Read(stream);
					node.parent = ret;

					if (ret.branch > 0) {
						//ret.dataChildren.Add(node);
						ret.NodeChildren.Add(node);
					} else {
						ret.NodeChildren.Add(node);
					}
				}
			} else if (ret.datatype == DataType.PARTICLE) {
				Console.WriteLine("ERROR: Unknown datatype: " + ret.datatype.ToString() + " at " + stream.Position);
			}

			if ((ret.flags & Flags.HASOWNER) != Flags.HASOWNER) {
				ret.size = reader.ReadUInt32();
			}
			return ret;
		}

		#endregion

	}
}
