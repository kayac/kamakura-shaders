
namespace Kayac.VisualArts
{

	public class VersionDescriptor
	{
		public readonly int major;
		public readonly int minor;
		public readonly int patch;

		public readonly string stringDescriptor;

		public VersionDescriptor(int major, int minor, int patch)
		{
			this.major = major;
			this.minor = minor;
			this.patch = patch;
			this.stringDescriptor = major + "." + minor + "." + patch;
		}

		override public string ToString()
		{
			return stringDescriptor;
		}
	}

}