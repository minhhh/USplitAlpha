using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UBootstrap
{
    public class SpriteCollection : MonoBehaviour
    {
        public List<Sprite> sprites = new List<Sprite> ();
        public string collectionName;
        public string folderName;
        public Texture2D texture;

        void Awake ()
        {
        }

        void OnDestroy ()
        {
        }

        public Sprite GetSprite (int index)
        {
            if (index >= 0 && index < sprites.Count) {
                return sprites [index];
            }

            return null;
        }

        public Sprite GetSprite (string spriteName)
        {
            foreach (Sprite sprite in sprites) {
                if (sprite != null && sprite.name == spriteName) {
                    return sprite;
                }
            }
            return null;
        }
    }

}