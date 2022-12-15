using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class BuildRouter
    {
        public List<IElemBuilder> _buildStack = null;

        /// <summary>
        /// Patchable method to append or remove builders from the build stack
        /// </summary>
        public void ResetStack()
        {
            _buildStack = new() {
                new DefaultBuilder(),

                new RoomBuilder(),
                new AdminMapBuilder(),
                new RoomNameBuilder(),

                new MinimapBuilder(),
                new DummyBuilder(),
                new UtilBuilder(),
                new SpawnBuilder(),
                new VentBuilder(),
                new CamBuilder(),
                new TaskBuilder(),
                new DecBuilder(),

                new SabBuilder(),
                new SabConsoleBuilder(),
                new SabMapBuilder(),

                new LadderBuilder(),
                new PlatformBuilder(),
                new StarfieldBuilder(),
                new FloatBuilder(),
                new AmbientSoundBuilder(),
                new StepSoundBuilder(),
                new TeleBuilder(),
                new TriggerAreaBuilder(),
                new TriggerConsoleBuilder(),

                new TriggerBuilder()
            };
        }

        /// <summary>
        /// Passes <c>LIElement</c> data through the build 
        /// stack to construct a GameObject.
        /// Should be run from <c>LIShipStatus.AddElement</c>.
        /// </summary>
        /// <param name="element">Element data to build</param>
        /// <returns></returns>
        public GameObject Build(LIElement element)
        {
            if (_buildStack == null)
                ResetStack();
            string objName = element.name.Replace("\\n", " ");
            GameObject gameObject = new GameObject(objName);
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
            if (_buildStack == null)
                ResetStack();
            foreach (IElemBuilder builder in _buildStack)
                builder.PostBuild();
        }
    }
}