using InnerNet;
using LevelImposter.Core.Utils;
using LevelImposter.MapFormat.Models;
using UnityEngine;

namespace LevelImposter.RuntimeTests;

/// <summary>
///     Starts vanilla Skeld Freeplay and confirms a local player enters the
///     vanilla ship before any LevelImposter map test runs.
/// </summary>
public sealed class VanillaFreeplayLaunchTest : IRuntimeTest
{
    private const float LoginTimeoutSeconds = 180f;
    private const float TimeoutSeconds = 90f;
    private float _deadline;
    private bool _started;
    private bool _connected;

    public string Name => "Launch vanilla Skeld Freeplay";

    public void Start()
    {
        _deadline = Time.realtimeSinceStartup + LoginTimeoutSeconds;
        var client = AmongUsClient.Instance;
        if (client == null)
            throw new InvalidOperationException("AmongUsClient is not initialized.");

        _started = true;
    }

    public RuntimeTestResult? Tick()
    {
        if (!_started)
            return null;

        if (!_connected)
        {
            if (!IsLoginFlowComplete())
            {
                if (Time.realtimeSinceStartup >= _deadline)
                    return new RuntimeTestResult(false, "Timed out waiting for the Among Us login flow to finish.");

                return null;
            }

            StartVanillaFreeplay();
            return null;
        }

        var client = AmongUsClient.Instance;
        if (client != null &&
            client.NetworkMode == NetworkModes.FreePlay &&
            client.TutorialMapId == (int)MapType.Skeld &&
            ShipStatus.Instance != null &&
            PlayerControl.LocalPlayer != null)
        {
            return new RuntimeTestResult(true, "Vanilla Skeld Freeplay and local player are active.");
        }

        if (Time.realtimeSinceStartup >= _deadline)
            return new RuntimeTestResult(false, DescribeCurrentState(client));

        return null;
    }

    private static bool IsLoginFlowComplete()
    {
        if (!DestroyableSingleton<EOSManager>.InstanceExists ||
            !DestroyableSingleton<AccountManager>.InstanceExists)
            return false;

        return DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow() &&
               !DestroyableSingleton<AccountManager>.Instance.signInScreen.IsOpen();
    }

    private void StartVanillaFreeplay()
    {
        var client = AmongUsClient.Instance;
        if (client == null)
            throw new InvalidOperationException("AmongUsClient disappeared after login completed.");

        // Remove any map left in LevelImposter state and explicitly select vanilla Skeld.
        GameConfiguration.SetMap(null);
        GameConfiguration.SetLobbyMap(null);

        client.MainMenuScene = "MainMenu";
        client.OnlineScene = "Tutorial";
        client.NetworkMode = NetworkModes.FreePlay;
        client.TutorialMapId = (int)MapType.Skeld;

        DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
        client.SetEndpoint("127.0.0.1", 22023, false);

        _deadline = Time.realtimeSinceStartup + TimeoutSeconds;
        _connected = true;
        client.Connect(MatchMakerModes.HostAndClient, null);
    }

    private static string DescribeCurrentState(AmongUsClient? client)
    {
        return $"Timed out waiting for vanilla Freeplay. " +
               $"NetworkMode={client?.NetworkMode.ToString() ?? "unavailable"}, " +
               $"TutorialMapId={client?.TutorialMapId.ToString() ?? "unavailable"}, " +
               $"ShipStatus={ShipStatus.Instance != null}, " +
               $"LocalPlayer={PlayerControl.LocalPlayer != null}.";
    }
}
