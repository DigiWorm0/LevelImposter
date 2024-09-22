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
        _text.fontSize = 1.6f;

        // Position Textbox
        var position = new Vector3
        {
            x = 4.9f,
            y = -5.05f,
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
        versionTagBuilder.Append("<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">");
        if (isFallback || currentMap == null)
        {
            versionTagBuilder.Append("Random Custom Map\n");
        }
        else
        {
            versionTagBuilder.Append($"<color=#1a95d8>{currentMap.name}</color>");
            versionTagBuilder.Append($" by {currentMap.authorName}");
        }

        versionTagBuilder.Append("</font>");

        // Version
        versionTagBuilder.Append('\n');
        versionTagBuilder.Append("<font=\"VCR SDF\">");
        versionTagBuilder.Append("<size=1.2>");
        versionTagBuilder.Append("<color=#1a95d888>L</color>");
        versionTagBuilder.Append("<color=#cb282888>I</color>");
        versionTagBuilder.Append("<color=#ffffff88> v");
        versionTagBuilder.Append(LevelImposter.DisplayVersion);
        versionTagBuilder.Append("</color>");
        versionTagBuilder.Append("</size>");
        versionTagBuilder.Append("</font>");

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