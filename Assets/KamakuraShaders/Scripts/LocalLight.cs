using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Kayac.VisualArts
{

	[ExecuteInEditMode]
	public class LocalLight : MonoBehaviour
	{

		public Quaternion totalRotationVector
		{
			get
			{
				if (relativeToMainLight && mainLightTransform != null)
				{
					return mainLightTransform.rotation * _rotationVector;
				}
				return _rotationVector;
			}
			set
			{
				if (relativeToMainLight && mainLightTransform != null)
				{
					_rotationVector = Quaternion.Inverse(mainLightTransform.rotation) * value;
				}
				else
				{
					_rotationVector = value;
				}
				rotation = _rotationVector.eulerAngles;
			}
		}


		[Header("Rotation")]
		public Vector3 rotation = Vector3.zero;
		public bool relativeToMainLight = true;
		public Transform mainLightTransform;



		[Header("Intensity")]
		[Range(0.0f, 1.0f)]
		public float globalLightIntensity = 1.0f;
		[Range(0.0f, 1.0f)]
		public float localLightIntensity = 1.0f;

		[Header("Other Options")]
		public bool useLocalLightForSpecular = false;
		public Color localLightColor = Color.white;

		public List<Renderer> renderers { get; private set; }

		[HideInInspector]
		[SerializeField]
		private Quaternion _rotationVector = Quaternion.identity;

		private Quaternion _appliedRotationVector;
		private Color _appliedLightColor = Color.black;
		private float _appliedLocalLightIntensity = -1.0f;
		private float _appliedGlobalLightIntensity = -1.0f;

		private bool _usedLocalLightForSpecular = false;

		const string LightVectorPropName = "_LocalLightVec";
		const string LightColorPropName = "_LocalLightColor";
		const string LightIntensityPropName = "_LocalLightIntensity";
		const string GlobalLightIntensityPropName = "_GlobalLightIntensity";
		const string UseLocalLightForSpecularPropName = "_LocalLightVecAsMain";

		MaterialPropertyBlock _tempPropBlock;
		bool _needsUpdate;

		void OnEnable()
		{
			if (_tempPropBlock == null)
			{
				_tempPropBlock = new MaterialPropertyBlock();
			}
			UpdateHierarchicalIntegrity();
		}

		void UpdateHierarchicalIntegrity()
		{
			renderers = new List<Renderer>();
			IncludeChildren(transform, renderers);
			ExcludeFromParent(transform, renderers);
			_needsUpdate = true;
			rotation = _rotationVector.eulerAngles;
		}

		void OnDisable()
		{
			TransferToParent(transform, renderers);
			renderers = null;
		}

		void IncludeChildren(Transform t, List<Renderer> renderers)
		{
			var renderer = t.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderers.Add(renderer);
			}
			foreach (Transform childT in t)
			{
				var childComponent = childT.GetComponent<LocalLight>();
				if (childComponent == null || childComponent.renderers == null || childComponent.renderers.Count == 0)
				{
					IncludeChildren(childT, renderers);
				}
			}
		}

		void ExcludeFromParent(Transform t, List<Renderer> renderers)
		{
			Transform parentT = t.parent;
			LocalLight parentComponent = null;
			while (parentT != null)
			{
				parentComponent = parentT.GetComponent<LocalLight>();
				if (parentComponent != null)
				{
					parentComponent.ExcludeRenderers(renderers);
				}
				parentT = parentT.parent;
			}
		}

		void TransferToParent(Transform t, List<Renderer> renderers)
		{
			Transform parentT = t.parent;
			LocalLight parentComponent = null;
			while (parentT != null && parentComponent == null)
			{
				parentComponent = parentT.GetComponent<LocalLight>();
				parentComponent = parentComponent != null && parentComponent.enabled ? parentComponent : null;
				parentT = parentT.parent;
			}
			if (parentComponent != null && parentComponent.IncludeRenderers(renderers)) {;}
			else
			{
				UseGlobalLight(renderers);
			}
		}

		bool IncludeRenderers(List<Renderer> renderers)
		{
			if (this.renderers != null)
			{
				this.renderers.AddRange(renderers);
				_needsUpdate = true;
				renderers.Clear();
				Update();
				return true;
			}

			return false;
		}

		void ExcludeRenderers(List<Renderer> renderers)
		{
			if (this.renderers != null)
			{
				this.renderers.RemoveAll(r => {
					var removeRenderer = renderers.Contains(r);
					return removeRenderer;
				});
			}
		}

		void UseGlobalLight(List<Renderer> renderers)
		{
			var rs = renderers.GetEnumerator();
			while (rs.MoveNext())
			{
				var renderer = rs.Current;
				_tempPropBlock.Clear();
				renderer.GetPropertyBlock(_tempPropBlock);
				_tempPropBlock.SetFloat(UseLocalLightForSpecularPropName, 0.0f);
				_tempPropBlock.SetFloat(GlobalLightIntensityPropName, 1.0f);
				_tempPropBlock.SetFloat(LightIntensityPropName, localLightIntensity);
				_tempPropBlock.SetVector(LightVectorPropName, Vector4.zero);
				_tempPropBlock.SetVector(LightColorPropName, Vector4.zero);
				renderer.SetPropertyBlock(_tempPropBlock);
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

		void SetMaterialsLightVector(Renderer renderer, float globalLightIntensity, Vector3 lightVector, Color lightColor, bool useLocalLightForSpecular)
		{
			_tempPropBlock.Clear();
			renderer.GetPropertyBlock(_tempPropBlock);
			_tempPropBlock.SetFloat(UseLocalLightForSpecularPropName, useLocalLightForSpecular ? 1.0f : 0.0f);
			_tempPropBlock.SetFloat(GlobalLightIntensityPropName, globalLightIntensity);
			_tempPropBlock.SetFloat(LightIntensityPropName, localLightIntensity);
			_tempPropBlock.SetVector(LightVectorPropName, lightVector);
			_tempPropBlock.SetVector(LightColorPropName, lightColor);
			renderer.SetPropertyBlock(_tempPropBlock);
		}

		public void Update ()
		{
			_rotationVector = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
			if (!_needsUpdate && totalRotationVector != _appliedRotationVector)
			{
				_needsUpdate = true;
			}

			if (!_needsUpdate && localLightColor != _appliedLightColor)
			{
				_needsUpdate = true;
			}

			if (!_needsUpdate && localLightIntensity != _appliedLocalLightIntensity)
			{
				_needsUpdate = true;
			}

			if (!_needsUpdate && globalLightIntensity != _appliedGlobalLightIntensity)
			{
				_needsUpdate = true;
			}

			if (!_needsUpdate && useLocalLightForSpecular != _usedLocalLightForSpecular)
			{
				_needsUpdate = true;
			}

			if (_needsUpdate)
			{
				_needsUpdate = false;
				Vector3 lightVector = totalRotationVector * Vector3.back;
				var renderers = this.renderers.GetEnumerator();
				while (renderers.MoveNext())
				{
					SetMaterialsLightVector(renderers.Current, globalLightIntensity, lightVector, localLightColor, useLocalLightForSpecular);
				}

				_appliedRotationVector = totalRotationVector;
				_appliedLightColor = localLightColor;
				_appliedLocalLightIntensity = localLightIntensity;
				_appliedGlobalLightIntensity = globalLightIntensity;
				_usedLocalLightForSpecular = useLocalLightForSpecular;
			}
		}
	}
}