using LevelImposter.Core;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class BuildRouter
    {
        private List<IElemBuilder> _buildStack = new() {
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

            new TriggerBuilder(),
            new CustomTextBuilder(),
            new ColorBuilder()
        };

        /// <summary>
        /// Patch me to add your own custom builders.
        /// Builders should implement <c>IElemBuilder</c>.
        /// </summary>
        public BuildRouter() { }

        /// <summary>
        /// Passes <c>LIElement</c> data through the build 
        /// stack to construct a GameObject.
        /// Should be run from <c>LIShipStatus.AddElement</c>.
        /// </summary>
        /// <param name="element">Element data to build</param>
        /// <returns></returns>
        public GameObject Build(LIElement element)
        {
            string objName = element.name.Replace("\\n", " ");
            var gameObject = new GameObject(objName);
            foreach (IElemBuilder builder in _buildStack)
            {
                builder.Build(element, gameObject);
            }
            return gameObject;
        }

        /// <summary>
        /// Runs the post-build process on every <c>IElemBuilder</c>.
        /// </summary>
        public void PostBuild()
        {
            foreach (IElemBuilder builder in _buildStack)
                builder.PostBuild();
        }
    }
}