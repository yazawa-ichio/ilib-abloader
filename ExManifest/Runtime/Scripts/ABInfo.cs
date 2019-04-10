using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib.AssetBundles.ExManifest
{
	[Serializable]
	public class ABInfo
	{
		public string Name;
		public string Hash;
		public uint CRC;
		public long Size;
		public int DepIndex = -1;
		public int TagIndex = -1;

		public ABInfo Duplicate()
		{
			ABInfo info = new ABInfo();
			info.Name = Name;
			info.Hash = Hash;
			info.CRC = CRC;
			info.Size = Size;
			info.DepIndex = DepIndex;
			info.TagIndex = TagIndex;
			return info;
		}

	}

	[Serializable]
	public class DepInfo
	{
		public int[] Deps;
	}

	[Serializable]
	public class TagInfo
	{
		public string[] Names;
	}

	[Serializable]
	public class RefInfo
	{
		public int Index;
		public string Id;
		public string AssetName;
	}

}
