using UnityEditor;
using UnityEngine;

namespace Kayac.VisualArts
{
	[InitializeOnLoad]
	public class ImportPackageMonitor
	{
		static ImportPackageMonitor()
		{
			AssetDatabase.importPackageCompleted += CheckImportedPackage;
		}

		static void CheckImportedPackage(string packageName)
		{
			if (packageName.Contains("KamakuraShaders") && EditorPrefs.GetInt("KamakuraShaders_Version", 0) < (int)VersionDescriptor.Instance)
			{
				PatchTool.CheckForPatch();
				EditorPrefs.SetInt("KamakuraShaders_Version", (int)VersionDescriptor.Instance);
			}
		}
	}
}