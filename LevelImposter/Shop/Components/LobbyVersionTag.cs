using System;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class LobbyVersionTag(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private TMP_Text? _text;

    [HideFromIl2Cpp] public static LobbyVersionTag? Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        // Create Textbox
        _text = gameObject.AddComponent<TextMeshPro>();
        _text.fontSize = 1.4f;

        // Position Textbox
        var position = new Vector3
        {
            x = 4.9f,
            y = -5.1f,
            z = -30
        };
        transform.localPosition = position;

        // Update Text
        UpdateText();
    }

    public void Update()
    {
        if (!_text)
            return;

        // Enable/Disable Text
        _text.enabled = ShouldEnable();
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public static void UpdateText()
    {
        if (Instance == null || Instance._text == null)
            return;

        // Get the current map
        var currentMap = MapLoader.CurrentMap;
        var isFallback = GameState.IsFallbackMapLoaded;

        // Generate version tag
        StringBuilder versionTagBuilder = new();

        // Map Name
        if (isFallback || currentMap == null)
        {
            versionTagBuilder.Append("Random Custom Map\n");
        }
        else
        {
            versionTagBuilder.Append($"<color=#1a95d8>{currentMap.name}</color>");
            versionTagBuilder.Append($" by {currentMap.authorName}\n");
        }

        // Version
        versionTagBuilder.Append("<size=1.3>");
        versionTagBuilder.Append("<color=#1a95d888>Level</color>");
        versionTagBuilder.Append("<color=#cb282888>Imposter</color>");
        versionTagBuilder.Append("<color=#ffffff88> v");
        versionTagBuilder.Append(LevelImposter.DisplayVersion);
        versionTagBuilder.Append("</color>");
        versionTagBuilder.Append("</size>");

        // Set Text
        Instance._text.text = versionTagBuilder.ToString();
    }

    private bool ShouldEnable()
    {
        if (MapLoader.CurrentMap == null)
            return false;

        if (MapUtils.GetCurrentMapType() != MapType.LevelImposter)
            return false;

        if (!GameState.IsInLobby)
            return false;

        return true;
    }
}