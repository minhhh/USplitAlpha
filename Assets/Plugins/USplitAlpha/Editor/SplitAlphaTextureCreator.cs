using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace USplitAlpha
{
    public class SplitAlphaTextureCreator : AssetPostprocessor
    {
        public static readonly string AlphaFolderSuffix = "_alpha";
        public static readonly TextureCompressionQuality CompressionQuality = TextureCompressionQuality.Normal;


        private static readonly string PNGSuffix = ".png";
        private static readonly string MatSuffix = ".mat";
        private static readonly string StarPng = "*.png";
        private static readonly string StarMat = "*.mat";
        private static readonly string AssetsDir = "Assets";
        private static readonly string Android = "Android";
        private static readonly string iPhone = "iPhone";

        private static bool _isRevert = false;
        private static string _currentTexturePath;

        [MenuItem ("Assets/USplitAlpha/Apply", false, 1)]
        private static void MenuItem_ApplySplitAlphaToSelectedTexture ()
        {
            _ApplySplitAlphaToSelectedTexture (false);
        }

        [MenuItem ("Assets/USplitAlpha/Revert", false, 2)]
        private static void MenuItem_RevertSplitAlphaToSelectedTexture ()
        {
            _ApplySplitAlphaToSelectedTexture (true);
        }

        private static void _ApplySplitAlphaToSelectedTexture (bool isRevert)
        {
        
            HashSet<string> selectedObjectPaths = new HashSet<string> ();

            foreach (var assetGUID in Selection.assetGUIDs) {
                var assetPath = AssetDatabase.GUIDToAssetPath (assetGUID);
                if (AssetDatabase.IsValidFolder (assetPath)) {
                    selectedObjectPaths.UnionWith (Directory.GetFiles (assetPath, StarPng, SearchOption.AllDirectories));
                    selectedObjectPaths.UnionWith (Directory.GetFiles (assetPath, StarMat, SearchOption.AllDirectories));
                } else if (assetPath.EndsWith (PNGSuffix) || assetPath.EndsWith (MatSuffix)) {
                    selectedObjectPaths.Add (assetPath);
                }
            }

            var filteredObjectPaths = selectedObjectPaths.Where (x => !_IsAlphaTexture (x)).ToList ();

            var total = selectedObjectPaths.Count;
            var index = 0;
            foreach (var path in selectedObjectPaths) {
                index++;
                var progressText = string.Format ("[{0}/{1}] : {2}", index, total, path);
                if (EditorUtility.DisplayCancelableProgressBar (
                        "Applying Alpha", progressText, (float)index / total)) {
                    break;
                }
                if (path.EndsWith (PNGSuffix)) {
                    ApplySplitAlphaToTexture (path, isRevert);
                } else {
                    ApplySplitAlphaToMaterial (path, isRevert);
                }

            }

            Resources.UnloadUnusedAssets ();
            AssetDatabase.Refresh ();
            UnityEditor.EditorUtility.ClearProgressBar ();
        }

        public static bool ApplySplitAlphaToMaterial (string path, bool isRevert = false)
        {
            if (string.IsNullOrEmpty (path)) {
                Debug.LogError (string.Format ("ApplySplitAlphaToMaterial: path is null or empty of [{0}]", path));
                return false;
            }

            Material m = AssetDatabase.LoadAssetAtPath <Material> (path);

            if (m == null) {
                Debug.LogError (string.Format ("ApplySplitAlphaToMaterial: Material is null or empty of [{0}]", path));
                return false;
            }

            SplitAlphaReplacer.Apply (m);

            return true;
        }

        public static bool ApplySplitAlphaToTexture (string path, bool isRevert = false)
        {
            if (string.IsNullOrEmpty (path)) {
                Debug.LogError (string.Format ("ApplySplitAlphaToTexture: path is null or empty of [{0}]", path));
                return false;
            }

            if (_IsAlphaTexture (path)) {
                return false;
            }

            _isRevert = isRevert;
            _currentTexturePath = path;

            AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
            AssetDatabase.SaveAssets ();
            _currentTexturePath = null;
            AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);

            return true;
        }

        private static bool _IsAlphaTexture (string path)
        {
            string directoryName = Path.GetDirectoryName (path);
            if (!directoryName.EndsWith ("/" + AlphaFolderSuffix)) {
                return false;
            }
            return true;
        }

        public override int GetPostprocessOrder ()
        {
            return -9999;
        }

        void OnPreprocessTexture ()
        {
            if (string.IsNullOrEmpty (_currentTexturePath)) {
                return;
            }

            var importer = (assetImporter as TextureImporter);

            if (_currentTexturePath == importer.assetPath) {
                _SetTrueColorFormat (importer);
                importer.isReadable |= !_isRevert;
            }
        }

        void OnPostprocessTexture (Texture2D texture)
        {
            var importer = (assetImporter as TextureImporter);

            if (_currentTexturePath != importer.assetPath && _IsAlphaTexture (importer.assetPath)) {
                if (!_isRevert) {
                    _SetIOSAndAndroidRGB4 (importer, CompressionQuality);
                }
            } else if (_currentTexturePath == importer.assetPath) {
                if (!_isRevert) {
                    Texture2D alphaTexture = _CreateAlphaTexture (texture, importer);

                    if (alphaTexture != null) {
                        AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (alphaTexture), ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
                        _SetIOSAndAndroidRGB4 (importer, CompressionQuality);
                    } else {
                        _SetTrueColorFormat (importer);
                    }
                } else {
                    _DeleteAlphaTexture (GetAlphaTextureFilePath (importer.assetPath));
                }
            }
        }

        private static Texture2D _CreateAlphaTexture (Texture2D texture, TextureImporter textureImporter)
        {
            if (texture == null) {
                Debug.LogError (string.Format ("{0}::{1} Texture is null!", "SplitAlphaTextureCreator", "CreateAlphaTexture"));
                return null;
            }

            if (!textureImporter.isReadable) {
                Debug.LogError (string.Format ("{0}::{1} Texture must be readwrite enabled: {2}", "SplitAlphaTextureCreator", "CreateAlphaTexture", textureImporter.assetPath));
                return null;
            }

            if (!_IsPOTAndMultipleOf4 (texture.width) || !_IsPOTAndMultipleOf4 (texture.height)) {
                Debug.LogError (string.Format ("{0}::{1} Only textures with width/height being POT and multiple of 4 can be used: {2}", "SplitAlphaTextureCreator", "CreateAlphaTexture", textureImporter.assetPath));
                return null;
            }

            string savedPath = GetAlphaTextureFilePath (textureImporter.assetPath);
            Texture2D alphaTexture = _CreateRawAlphaTexture (texture);
            _SaveAlphaTexture (alphaTexture, savedPath);

            return alphaTexture;
        }

        public static string GetAlphaTextureFilePath (string assetPath)
        {
            string sourcePath = assetPath.Replace (AssetsDir, Application.dataPath);
            string sourceDirPath = Path.GetDirectoryName (sourcePath);
            string sourceFileName = Path.GetFileNameWithoutExtension (sourcePath);
            string alphaFileName = sourceFileName + PNGSuffix;
            return Path.Combine (Path.Combine (sourceDirPath, AlphaFolderSuffix), alphaFileName);
        }

        public static string GetAlphaTextureAssetPath (string assetPath)
        {
            string sourcePath = assetPath;
            string sourceDirPath = Path.GetDirectoryName (sourcePath);
            string sourceFileName = Path.GetFileNameWithoutExtension (sourcePath);
            string alphaFileName = sourceFileName + PNGSuffix;
            return Path.Combine (Path.Combine (sourceDirPath, AlphaFolderSuffix), alphaFileName);
        }

        private static Texture2D _CreateRawAlphaTexture (Texture2D texture)
        {
            var alphaTexture = new Texture2D (texture.width, texture.height, TextureFormat.RGB24, false);
            alphaTexture.wrapMode = TextureWrapMode.Clamp;

            var pixels = texture.GetPixels ();
            for (int i = 0; i < pixels.Length; i++) {
                var a = pixels [i].a;
                pixels [i] = new Color (a, a, a);
            }
            alphaTexture.SetPixels (pixels);

            return alphaTexture;
        }

        private static void _SaveAlphaTexture (Texture2D texture, string filePath)
        {
            FileInfo fileInfo = new FileInfo (filePath);

            if (!fileInfo.Directory.Exists) {
                fileInfo.Directory.Create ();
            }
            var bytes = texture.EncodeToPNG ();
            File.WriteAllBytes (filePath, bytes);
        }

        private static void _DeleteAlphaTexture (string filePath)
        {
            File.Delete (filePath);
            string dirPath = Path.GetDirectoryName (filePath);

            string[] filePaths = Directory.GetFiles (dirPath, StarPng, SearchOption.AllDirectories);
            if (filePaths.Length == 0) {
                Directory.Delete (dirPath, true);
            }
        }

        private static bool _IsPOTAndMultipleOf4 (int size)
        {
            return size >= 4 && ((size & (size - 1)) == 0);
        }

        private static void _SetTrueColorFormat (TextureImporter textureImporter)
        {
            textureImporter.ClearPlatformTextureSettings (Android);
            textureImporter.ClearPlatformTextureSettings (iPhone);
            TextureImporterPlatformSettings platformTextureSettings;

            platformTextureSettings = textureImporter.GetDefaultPlatformTextureSettings ();
            platformTextureSettings.format = TextureImporterFormat.RGBA32;
            platformTextureSettings.overridden = true;

            textureImporter.SetPlatformTextureSettings (platformTextureSettings);

            textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            textureImporter.maxTextureSize = 2048;
            textureImporter.generateCubemap = TextureImporterGenerateCubemap.None;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.mipmapEnabled = false;
        }

        public static void _SetIOSAndAndroidRGB4 (TextureImporter textureImporter, TextureCompressionQuality compressionQuality)
        {
            int cq = (int)compressionQuality;

            TextureImporterPlatformSettings platformTextureSettings;
            platformTextureSettings = textureImporter.GetPlatformTextureSettings (Android);
            platformTextureSettings.format = TextureImporterFormat.ETC_RGB4;
            platformTextureSettings.compressionQuality = cq;
            platformTextureSettings.overridden = true;
            textureImporter.SetPlatformTextureSettings (platformTextureSettings);

            platformTextureSettings = textureImporter.GetPlatformTextureSettings (iPhone);
            platformTextureSettings.format = TextureImporterFormat.PVRTC_RGB4;
            platformTextureSettings.compressionQuality = cq;
            platformTextureSettings.overridden = true;
            textureImporter.SetPlatformTextureSettings (platformTextureSettings);

            platformTextureSettings = textureImporter.GetDefaultPlatformTextureSettings ();
            platformTextureSettings.format = TextureImporterFormat.RGB24;
            platformTextureSettings.overridden = true;
            textureImporter.SetPlatformTextureSettings (platformTextureSettings);

            textureImporter.alphaSource = TextureImporterAlphaSource.None;
            textureImporter.mipmapEnabled = false;
        }

    }
}