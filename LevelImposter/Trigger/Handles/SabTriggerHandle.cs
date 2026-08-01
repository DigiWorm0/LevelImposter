namespace LevelImposter.Trigger;

public class SabTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        // Only handle triggers on the client that triggered them
        var origin = signal.SourcePlayer;
        var isClient = origin == null || origin == PlayerControl.LocalPlayer;
        if (!isClient)
            return;

        // Handle sabotage triggers
        switch (signal.TriggerID)
        {
            case "startOxygen":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 128);
                break;
            case "startLights":
                byte switchBits = 4;
                for (var i = 0; i < 5; i++)
                    if (BoolRange.Next())
                        switchBits |= (byte)(1 << i);
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(switchBits | 128));
                break;
            case "startReactor":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 128);
                break;
            case "startComms":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 128);
                break;
            case "startMixup":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 1);
                break;
            case "endOxygen":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
                break;
            case "endLights":
                var lights = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical,
                    (byte)((lights.ExpectedSwitches ^ lights.ActualSwitches) | 128));
                break;
            case "endReactor":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16);
                break;
            case "endComms":
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 0);
                break;
            case "endMixup":
                if (!ShipStatus.Instance.Systems.ContainsKey(SystemTypes.MushroomMixupSabotage))
                    return;
                var mixup = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage]
                    .Cast<MushroomMixupSabotageSystem>();
                mixup.currentSecondsUntilHeal = 0.01f;
                mixup.IsDirty = true;
                // TODO: Transmit to other clients
                break;
        }
    }
}