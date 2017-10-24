using UnityEngine;
using UnityEditor;
using System.IO;

namespace UBootstrap.Editor
{
    [CustomEditor (typeof(SpriteCollection))]
    public class SpriteCollectionEditor : UnityEditor.Editor
    {
        private SerializedProperty folderName, texture, sprites;

        protected virtual void OnEnable ()
        {
            folderName = serializedObject.FindProperty ("folderName");
            texture = serializedObject.FindProperty ("texture");
            sprites = serializedObject.FindProperty ("sprites");
        }

        override public void OnInspectorGUI ()
        {
            SpriteCollection component = (SpriteCollection)target;
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.PropertyField (folderName);

            float reloadWidth = GUI.skin.label.CalcSize (new GUIContent ("Reload")).x + 20;
            if (GUILayout.Button ("Reload", GUILayout.Width (reloadWidth))) {
//                Debug.Log (this.GetType ().Name + "::OnInspectorGUI Load pngs from " + component.folderName);
                string[] spritePaths = Directory.GetFiles (Application.dataPath + "/" + component.folderName, "*.png", SearchOption.TopDirectoryOnly);
                component.sprites.Clear ();

                string spritePath = string.Empty;
                for (int i = 0; i < spritePaths.Length; i++) {
                    var index = spritePaths [i].IndexOf ("/Assets") + 1;
                    spritePath = spritePaths [i].Substring (index);
                    var items = AssetDatabase.LoadAllAssetsAtPath (spritePath);
                    foreach (var item in items) {
                        if (item is Sprite) {
                            component.sprites.Add ((Sprite)item);
                        }
                    }

                }
            }

            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.PropertyField (texture);
            if (GUILayout.Button ("Reload", GUILayout.Width (reloadWidth))) {
                component.sprites.Clear ();
                string spritePath = AssetDatabase.GetAssetPath (component.texture);
                var index = spritePath.IndexOf ("/Assets") + 1;
                spritePath = spritePath.Substring (index);
                var items = AssetDatabase.LoadAllAssetsAtPath (spritePath);
                foreach (var item in items) {
                    if (item is Sprite) {
                        component.sprites.Add ((Sprite)item);
                    }
                }

            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.PropertyField (sprites, true);
            serializedObject.ApplyModifiedProperties ();
        }
    }
}