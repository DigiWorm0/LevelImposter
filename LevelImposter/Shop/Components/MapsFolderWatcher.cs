using System;
using System.Collections;
using System.IO;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.FileIO.API;
using Reactor.Utilities;
using UnityEngine;

namespace LevelImposter.Shop.Components;

public class MapsFolderWatcher(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static bool _ignoreChanges;
    private static IEnumerator? _ignoreChangesCoroutine;
    private readonly FileSystemWatcher _mapsWatcher = new(MapFileAPI.GetDirectory());

    private bool _refreshMapsFolder;

    public void Start()
    {
        _mapsWatcher.Filter = "*.*";
        _mapsWatcher.IncludeSubdirectories = false;
        _mapsWatcher.EnableRaisingEvents = true;
        _mapsWatcher.Created += OnMapsFolderChange;
        _mapsWatcher.Deleted += OnMapsFolderChange;
        _mapsWatcher.Renamed += OnMapsFolderChange;

        StartCoroutine(CoRefreshMaps().WrapToIl2Cpp());
    }

    public void OnDestroy()
    {
        _mapsWatcher.Dispose();
    }

    /// <summary>
    ///     Ignores filesystem changes from the previous 100ms or the next 500ms.
    /// </summary>
    public static void IgnoreChanges()
    {
        if (_ignoreChangesCoroutine != null)
            Coroutines.Stop(_ignoreChangesCoroutine);
        _ignoreChangesCoroutine = Coroutines.Start(CoIgnoreChanges(0.5f));
    }

    private static IEnumerator CoIgnoreChanges(float duration)
    {
        _ignoreChanges = true;
        yield return new WaitForSeconds(duration);
        _ignoreChanges = false;
    }

    [HideFromIl2Cpp]
    private void OnMapsFolderChange(object sender, FileSystemEventArgs e)
    {
        // Check if we're ignoring changes
        // (When this process is causing the change)
        if (_ignoreChanges)
            return;

        // Flag the maps folder to be refreshed.
        // This is to avoid issues w/ elements updating outside the main Unity thread.
        _refreshMapsFolder = true;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoRefreshMaps()
    {
        while (true)
        {
            // Wait for flag
            while (!_refreshMapsFolder)
                yield return null;

            // Wait atleast 100ms to allow files to settle before reading
            // Also gives time for _ignoreChanges to be applied
            while (_refreshMapsFolder)
            {
                _refreshMapsFolder = false;
                yield return new WaitForSeconds(0.1f);
            }

            // Check if we're ignoring changes
            if (_ignoreChanges)
                continue;

            // Refresh the tab in the main Unity thread
            // (Only if we're on a local maps page)
            if (ShopManager.Instance?.CurrentTab == ShopTab.DownloadedMaps ||
                ShopManager.Instance?.CurrentTab == ShopTab.DownloadedLobbyMaps)
                ShopManager.Instance.RefreshTab();
        }
    }
}