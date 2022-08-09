using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class DefaultBuilder
    {
        public GameObject Build(LIElement elem)
        {
            GameObject gameObject = new GameObject(elem.name);
            gameObject.layer = (int)Layer.Ship;
            gameObject.transform.position = new Vector3(elem.x, elem.y, elem.z);
            gameObject.transform.rotation = Quaternion.Euler(0, 0, elem.rotation);
            gameObject.transform.localScale = new Vector3(elem.xScale, elem.yScale, 0);

            if (elem.properties.spriteData != null)
            {
                SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                if (elem.properties.spriteData.StartsWith("data:image/gif;base64,"))
                {
                    GIFAnimator animator = gameObject.AddComponent<GIFAnimator>();
                    animator.Animate(elem.properties.spriteData);
                }
                else
                {
                    spriteRenderer.sprite = generateSprite(elem.properties.spriteData);
                }
            }

            if (elem.properties.colliders != null)
            {
                GameObject shadowObj = null;
                foreach (LICollider colliderData in elem.properties.colliders)
                {
                    if (colliderData.isSolid)
                    {
                        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
                        collider.SetPath(0, colliderData.GetPoints());
                    }
                    else
                    {
                        EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
                        collider.SetPoints(colliderData.GetPoints());
                    }

                    if (colliderData.blocksLight)
                    {
                        if (shadowObj == null)
                        {
                            shadowObj = new GameObject("Shadows");
                            shadowObj.transform.SetParent(gameObject.transform);
                            shadowObj.transform.localPosition = new Vector3(0, 0, 0);
                            shadowObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            shadowObj.transform.localScale = Vector3.one;
                            shadowObj.layer = (int)Layer.Shadow;
                        }

                        EdgeCollider2D collider = shadowObj.AddComponent<EdgeCollider2D>();
                        collider.SetPoints(colliderData.GetPoints());
                    }
                }
            }

            /*
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(elem.xScale, elem.yScale);
            boxCollider.offset = new Vector2(elem.xScale / 2, elem.yScale / 2);
            boxCollider.isTrigger = true;
            */

            return gameObject;
        }

        private Sprite generateSprite(string base64)
        {
            string sub64 = base64.Substring(base64.IndexOf(",") + 1);
            byte[] data = Convert.FromBase64String(sub64);
            Texture2D texture = new Texture2D(1, 1);
            ImageConversion.LoadImage(texture, data);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }
}
