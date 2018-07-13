using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Kayac.VisualArts
{
	public class PatchTool
	{
		[MenuItem("Kayac/Kamakura Shaders/Check For Patch")]
		public static void CheckForPatch()
		{
			var assetProvider = new AssetProvider();
			var patchType = typeof(IPatchModule);
			var patches = System.AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => patchType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
				.Select(p => System.Activator.CreateInstance(p, assetProvider) as IPatchModule)
				.OrderBy(p => p.toTargetVersion);

			var patchesToApply = patches.Where(p => p.IsPatchNeeded());
			var patchesCount = patchesToApply.Count();

			if (patchesCount > 0)
			{
				var doPatch = EditorUtility.DisplayDialog("Kamakura Shaders Patch is available", "Please make a backup of your project before applying this patch", "Apply Patch", "Cancel");
				if (doPatch)
				{
					foreach (var patch in patchesToApply)
					{
						patch.DoPatch();
					}
				}
			}
			else
			{
				EditorUtility.DisplayDialog("Info", "No patch needed", "Close");
			}
		}

		internal static float GetNumericVersion(float major, float minor, float patch)
		{
			return major * 100 * 100 + minor * 100 + patch;
		}

	}

	public class AssetProvider
	{
		public List<Material> materials { get; private set; }

		public AssetProvider()
		{
			var customEditorField = typeof(Shader).GetProperty("customEditor", BindingFlags.NonPublic | BindingFlags.Instance);
			var objParam = new object[0];

			var materialsPath = AssetDatabase.FindAssets("t:material")
				.Select(t => AssetDatabase.GUIDToAssetPath(t));

			materials = materialsPath
				.Select(p => AssetDatabase.LoadAssetAtPath<Material>(p))
				.Where(m => customEditorField.GetValue(m.shader, objParam).ToString().EndsWith("KamakuraShaderGUI")).ToList();
		}

		public void SaveAssets()
		{
			AssetDatabase.SaveAssets();
		}
	}
}