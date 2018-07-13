using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Kayac.VisualArts
{

	public enum KSFlags : int
	{
		NonSelectable = 1,
		EnableFlag = 2,
		AutoRenderQueue = 3,
	}

	public class MaterialKSFlagsDecorator : MaterialPropertyDrawer
	{

		HashSet<string> _flags;
		static readonly Dictionary<KSFlags, string> _enumToDict;

		static MaterialKSFlagsDecorator()
		{
			_enumToDict = new Dictionary<KSFlags, string>(
				Enum.GetValues(typeof(KSFlags))
					.Cast<KSFlags>()
					.ToDictionary(k => k, v => v.ToString()),
				new KSFlagsComparer());
		}

		public MaterialKSFlagsDecorator(string oldSerializedName)
		{
			_flags = new HashSet<string>(oldSerializedName.Split('|'));
		}

		public bool IsFlagSet(KSFlags flag)
		{
			return _flags.Contains(_enumToDict[flag]);
		}

		public struct KSFlagsComparer : IEqualityComparer<KSFlags>
		{
			public bool Equals(KSFlags x, KSFlags y)
			{
				return x == y;
			}

			public int GetHashCode(KSFlags obj)
			{
				return (int)obj;
			}
		}

		public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) { }

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return 0;
		}
	}

}