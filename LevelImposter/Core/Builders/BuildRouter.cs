using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class BuildRouter
    {
        public List<IElemBuilder> _buildStack;

        public BuildRouter()
        {
            InitStack();
        }

        /*
         *      Patch this Method to add/remove builders
         */
        public void InitStack()
        {
            _buildStack = new List<IElemBuilder> {
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
                new SabMapBuilder(),
                new LadderBuilder(),
                new PlatformBuilder(),
                new StarfieldBuilder(),
                new FloatBuilder(),
                new AmbientSoundBuilder(),
                new StepSoundBuilder(),
                new TeleBuilder(),
                new TriggerAreaBuilder(),

                new NoShadowBuilder(),
                new TriggerBuilder()
            };
        }

        public GameObject Build(LIElement element)
        {
            string objName = element.name.Replace("\\n", " ");
            GameObject gameObject = new GameObject(objName);
            foreach (IElemBuilder builder in _buildStack)
            {
                builder.Build(element, gameObject);
            }
            return gameObject;
        }

        public void PostBuild()
        {
            foreach (IElemBuilder builder in _buildStack)
                builder.PostBuild();
        }
    }
}