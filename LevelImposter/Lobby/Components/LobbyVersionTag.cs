using System;
using System.Text;
using LevelImposter.Core;
using LevelImposter.Shop;
using TMPro;
using UnityEngine;

namespace LevelImposter.Lobby;

public class LobbyVersionTag(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private TMP_Text? _text;
    private static LobbyVersionTag? _instance;
    
    public static bool IsInitialized => _instance != null;

    public void Awake()
    {
        _instance = this;
    }

    public void Start()
    {
        // Create Textbox
        _text = gameObject.AddComponent<TextMeshPro>();
        _text.fontSize = 1.6f;

        // Create Aspect Position
        var aspect = gameObject.AddComponent<AspectPosition>();
        aspect.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspect.DistanceFromEdge = new Vector3(10.4f, -2.0f, -30.0f);
        aspect.AdjustPosition();
        
        // Scale on Mobile
        if (LIConstants.IsMobile)
        {
            transform.localScale = new Vector3(1.15f, 1.15f, 1.1f);
            aspect.DistanceFromEdge = new Vector3(12.0f, -2.0f, -30.0f);
            aspect.AdjustPosition();
        }

        // Update Text
        UpdateText();
    }

    public void Update()
    {
        _text?.enabled = ShouldEnable();
    }

    public void OnDestroy()
    {
        _instance = null;
    }

    public static void UpdateText()
    {
        if (_instance == null || _instance._text == null)
            return;

        // Get the current map
        var currentMap = GameConfiguration.CurrentMap;
        if (currentMap == null)
            return;

        // Generate version tag
        StringBuilder versionTagBuilder = new();

        // Map Name
        versionTagBuilder.Append("<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">");
        if (GameConfiguration.HideMapName)
        {
            versionTagBuilder.Append("Random Custom Map");
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
        _instance._text.text = versionTagBuilder.ToString();
    }

    private static bool ShouldEnable()
    {
        if (GameConfiguration.CurrentMap == null)
            return false;

        if (GameConfiguration.CurrentMapType != MapType.LevelImposter)
            return false;

        if (!GameState.IsInLobby)
            return false;

        return true;
    }
}