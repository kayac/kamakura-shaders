using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Kayac.VisualArts
{
	public interface IPatchModule
	{
		int fromTargetVersion { get; }
		int toTargetVersion { get; }
		AssetProvider assetProvider { get; }
		int patchNumber { get; }
		bool IsPatchNeeded();
		bool DoPatch();
	}

	public abstract class BasePatchModule : IPatchModule
	{
		public abstract int fromTargetVersion { get; }
		public abstract int toTargetVersion { get; }
		public AssetProvider assetProvider { get; private set; }
		public abstract int patchNumber { get; }

		public abstract bool DoPatch();
		public abstract bool IsPatchNeeded();

		private const int PatchCountPerElement = 22;
		private const int PatchCountPerInt = 2 * PatchCountPerElement;
		private const string PatchDataField = "_PatchData";

		public BasePatchModule(AssetProvider assetProvider)
		{
			this.assetProvider = assetProvider;
		}

		protected bool IsMaterialVersionMatched(Material material)
		{
			var materialVersion = material.GetVector("_ShaderVersion");
			var materialVersionNum = VersionDescriptor.ConvertToSingleInt((int)materialVersion.x, (int)materialVersion.y, (int)materialVersion.z);
			return (materialVersionNum >= fromTargetVersion && materialVersionNum <= toTargetVersion);
		}

		protected bool IsMaterialPatched(Material material)
		{
			Vector4 patchData = material.GetVector(PatchDataField);
			if (patchNumber > PatchCountPerInt)
			{
				var higherPatch = (int)patchData.z | ((int)patchData.w << PatchCountPerElement);
				return (higherPatch & (1 << (patchNumber + PatchCountPerInt - 1))) != 0;
			}
			else
			{
				var lowerPatch = (int)patchData.x | ((int)patchData.y << PatchCountPerElement);
				return (lowerPatch & (1 << (patchNumber - 1))) != 0;
			}
		}

		protected void WritePatchFlag(Material material)
		{
			Vector4 patchData = material.GetVector(PatchDataField);
			var newPatchData = UpdatePatchFlag(patchData);
			material.SetVector(PatchDataField, newPatchData);
		}

		private Vector4 UpdatePatchFlag(Vector4 patchData)
		{
			var higherPatch = (int)patchData.z | ((int)patchData.w << PatchCountPerElement);
			var lowerPatch = (int)patchData.x | ((int)patchData.y << PatchCountPerElement);

			if (patchNumber > PatchCountPerInt)
			{
				higherPatch = higherPatch | (1 << (patchNumber - PatchCountPerInt - 1));
			}
			else
			{
				lowerPatch = lowerPatch | (1 << (patchNumber - 1));
			}
			patchData.x = lowerPatch & ((1 << PatchCountPerElement) - 1);
			patchData.y = lowerPatch >> PatchCountPerElement;
			patchData.z = higherPatch & ((1 << PatchCountPerElement) - 1);;
			patchData.w = higherPatch >> PatchCountPerElement;

			return patchData;
		}
	}

	public sealed partial class PatchModules
	{
		private class PatchModule01 : BasePatchModule
		{
			private int _fromTargetVersion;
			public override int fromTargetVersion { get { return _fromTargetVersion; } }
			private int _toTargetVersion;
			public override int toTargetVersion { get { return _toTargetVersion; } }
			public override int patchNumber { get { return 1; } }

			public PatchModule01(AssetProvider assetProvider) : base(assetProvider)
			{
				_fromTargetVersion = VersionDescriptor.ConvertToSingleInt(0, 0, 0);
				_toTargetVersion = VersionDescriptor.ConvertToSingleInt(1, 0, 7);
			}

			List<Material> _targetMaterials;

			bool IsApplicable(Material material)
			{
				if (!IsMaterialVersionMatched(material)) { return false; }
				var hasPatched = IsMaterialPatched(material);
				var hasTargetKeywords = !hasPatched && material.HasProperty("_LocalLightVec");
				var hasNonDefaultValue = hasTargetKeywords && HasNonDefaultValue(material);
				return hasNonDefaultValue;
			}

			bool HasNonDefaultValue(Material material)
			{
				return material.GetVector("_LocalLightVec") != new Vector4(1, 0, 0, 0)
					|| material.GetColor("_LocalLightColor") != new Color(0, 0, 0, 0)
					|| ((int)material.GetFloat("_GlobalLightIntensity")) != 1
					|| ((int)material.GetFloat("_LocalLightIntensity")) != 0
					|| ((int)material.GetFloat("_LocalLightVecAsMain")) != 0
					|| ((int)material.GetFloat("_CubeColorUseLocalSpace")) != 0;
			}

			override public bool IsPatchNeeded()
			{
				_targetMaterials = assetProvider.materials.Where(m => IsApplicable(m)).ToList();
				return _targetMaterials != null && _targetMaterials.Count > 0;
			}

			override public bool DoPatch()
			{
				foreach (var material in _targetMaterials)
				{
					material.SetVector("_LocalLightVec", new Vector4(1, 0, 0, 0));
					material.SetColor("_LocalLightColor", new Color(0, 0, 0, 0));
					material.SetFloat("_GlobalLightIntensity", 1);
					material.SetFloat("_LocalLightIntensity", 0);
					material.SetFloat("_LocalLightVecAsMain", 0);
					material.SetFloat("_CubeColorUseLocalSpace", 0);
					WritePatchFlag(material);
					Debug.Log("Patched " + material.name);
				}
				assetProvider.SaveAssets();
				return true;
			}
		}
	}

}