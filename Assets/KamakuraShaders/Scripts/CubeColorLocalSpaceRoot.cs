using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kayac.VisualArts
{
	[ExecuteInEditMode]
	public class CubeColorLocalSpaceRoot : MonoBehaviour
	{

		private List<Renderer> _renderers;

		private Matrix4x4 _appliedLocalSpaceMatrix;
		const string LocalSpaceMatrixFlag = "_CubeColorUseLocalSpace";
		const string LocalSpaceMatrixRow0 = "_CubeColorLocalSpaceMatrixRow0";
		const string LocalSpaceMatrixRow1 = "_CubeColorLocalSpaceMatrixRow1";
		const string LocalSpaceMatrixRow2 = "_CubeColorLocalSpaceMatrixRow2";

		private Transform _trs;

		bool _needsUpdate;

		void OnEnable()
		{
			if (_trs == null)
			{
				_trs = transform;
			}
			UpdateHierarchicalIntegrity();
		}

		void UpdateHierarchicalIntegrity()
		{
			_renderers = new List<Renderer>();
			IncludeChildren(transform, _renderers);
			ExcludeFromParent(transform, _renderers);
			_needsUpdate = true;
		}

		void OnDisable()
		{
			TransferToParent(transform, _renderers);
			_renderers = null;
		}

		void IncludeChildren(Transform t, List<Renderer> renderers)
		{
			var renderer = t.GetComponent<Renderer>();
			if (renderer != null)
			{
				// Debug.Log(name + " Adds +" + renderer.name);
				renderers.Add(renderer);
			}
			foreach (Transform childT in t)
			{
				var childComponent = childT.GetComponent<CubeColorLocalSpaceRoot>();
				if (childComponent == null || childComponent._renderers == null || childComponent._renderers.Count == 0)
				{
					IncludeChildren(childT, renderers);
				}
			}
		}

		void ExcludeFromParent(Transform t, List<Renderer> renderers)
		{
			// Debug.Log(name + " ExcludeFromParent");
			Transform parentT = t.parent;
			CubeColorLocalSpaceRoot parentComponent = null;
			while (parentT != null)
			{
				parentComponent = parentT.GetComponent<CubeColorLocalSpaceRoot>();
				if (parentComponent != null)
				{
					parentComponent.ExcludeRenderers(renderers);
				}
				parentT = parentT.parent;
			}
		}

		void TransferToParent(Transform t, List<Renderer> renderers)
		{
			// Debug.Log(name + " TransferToParent");
			Transform parentT = t.parent;
			CubeColorLocalSpaceRoot parentComponent = null;
			while (parentT != null && parentComponent == null)
			{
				parentComponent = parentT.GetComponent<CubeColorLocalSpaceRoot>();
				parentComponent = parentComponent != null && parentComponent.enabled ? parentComponent : null;
				parentT = parentT.parent;
			}
			if (parentComponent != null && parentComponent.IncludeRenderers(renderers)) {;}
			else
			{
				UseGlobaSpace(renderers);
			}
		}

		bool IncludeRenderers(List<Renderer> renderers)
		{
			if (_renderers != null)
			{
				_renderers.AddRange(renderers);
				_needsUpdate = true;
				renderers.Clear();
				Update();
				return true;
			}

			return false;
		}

		void ExcludeRenderers(List<Renderer> renderers)
		{
			if (_renderers != null)
			{
				_renderers.RemoveAll(r => {
					var removeRenderer = renderers.Contains(r);
					// if (removeRenderer)
						// Debug.Log(name + " Removes -" + r.name);
					return removeRenderer;
				});
			}
		}

		void UseGlobaSpace(List<Renderer> renderers)
		{
			var rs = renderers.GetEnumerator();
			while (rs.MoveNext())
			{
				SetMaterialsLocalSpaceMatrix(rs.Current, Matrix4x4.identity);
			}
		}

		void OnTransformParentChanged()
		{
			UpdateHierarchicalIntegrity();
			if (!this.enabled)
			{
				OnDisable();
			}
		}

		void OnTransformChildrenChanged()
		{
			UpdateHierarchicalIntegrity();
			if (!this.enabled)
			{
				OnDisable();
			}
		}

		void SetMaterialsLocalSpaceMatrix(Renderer renderer, Matrix4x4 localSpaceMatrix, bool useLocalSpace = true)
		{
			var materials = renderer.sharedMaterials;
			if (materials == null)
			{
				return;
			}
			for (int i = 0; i < materials.Length; ++i)
			{
				if (materials[i] != null)
				{
					Vector3 row0 = localSpaceMatrix.GetRow(0);
					Vector3 row1 = localSpaceMatrix.GetRow(1);
					Vector3 row2 = localSpaceMatrix.GetRow(2);
					var mat = materials[i];
					mat.SetVector(LocalSpaceMatrixRow0, row0);
					mat.SetVector(LocalSpaceMatrixRow1, row1);
					mat.SetVector(LocalSpaceMatrixRow2, row2);
					mat.SetFloat(LocalSpaceMatrixFlag, useLocalSpace ? 1.0f : 0.0f);
				}
			}
		}


		public void Update ()
		{
			if (!_needsUpdate && _appliedLocalSpaceMatrix != _trs.localToWorldMatrix)
			{
				_needsUpdate = true;
			}

			if (_needsUpdate)
			{
				_needsUpdate = false;
				var renderers = _renderers.GetEnumerator();
				_appliedLocalSpaceMatrix = _trs.localToWorldMatrix;
				while (renderers.MoveNext())
				{
					SetMaterialsLocalSpaceMatrix(renderers.Current, _appliedLocalSpaceMatrix);
				}
			}
		}
	}

}

