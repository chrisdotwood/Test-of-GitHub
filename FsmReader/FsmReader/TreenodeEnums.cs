using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FsmReader {

	/// <summary>
	/// Values taken from Flexsim C:\Program Files (x86)\Flexsim5\program\system\include\linklist.h
	/// </summary>
	public enum DataType : byte {
		None = 0,
		Float = 1,
		ByteBlock = 2,
		PointerCoupling = 3,
		Object = 4,
		Particle = 5,
		Undefined = 255
	}

	[Flags]
	public enum Flags : byte {
		None = 0x00,
		Expanded = 0x01,
		HasOwner = 0x02,
		CppFunc = 0x04,
		Selected = 0x08,
		HideConnectors = 0x10,
		HideLabel = 0x20,
		ExtendedFlags = 0x40,
		HasBranch = 0x80
	}

	[Flags]
	public enum FlagsExtended : uint {
		None = 0x0,
		ShowObject = 0x00000001,
		Selected = 0x00000002,
		Flexscript = 0x00000004,
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

}
