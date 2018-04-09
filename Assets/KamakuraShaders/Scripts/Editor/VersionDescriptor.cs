using UnityEngine;


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
	}

}