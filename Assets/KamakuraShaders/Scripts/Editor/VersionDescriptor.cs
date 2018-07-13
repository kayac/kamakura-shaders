using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;

namespace Kayac.VisualArts
{
	public class VersionDescriptor : ScriptableObject
	{

		[SerializeField]
		[HideInInspector]
		private int _major;

		[SerializeField]
		[HideInInspector]
		private int _minor;

		[SerializeField]
		[HideInInspector]
		private int _patch;


		[SerializeField]
		[HideInInspector]
		private string _stringDescriptor;


		public int major { get { return _major; } }
		public int minor { get { return _minor; } }
		public int patch { get { return _patch; } }

		public string stringDescriptor { get { return _stringDescriptor; } }

		public void SetVersion(int major, int minor, int patch)
		{
			_major = major;
			_minor = minor;
			_patch = patch;
			_stringDescriptor = major + "." + minor + "." + patch;
		}

		override public string ToString()
		{
			return stringDescriptor;
		}

		public static explicit operator int(VersionDescriptor v)
		{
			return (v._major << 14) + (v._minor << 7) + v._patch;
		}

		public static explicit operator VersionDescriptor(int n)
		{
			var v = ScriptableObject.CreateInstance<VersionDescriptor>();
			v.SetVersion(n >> 14, (n >> 7) & ((1 << 7) - 1), n & ((1 << 7) - 1));
			return v;
		}

		public static int ConvertToSingleInt(int major, int minor, int patch)
		{
			return (major << 14) + (minor << 7) + patch;
		}

		private static VersionDescriptor s_instance;

		public static VersionDescriptor Instance
		{
			get
			{
				if (s_instance == null)
				{
					var verGuids = AssetDatabase.FindAssets("t:kayac.visualarts.versiondescriptor");
					if (verGuids.Length == 1)
					{
						var path = AssetDatabase.GUIDToAssetPath(verGuids[0]);
						s_instance = AssetDatabase.LoadAssetAtPath<VersionDescriptor>(path);
					}
					else
					{
						throw new System.InvalidProgramException("Could not find Kamakura Shaders Version Descriptor asset. Try to re-import the package.");
					}
				}
				return s_instance;
			}
		}
	}

}