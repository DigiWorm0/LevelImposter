using System;
using System.Collections.Generic;
using System.Diagnostics;
using LevelImposter.Core;

namespace LevelImposter.Builders;

public class BuildRouter
{
    public enum BuildStep
    {
        PreBuild,
        Build,
        PostBuild
    }

    private readonly List<IElemBuilder> _buildStack = new()
    {
        new MapPropertiesBuilder(),

        new TransformBuilder(),
        new SpriteBuilder(),
        new ColliderBuilder(),
        new MinigameSpriteBuilder(),
        new LayerBuilder(),

        new RoomBuilder(),
        new AdminMapBuilder(),
        new RoomNameBuilder(),

        new MinimapBuilder(),
        new DummyBuilder(),
        new UtilBuilder(),
        new SpawnBuilder(),
        new VentBuilder(),
        new CamBuilder(),
        new DisplayBuilder(),
        new TaskBuilder(),
        new DecBuilder(),
        new PhysicsObjectBuilder(),
        new MeetingOptionsBuilder(),
        new SabotageOptionsBuilder(),
        new OneWayColliderBuilder(),
        new DecontaminationBuilder(),
        new SporeBuilder(),
        new BinocularsBuilder(),
        new FilterBuilder(),
        new EjectBuilder(),
        new EjectDummyBuilder(),
        new EjectHandBuilder(),
        new ValueBuilder(),
        new PlayerMoverBuilder(),

        new SabBuilder(),
        new SabMixupBuilder(),
        new SabConsoleBuilder(),
        new SabMapBuilder(),
        new SabDoorBuilder(),

        new MinimapSpriteBuilder(),
        new LadderBuilder(),
        new PlatformBuilder(),
        new StarfieldBuilder(),
        new FloatBuilder(),
        new ScrollBuilder(),
        new AmbientSoundBuilder(),
        new StepSoundBuilder(),
        new TeleBuilder(),
        new TriggerAreaBuilder(),
        new TriggerConsoleBuilder(),
        new TriggerStartBuilder(),
        new TriggerDeathBuilder(),
        new TriggerShakeBuilder(),
        new TriggerAnimBuilder(),

        new CustomTextBuilder(),
        new ColorBuilder()
    };

    private readonly Stopwatch _sw = new();

    public void RunBuildStep(BuildStep buildStep, LIElement elem)
    {
        try
        {
            // Start Build Timer
            _sw.Restart();

            // Get Element
            var obj = LIShipStatus.GetInstance().MapObjectDB.GetObject(elem.id);
            if (obj == null)
                throw new Exception($"Could not find {elem} in map object db");

            // Run Builders
            foreach (var builder in _buildStack) {
                
                // Execute Build Step
                switch (buildStep)
                {
                    case BuildStep.PreBuild:
                        builder.OnPreBuild(elem, obj);
                        break;
                    case BuildStep.Build:
                        builder.OnBuild(elem, obj);
                        break;
                    case BuildStep.PostBuild:
                        builder.OnPostBuild(elem, obj);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(buildStep), buildStep, null);
                }
            }

            // Stop Build Timer
            _sw.Stop();
            if (_sw.ElapsedMilliseconds > LIConstants.ELEM_WARN_TIME)
                LILogger.Warn($"{elem.name} took {_sw.ElapsedMilliseconds}ms to {buildStep}");
        }
        catch (Exception ex)
        {
            // Log Error
            LILogger.Error($"Error building {elem.name} during {buildStep}: {ex}");
        }
    }

    public void Cleanup()
    {
        foreach (var builder in _buildStack)
            builder.OnCleanup();
    }
}