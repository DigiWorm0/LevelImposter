using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class DefaultBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            obj.layer = (int)Layer.Ship;
            obj.transform.position = new Vector3(elem.x, elem.y, elem.z);
            obj.transform.rotation = Quaternion.Euler(0, 0, elem.rotation);
            obj.transform.localScale = new Vector3(elem.xScale, elem.yScale, 0);

            if (elem.properties.spriteData != null)
            {
                SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
                if (elem.properties.spriteData.StartsWith("data:image/gif;base64,"))
                {
                    GIFAnimator animator = obj.AddComponent<GIFAnimator>();
                    animator.Init(elem.properties.spriteData);
                    animator.Play(true);
                }
                else
                {
                    SpriteLoader.Instance.LoadSprite(elem, obj);
                }
            }

            if (elem.properties.colliders != null)
            {
                foreach (LICollider colliderData in elem.properties.colliders)
                {
                    if (colliderData.isSolid)
                    {
                        PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();
                        collider.pathCount = 1;
                        collider.SetPath(0, colliderData.GetPoints());
                    }
                    else
                    {
                        EdgeCollider2D collider = obj.AddComponent<EdgeCollider2D>();
                        collider.SetPoints(colliderData.GetPoints());
                    }

                    if (colliderData.blocksLight)
                    {
                        GameObject shadowObj = new("Shadow " + colliderData.id);
                        shadowObj.transform.SetParent(obj.transform);
                        shadowObj.transform.localPosition = new Vector3(0, 0, 0);
                        shadowObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        shadowObj.transform.localScale = Vector3.one;
                        shadowObj.layer = (int)Layer.Shadow;

                        EdgeCollider2D shadowCollider = shadowObj.AddComponent<EdgeCollider2D>();
                        shadowCollider.SetPoints(colliderData.GetPoints(colliderData.isSolid));
                    }
                }
            }
        }

        public void PostBuild() { }
    }
}
