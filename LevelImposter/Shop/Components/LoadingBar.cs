using System;
using UnityEngine;

namespace LevelImposter.Shop
{
    public class LoadingBar : MonoBehaviour
    {
        public LoadingBar(IntPtr intPtr) : base(intPtr)
        {
        }

        public static LoadingBar? Instance { get; private set; }

        private GameObject? _loadingBar = null;
        private TMPro.TMP_Text? _mapText = null;
        private TMPro.TMP_Text? _statusText = null;

        /// <summary>
        /// Sets the name of the map being loaded
        /// </summary>
        /// <param name="mapName">Name of the map</param>
        public void SetMapName(string mapName)
        {
            _mapText?.SetText(mapName);
        }

        /// <summary>
        /// Sets the status text of the loading bar
        /// </summary>
        /// <param name="status">Text to display</param>
        public void SetStatus(string status)
        {
            _statusText?.SetText(status);
        }

        /// <summary>
        /// Sets the progress of the loading bar
        /// </summary>
        /// <param name="percent">Percentage of completion, from 0 to 1</param>
        public void SetProgress(float percent)
        {
            if (_loadingBar == null)
                return;

            _loadingBar.transform.localPosition = new Vector3(percent - 1, 0, 0);
        }

        /// <summary>
        /// Sets the visibility of the loading bar
        /// </summary>
        /// <param name="visible">True iff the loading bar should be visible</param>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void Awake()
        {
            // Singleton
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _loadingBar = transform.Find("BarMask").Find("Bar").gameObject;
            _mapText = transform.Find("MapText").GetComponent<TMPro.TMP_Text>();
            _statusText = transform.Find("StatusText").GetComponent<TMPro.TMP_Text>();

            //DontDestroyOnLoad(gameObject);
        }
        public void OnDestroy()
        {
            Instance = null;

            _loadingBar = null;
            _mapText = null;
            _statusText = null;
        }
    }
}