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
		Dictionary<MaterialProperty, MaterialPropertyDrawer> _cachedDrawerDict = new Dictionary<MaterialProperty, MaterialPropertyDrawer>();
		Dictionary<MaterialProperty, List<MaterialPropertyDrawer>> _cachedDecoratorDict = new Dictionary<MaterialProperty, List<MaterialPropertyDrawer>>();
		Dictionary<MaterialProperty, bool> _cachedHeader = new Dictionary<MaterialProperty, bool>();
		Dictionary<MaterialProperty, Dictionary<System.Type, MaterialPropertyDrawer>> _cachedPropDrawerByTypes = new Dictionary<MaterialProperty, Dictionary<System.Type, MaterialPropertyDrawer>>();



		public ShaderReflectionHelper()
		{
			var materialPropHandlerType = System.Type.GetType("UnityEditor.MaterialPropertyHandler, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);
			getHandlerMethod = materialPropHandlerType.GetMethod("GetHandler", BindingFlags.NonPublic | BindingFlags.Static);
		}

		public void ApplyMaterialPropertyHandler(MaterialProperty matProp, Shader shader)
		{
			var drawer = GetDrawer(matProp, shader);
			if (drawer != null)
			{
				drawer.Apply(matProp);
			}

			var decorators = GetDecorators(matProp, shader);
			if (decorators != null)
			{
				foreach (var decorator in decorators)
				{
					decorator.Apply(matProp);
				}
			}
		}

		private object GetCachedHandler(MaterialProperty matProp, Shader shader)
		{
			return getHandlerMethod.Invoke(null, new object[] {shader, matProp.name});
		}

		private MaterialPropertyDrawer GetDrawer(MaterialProperty matProp, Shader shader)
		{
			MaterialPropertyDrawer drawer;
			if (!_cachedDrawerDict.TryGetValue(matProp, out drawer))
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
				drawer = propDrawerInfo.GetValue(handler) as MaterialPropertyDrawer;
				if (drawer == null)
				{
					_cachedDrawerDict[matProp] = null;
					return null;
				}

				Dictionary<System.Type, MaterialPropertyDrawer> cache;
				if (!_cachedPropDrawerByTypes.TryGetValue(matProp, out cache))
				{
					cache = new Dictionary<System.Type, MaterialPropertyDrawer>();
					_cachedPropDrawerByTypes[matProp] = cache;
				}
				cache[drawer.GetType()] = drawer;
			}
			return drawer;
		}

		private List<MaterialPropertyDrawer> GetDecorators(MaterialProperty matProp, Shader shader)
		{
			List<MaterialPropertyDrawer> decorators;
			if (!_cachedDecoratorDict.TryGetValue(matProp, out decorators))
			{
				var handler = GetCachedHandler(matProp, shader);
				if (handler == null)
				{
					return null;
				}
				if (decoratorDrawerInfo == null)
				{
					var handlerType = handler.GetType();
					decoratorDrawerInfo = handlerType.GetField("m_DecoratorDrawers", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				decorators = decoratorDrawerInfo.GetValue(handler) as List<MaterialPropertyDrawer>;
				if (decorators == null)
				{
					_cachedDecoratorDict[matProp] = null;
					return null;
				}
				_cachedDecoratorDict[matProp] = decorators;

				Dictionary<System.Type, MaterialPropertyDrawer> cache;
				if (!_cachedPropDrawerByTypes.TryGetValue(matProp, out cache))
				{
					cache = new Dictionary<System.Type, MaterialPropertyDrawer>();
					_cachedPropDrawerByTypes[matProp] = cache;
				}
				if (decorators != null)
				{
					foreach (var decorator in decorators)
					{
						cache[decorator.GetType()] = decorator;
					}
				}
			}
			return decorators;
		}

		public bool IsHeader(MaterialProperty matProp, Shader shader)
		{
			bool isHeader = false;
			if (!_cachedHeader.TryGetValue(matProp, out isHeader))
			{
				var decorators = GetDecorators(matProp, shader);
				isHeader = decorators != null && decorators.Any(d => d.GetType().Name.IndexOf("HeaderDecorator", System.StringComparison.Ordinal) >= 0);
				_cachedHeader[matProp] = isHeader;
			}
			return isHeader;
		}

		public T GetPropertyDrawer<T>(MaterialProperty matProp, Shader shader) where T : MaterialPropertyDrawer
		{
			MaterialPropertyDrawer result = null;
			Dictionary<System.Type, MaterialPropertyDrawer> cache;
			if (!_cachedPropDrawerByTypes.TryGetValue(matProp, out cache))
			{
				GetDecorators(matProp, shader);
				GetDrawer(matProp, shader);
				if (!_cachedPropDrawerByTypes.TryGetValue(matProp, out cache))
				{
					return result as T;
				}
			}
			cache.TryGetValue(typeof(T), out result);
			return result as T;
		}
	}

}