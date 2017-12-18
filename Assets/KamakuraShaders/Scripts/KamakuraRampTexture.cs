using System.Collections.Generic;
using UnityEngine;

namespace Kayac.VisualArts
{
	[CreateAssetMenu(fileName = "NewRamp", menuName = "Ramp Texture")]
	public class KamakuraRampTexture : ScriptableObject
	{
		public int width = 8;
		public Gradient[] gradients = new Gradient[]
		{
			new Gradient()
			{
				colorKeys = new GradientColorKey[]{new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1)},
				alphaKeys = new GradientAlphaKey[]{new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)}
			}
		};
	}
}