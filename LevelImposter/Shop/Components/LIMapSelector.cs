using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using LevelImposter.Core;
using System.Collections;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.Shop
{
    public class LIMapSelector : OptionBehaviour
    {
        public List<LIMapOption> mapOptions = new List<LIMapOption>();

        private TextMeshPro titleText = null;
        private TextMeshPro valueText = null;
        private PassiveButton incrementBtn = null;
        private PassiveButton decrementBtn = null;
        private SpriteRenderer background = null;
        private int current = 0;

        private float colorDuration = 0.4f;
        private readonly Color[] rainbowColors = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.magenta
        };

        public LIMapSelector(IntPtr intPtr) : base(intPtr)
        {
        }

        public void Awake()
        {
            name = "LI-MapName";

            titleText = transform.FindChild("Title_TMP").GetComponent<TextMeshPro>();
            valueText = transform.FindChild("Value_TMP").GetComponent<TextMeshPro>();
            incrementBtn = transform.FindChild("Plus_TMP").GetComponent<PassiveButton>();
            decrementBtn = transform.FindChild("Minus_TMP").GetComponent<PassiveButton>();
            background = transform.FindChild("Background").GetComponent<SpriteRenderer>();

            titleText.text = "Map Name";
            valueText.text = "Loading...";
            incrementBtn.OnClick.AddListener((Action)Increment);
            decrementBtn.OnClick.AddListener((Action)Decrement);

            LoadOptions();
        }

        public void OnEnable()
        {
            GameOptionsData gameOptions = PlayerControl.GameOptions;
            if (MapLoader.currentMap == null)
                SetValue(gameOptions.MapId);
            else
                SetValue(GetOptionIndex(MapLoader.currentMap.id));
            StartCoroutine(CoAnimateRainbow().WrapToIl2Cpp());
        }

        public void Increment()
        {
            SetValue(Math.Min(current + 1, mapOptions.Count - 1));
        }
        public void Decrement()
        {
            SetValue(Math.Max(current - 1, 0));
        }

        private void LoadOptions()
        {
            mapOptions.Clear();
            for (byte i = 0; i < Constants.MapNames.Count; i++)
            {
                LIMapOption mapOption = new LIMapOption();
                mapOption.name = Constants.MapNames[i];
                mapOption.shipID = i;
                mapOption.isCustom = false;
                mapOptions.Add(mapOption);
            }

            string[] mapIDs = MapFileAPI.Instance.ListIDs();
            foreach (string mapID in mapIDs)
            {
                Guid tempID;
                Guid.TryParse(mapID, out tempID);
                if (tempID == Guid.Empty)
                    continue;
                LIMetadata mapMetadata = MapFileAPI.Instance.Get(mapID);
                if (string.IsNullOrEmpty(mapMetadata.authorID) || string.IsNullOrEmpty(mapMetadata.id))
                    continue;
                LIMapOption mapOption = new LIMapOption();
                mapOption.name = mapMetadata.name;
                mapOption.shipID = 2;
                mapOption.isCustom = true;
                mapOption.mapID = mapID;
                mapOptions.Add(mapOption);
            }
        }

        private IEnumerator CoAnimateRainbow()
        {
            int i = 0;
            while (true)
            {
                float t = 0;
                Color a = rainbowColors[i];
                Color b = rainbowColors[(i + 1) % rainbowColors.Length];

                while (t < colorDuration)
                {
                    t += Time.deltaTime;
                    if (current < Constants.MapNames.Count)
                        background.color = Color.white;
                    else
                        background.color = Color.Lerp(a, b, t / colorDuration);
                    yield return null;
                }

                i = (i + 1) % rainbowColors.Length;
            }
        }

        private void SetValue(int index)
        {
            // Show Option
            if (index < 0 || index >= mapOptions.Count)
            {
                if (mapOptions.Count > 0)
                    SetValue(0);
                return;
            }
            LIMapOption mapOption = mapOptions[index];
            current = index;
            valueText.text = mapOption.name;

            // Save Settings
            if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
                return;
            if (mapOption.isCustom)
                MapLoader.LoadMap((string)mapOption.mapID);
            else
                MapLoader.UnloadMap();
            PlayerControl.GameOptions.MapId = mapOption.shipID;
            if (PlayerControl.LocalPlayer)
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
        }

        private int GetOptionIndex(string mapID)
        {
            for (int i = 0; i < mapOptions.Count; i++)
            {
                LIMapOption mapOption = mapOptions[i];
                if (mapOption.mapID == mapID)
                    return i;
            }
            return -1;
        }
    }

    public class LIMapOption
    {
        public string name { get; set; }
        public bool isCustom { get; set; }
        public string? mapID { get; set; }
        public byte shipID { get; set; }
    }
}
