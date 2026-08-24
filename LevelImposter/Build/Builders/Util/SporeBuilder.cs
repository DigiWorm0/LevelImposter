using Il2CppSystem.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using PowerTools;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class SporeBuilder
{
    public static List<Mushroom> Mushrooms { get; } = new();

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        Mushrooms.Clear();
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes = ["util-spore"]
    )]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var prefabSpore = prefab.GetComponent<Mushroom>();

        // Sprite
        var spriteRenderer = gameObject.CloneSprite(prefab, true);
        gameObject.layer = (int)Layer.Ship;

        // Screen Mask
        var sporeRange = (element.properties.sporeRange ?? 3.7f) * 0.65f;
        var screenMaskPrefab = prefab.transform.FindChild("SporeScreenMask").gameObject;
        var screenMaskObj = new GameObject("ScreenMask");
        screenMaskObj.transform.parent = gameObject.transform;
        screenMaskObj.transform.localPosition = Vector3.zero;
        screenMaskObj.transform.position = new Vector3(
            screenMaskObj.transform.position.x,
            screenMaskObj.transform.position.y,
            10.0f
        );
        screenMaskObj.transform.localScale = new Vector3(sporeRange, sporeRange, 1.2f);
        var screenMaskRenderer = screenMaskObj.CloneSprite(screenMaskPrefab, true);

        // Screen Graphic
        var screenGraphicPrefab = prefab.transform.FindChild("SporeScreenGraphic").gameObject;
        var screenGraphicObj = new GameObject("ScreenGraphic");
        screenGraphicObj.transform.parent = gameObject.transform;
        screenGraphicObj.transform.localPosition = Vector3.zero;
        screenGraphicObj.transform.position = new Vector3(
            screenGraphicObj.transform.position.x,
            screenGraphicObj.transform.position.y,
            -10.0f
        );
        screenGraphicObj.transform.localScale = new Vector3(sporeRange, sporeRange, 1.2f);
        var screenGraphicRenderer = screenGraphicObj.CloneSprite(screenGraphicPrefab, true);

        // Sprite Anim
        var spriteAnim = gameObject.GetComponent<SpriteAnim>();
        if (spriteAnim == null)
        {
            // Using a custom sprite
            // Create a dummy animator to prevent null reference exceptions
            var dummyAnimObj = new GameObject("DummyAnim");
            dummyAnimObj.transform.parent = gameObject.transform;
            dummyAnimObj.transform.localPosition = Vector3.zero;
            dummyAnimObj.transform.localScale = Vector3.one;
            dummyAnimObj.transform.rotation = Quaternion.identity;

            // Clone animation
            var dummySpriteRenderer = dummyAnimObj.CloneSprite(prefab, true);
            spriteAnim = dummyAnimObj.GetComponent<SpriteAnim>();

            // Hide renderer
            dummySpriteRenderer.enabled = false;

            // Move mask/graphics to dummy
            screenMaskRenderer.transform.parent = dummyAnimObj.transform;
            screenGraphicRenderer.transform.parent = dummyAnimObj.transform;
        }

        // Set Color
        screenGraphicRenderer.color = element.properties.gasColor?.ToUnity() ??
                                      screenGraphicPrefab.GetComponent<SpriteRenderer>().color;

        // Collider
        var collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = element.properties.range ?? 0.25f;
        collider.isTrigger = true;

        // Mushroom
        var mushroom = gameObject.AddComponent<Mushroom>();
        mushroom.id = Mushrooms.Count;
        mushroom.mushroomCollider = collider;
        mushroom.mushroom = spriteRenderer;
        mushroom.mushroomAnimator = spriteAnim;
        mushroom.sporeMask = screenMaskObj;
        mushroom.sporeCloudMaskAnimator = screenMaskRenderer.GetComponent<SpriteAnim>();
        mushroom.spores = screenGraphicRenderer;
        mushroom.sporeCloudAnimator = screenGraphicObj.GetComponent<SpriteAnim>();
        mushroom.mushroomIdle = prefabSpore.mushroomIdle;
        mushroom.mushroomAppear = prefabSpore.mushroomAppear;
        mushroom.mushroomSteppedOn = prefabSpore.mushroomSteppedOn;
        mushroom.sporeCloudIdle = prefabSpore.sporeCloudIdle;
        mushroom.sporeCloudAppear = prefabSpore.sporeCloudAppear;
        mushroom.sporeCloudDisappear = prefabSpore.sporeCloudDisappear;
        mushroom.spawnSound = prefabSpore.spawnSound;
        mushroom.activateSporeSound = prefabSpore.activateSporeSound;
        mushroom.secondsBetweenSporeReleases = element.properties.sporeCooldown ?? 17.0f;
        mushroom.secondsSporeIsActive = element.properties.sporeDuration ?? 5.0f;


        mushroom.Awake(); // Fire again to fix animation states
        mushroom.enabled = true;
        Mushrooms.Add(mushroom);
    }
}