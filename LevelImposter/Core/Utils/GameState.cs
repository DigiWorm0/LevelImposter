using LevelImposter.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace LevelImposter.Core
{
    public static class GameState
    {
        public static bool IsHost => AmongUsClient.Instance?.AmHost ?? false;

        public static bool IsInFreeplay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
        public static bool IsInLobby => LobbyBehaviour.Instance != null;
        public static bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";
        public static bool IsInShop => ShopManager.Instance != null;

        public static bool IsInCustomMap => LIShipStatus.Instance != null;
        public static bool IsCustomMapLoaded => MapLoader.CurrentMap != null && !MapLoader.IsFallback;
        public static bool IsFallbackMapLoaded => MapLoader.CurrentMap != null && MapLoader.IsFallback;
    }
}
