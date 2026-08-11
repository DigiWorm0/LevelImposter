using System;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.Core.Translations;
using LevelImposter.Core.Utils;
using LevelImposter.Networking.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LevelImposter.Shop.Components;

public class ModUpdater(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public Il2CppReferenceField<PassiveButton> closePopupButton = null!;
    public Il2CppReferenceField<GenericPopup> popup = null!;
    public Il2CppReferenceField<PassiveButton> updateButton = null!;
    public Il2CppReferenceField<TMP_Text> updateButtonVersionText = null!;

    private void Awake()
    {
        // Assign Button Events
        updateButton.Value.OnClick = new Button.ButtonClickedEvent();
        updateButton.Value.OnClick.AddListener((Action)UpdateMod);
    }

    private void Start()
    {
        // Don't check for updates on dev builds
        if (LevelImposter.IsDevBuild)
            return;

        // Check for updates
        GitHubAPI.GetLatestRelease(OnRelease, OnReleaseError);
    }

    private void OnReleaseError(string error)
    {
        LILogger.Warn("Failed to check for updates: " + error);
    }

    [HideFromIl2Cpp]
    private void OnRelease(GitHubAPI.GitHubRelease release)
    {
        if (GitHubAPI.IsCurrent(release))
            return;

        updateButton.Value.gameObject.SetActive(true);
        updateButtonVersionText.Value.text = $"<font=\"VCR SDF\">LI {release.Name}";
    }

    private void UpdateMod()
    {
        if (popup.Value == null)
            throw new Exception("Popup component is not assigned.");

        closePopupButton.Value.SetButtonEnableState(false);
        popup.Value.Show(Translation.Get("main_menu.update.updating"));
        updateButton.Value.gameObject.SetActive(false);

        GitHubAPI.UpdateMod(() =>
        {
            closePopupButton.Value.SetButtonEnableState(true);
            popup.Value.Show($"<b><color=green>{Translation.Get("main_menu.update_success.title")}</color></b>" +
                             $"\n{Translation.Get("main_menu.update_success.description")}");
        }, error =>
        {
            closePopupButton.Value.SetButtonEnableState(true);
            popup.Value.Show($"<b><color=red>{Translation.Get("main_menu.update_failed.title")}</color></b>" +
                             $"\n<size=1.5>{error}" +
                             $"\n<i>{Translation.Get("main_menu.update_failed.description")}</i></size>");
            updateButton.Value.gameObject.SetActive(true);
        });
    }
}