
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelImposter.Core;
using InnerNet;

namespace LevelImposter.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public MapFilter currentFilter = MapFilter.Downloaded;

        public static ShopManager Instance { get; private set; }

        private Scroller scroller;
        private Transform content;
        private Transform mainPanel;
        private Transform sidePanel;

        private GameObject mapBannerPrefab;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            mainPanel = transform.FindChild("MainPanel");
            sidePanel = transform.FindChild("SidePanel");
            content = mainPanel.FindChild("Content");
            mapBannerPrefab = content.FindChild("MapBannerPrefab").gameObject;

            GameObject downloadedBtn = sidePanel.FindChild("DownloadedBtn").gameObject;
            downloadedBtn.AddComponent<FilterButton>().Init(MapFilter.Downloaded);
            GameObject recentBtn = sidePanel.FindChild("TopBtn").gameObject;
            recentBtn.AddComponent<FilterButton>().Init(MapFilter.Top);
            GameObject verifiedBtn = sidePanel.FindChild("VerifiedBtn").gameObject;
            verifiedBtn.AddComponent<FilterButton>().Init(MapFilter.Verified);
            GameObject folderBtn = sidePanel.FindChild("FolderBtn").gameObject;
            folderBtn.AddComponent<FolderButton>().Init();

            scroller = mainPanel.gameObject.AddComponent<Scroller>();

            scroller.allowY = true;
            scroller.ContentYBounds = new FloatRange(-1.8f, -1.8f);
            scroller.ContentXBounds = new FloatRange(0, 0);
            scroller.ScrollbarXBounds = new FloatRange(0, 0);
            scroller.ScrollbarYBounds = new FloatRange(0, 0);
            scroller.Colliders = new UnhollowerBaseLib.Il2CppReferenceArray<Collider2D>(1);
            scroller.Colliders[0] = mainPanel.gameObject.GetComponent<BoxCollider2D>();
            scroller.Inner = content;

            ListDownloadedMaps();
        }

        public void ListDownloadedMaps()
        {
            LILogger.Info("Using downloaded maps...");
            currentFilter = MapFilter.Downloaded;
            RemoveChildren();
            string[] mapIDs = MapLoader.GetMapIDs();
            LIMetadata[] maps = new LIMetadata[mapIDs.Length];
            for (int i = 0; i < mapIDs.Length; i++)
            {
                maps[i] = MapLoader.GetMetadata(mapIDs[i]);
                maps[i].id = mapIDs[i];
            }
            ListMaps(maps);
        }

        public void ListRecentMaps()
        {
            LILogger.Info("Using recent maps...");
            currentFilter = MapFilter.Recent;
            RemoveChildren();
            MapAPI.GetRecentMaps(ListMaps);
        }

        public void ListVerifiedMaps()
        {
            LILogger.Info("Using featured maps...");
            currentFilter = MapFilter.Verified;
            RemoveChildren();
            MapAPI.GetVerifiedMaps(ListMaps);
        }

        public void ListTopMaps()
        {
            LILogger.Info("Using top maps...");
            currentFilter = MapFilter.Top;
            RemoveChildren();
            MapAPI.GetTopMaps(ListMaps);
        }

        private void RemoveChildren()
        {
            while (content.childCount > 1)
                DestroyImmediate(content.GetChild(1).gameObject);
        }

        public void ListMaps(LIMetadata[] maps)
        {
            LILogger.Info("Listed " + maps.Length + " maps");
            RemoveChildren();
            scroller.ContentYBounds = new FloatRange(-1.8f, (1.3f * maps.Length) - 1.8f);
            for (int i = 0; i < maps.Length; i++)
            {
                GameObject mapButton = Instantiate(mapBannerPrefab, content);
                mapButton.transform.localPosition = new Vector3(0, i * -1.3f + 1.8f, -1);
                mapButton.AddComponent<MapBanner>().Init(maps[i]);
            }
        }

        public void LaunchMap(string id)
        {
            LILogger.Info("Launching map in freeplay: " + id);
            MapLoader.LoadMap(id);

            AmongUsClient.Instance.TutorialMapId = 2;
            SaveManager.GameHostOptions.gameType = GameType.Normal;
            SoundManager.Instance.StopAllSound();
            AmongUsClient.Instance.GameMode = GameModes.FreePlay;
            DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
            AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, false);
            AmongUsClient.Instance.MainMenuScene = "MainMenu";
            AmongUsClient.Instance.OnlineScene = "Tutorial";
            AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);

            StartCoroutine(AmongUsClient.Instance.WaitForConnectionOrFail());
        }

        public void DownloadMap(Guid mapID, Action callbackFinish)
        {
            MapAPI.DownloadMap(mapID, (System.Action<string>)((string mapJson) =>
            {
                MapLoader.WriteMap(mapID.ToString(), mapJson);
                callbackFinish();
            }));
        }

        public void DeleteMap(string id)
        {
            MapLoader.DeleteMap(id.ToString());
        }
    }
}