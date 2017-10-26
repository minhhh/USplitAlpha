using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace USplitAlpha
{
    public class SplitAlphaReplacer
    {
        public static void Apply (Material m)
        {
            var texture = m.GetTexture ("_MainTex");
            if (texture == null || !(texture is Texture2D)) {
                Debug.LogErrorFormat ("{0}::{1} Make sure material have main texture!", "SplitAlphaReplacer", "Apply");
                return;
            }

            var alphaTexturePath = SplitAlphaTextureCreator.GetAlphaTextureAssetPath (AssetDatabase.GetAssetPath (texture));
            Texture2D alphaTexture = AssetDatabase.LoadAssetAtPath <Texture2D> (alphaTexturePath);
            if (alphaTexture == null) {
                Debug.LogErrorFormat ("{0}::{1} Cannot Find Alpha texture at {2}!", "SplitAlphaReplacer", "Apply", alphaTexturePath);
                return;
            }
            m.SetTexture ("_AlphaTex", alphaTexture);
        }
    }
}
