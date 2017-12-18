using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using System.IO;


namespace Kayac.VisualArts
{

	public class ShaderReflectionHelper
	{

		private MethodInfo getHandlerMethod;
		private FieldInfo propDrawerInfo;
		private FieldInfo decoratorDrawerInfo;

		public ShaderReflectionHelper()
		{
			var materialPropHandlerType = System.Type.GetType("UnityEditor.MaterialPropertyHandler, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);
			getHandlerMethod = materialPropHandlerType.GetMethod("GetHandler", BindingFlags.NonPublic | BindingFlags.Static);
		}

		public void ApplyMaterialPropertyHandler(MaterialProperty matProp, Shader shader)
		{
			var drawers = GetDrawers(matProp, shader);
			if (drawers == null)
			{
				return;
			}

			foreach (var drawer in drawers)
			{
				Debug.Log("Applying " + drawer.GetType().Name);
				drawer.Apply(matProp);
			}
		}

		private object GetCachedHandler(MaterialProperty matProp, Shader shader)
		{
			return getHandlerMethod.Invoke(null, new object[] {shader, matProp.name});
		}


		public MaterialPropertyDrawer GetDrawer(MaterialProperty matProp, Shader shader, string decoratorName = null)
		{

			var handler = GetCachedHandler(matProp, shader);

			if (handler == null)
			{
				return null;
			}

			if (propDrawerInfo == null)
			{
				var handlerType = handler.GetType();
				propDrawerInfo = handlerType.GetField("m_PropertyDrawer", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			if (decoratorDrawerInfo == null)
			{
				var handlerType = handler.GetType();
				decoratorDrawerInfo = handlerType.GetField("m_DecoratorDrawers", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			if (decoratorName == null)
			{
				var propDrawer = propDrawerInfo.GetValue(handler) as MaterialPropertyDrawer;
				return propDrawer;
			}
			else
			{
				var decoDrawers = decoratorDrawerInfo.GetValue(handler) as List<MaterialPropertyDrawer>;
				if (decoDrawers == null)
				{
					return null;
				}
				return decoDrawers.FirstOrDefault(t => t.GetType().ToString().Contains(decoratorName));
			}
		}



		public List<MaterialPropertyDrawer> GetDrawers(MaterialProperty matProp, Shader shader)
		{
			var handler = GetCachedHandler(matProp, shader);

			if (handler == null)
			{
				return null;
			}

			if (propDrawerInfo == null)
			{
				var handlerType = handler.GetType();
				propDrawerInfo = handlerType.GetField("m_PropertyDrawer", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			if (decoratorDrawerInfo == null)
			{
				var handlerType = handler.GetType();
				decoratorDrawerInfo = handlerType.GetField("m_DecoratorDrawers", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			var result = new List<MaterialPropertyDrawer>();

			var drawer = propDrawerInfo.GetValue(handler) as MaterialPropertyDrawer;
			if (drawer != null)
			{
				result.Add(drawer);
			}

			var decoDrawers = decoratorDrawerInfo.GetValue(handler) as List<MaterialPropertyDrawer>;
			if (decoDrawers != null)
			{
				result.AddRange(decoDrawers);
			}

			return result;
		}

		Dictionary<string, bool> _cachedHeader = new Dictionary<string, bool>();

		public bool IsHeader(MaterialProperty matProp, Shader shader)
		{
			bool isHeader = false;
			if (!_cachedHeader.TryGetValue(matProp.name, out isHeader))
			{
				isHeader = GetDrawer(matProp, shader, "Header") != null;
				_cachedHeader[matProp.name] = isHeader;
			}
			return isHeader;
		}
	}

}