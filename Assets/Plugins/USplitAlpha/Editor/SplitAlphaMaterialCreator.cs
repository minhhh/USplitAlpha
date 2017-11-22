using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace USplitAlpha
{
    public class SplitAlphaMaterialCreator
    {
        private static readonly string PNGSuffix = ".png";
        private static readonly string MatSuffix = ".mat";
        private static readonly string StarPng = "*.png";
        private static readonly string StarMat = "*.mat";

        [MenuItem ("Assets/USplitAlpha/Create Base Particle Materials", false, 100)]
        public static void CreateBaseParticleSplitAlphaMaterial ()
        {
            Dictionary <string, string> splitAlphaShaderNames = new Dictionary<string, string> ();
            splitAlphaShaderNames.Add ("_Ad", "SplitAlpha/Mobile/Particles/Additive");
            splitAlphaShaderNames.Add ("_Al", "SplitAlpha/Mobile/Particles/Alpha Blended");
            splitAlphaShaderNames.Add ("_Mu", "SplitAlpha/Mobile/Particles/Multiply");

            CreateSplitAlphaMaterial (splitAlphaShaderNames);
        }

        [MenuItem ("Assets/USplitAlpha/Create Base Particle Materials", true)]
        private static bool ValidateCreateBaseParticleSplitAlphaMaterial ()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem ("Assets/USplitAlpha/Create UIDefault Material", false, 101)]
        private static void CreateUIDefaultSplitAlphaMaterial ()
        {
            Dictionary <string, string> splitAlphaShaderNames = new Dictionary<string, string> ();
            splitAlphaShaderNames.Add ("_UIDefault", "SplitAlpha/UI/Default");

            CreateSplitAlphaMaterial (splitAlphaShaderNames);
        }

        [MenuItem ("Assets/USplitAlpha/Create UIDefault Material", true)]
        private static bool ValidateCreateUIDefaultSplitAlphaMaterial ()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem ("Assets/USplitAlpha/Create Sprite Default Material", false, 102)]
        private static void CreateSpriteDefaultSplitAlphaMaterial ()
        {
            Dictionary <string, string> splitAlphaShaderNames = new Dictionary<string, string> ();
            splitAlphaShaderNames.Add ("_SpriteDefault", "SplitAlpha/Sprites/Default");

            CreateSplitAlphaMaterial (splitAlphaShaderNames);
        }

        [MenuItem ("Assets/USplitAlpha/Create Sprite Default Material", true)]
        private static bool ValidateCreateSpriteDefaultSplitAlphaMaterial ()
        {
            return Selection.activeObject is Texture2D;
        }

        public static void CreateSplitAlphaMaterial (Dictionary <string, string> splitAlphaShaderNames)
        {
            HashSet<string> selectedObjectPaths = new HashSet<string> ();

            foreach (var assetGUID in Selection.assetGUIDs) {
                var assetPath = AssetDatabase.GUIDToAssetPath (assetGUID);
                if (AssetDatabase.IsValidFolder (assetPath)) {
                    selectedObjectPaths.UnionWith (Directory.GetFiles (assetPath, StarPng, SearchOption.AllDirectories));
                } else if (assetPath.EndsWith (PNGSuffix)) {
                    selectedObjectPaths.Add (assetPath);
                }
            }

            foreach (var path in selectedObjectPaths) {
                string materialPath = System.IO.Path.GetDirectoryName (path);
                string materialName = System.IO.Path.GetFileNameWithoutExtension (path);
                Texture2D mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (path);
                var alphaTexturePath = SplitAlphaTextureCreator.GetAlphaTextureAssetPath (path);
                Texture2D alphaTexture = AssetDatabase.LoadAssetAtPath <Texture2D> (alphaTexturePath);

                foreach (string shaderName in splitAlphaShaderNames.Keys) {
                    CreateSplitAlphaMaterial (splitAlphaShaderNames [shaderName], materialPath + "/" + materialName + shaderName + ".mat", mainTexture, alphaTexture);
                }
            }
        }

        private static void CreateSplitAlphaMaterial (string shaderName, string path, Texture2D mainTexture, Texture2D alphaTexture)
        {
            AssetDatabase.DeleteAsset (path);
            Material adMaterial = new Material (Shader.Find (shaderName));
            adMaterial.SetTexture ("_MainTex", mainTexture);
            adMaterial.SetTexture ("_AlphaTex", alphaTexture);
            AssetDatabase.CreateAsset (adMaterial, path);
        }

    }
}
