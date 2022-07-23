using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class BuildRouter
    {
        private DefaultBuilder defaultBuilder = new DefaultBuilder();
        private Builder[] builders = {
            new DummyBuilder(),
            new UtilBuilder(),
            new SpawnBuilder(),
            new VentBuilder(),
            new CamBuilder(),
            new TaskBuilder(),
            new RoomBuilder(),
            new DecBuilder(),
            new MinimapBuilder()
        };

        public GameObject Build(LIElement element)
        {
            GameObject gameObject = defaultBuilder.Build(element);
            foreach (Builder builder in builders)
            {
                builder.Build(element, gameObject);
            }
            return gameObject;
        }

        public void PostBuild()
        {
            foreach (Builder builder in builders)
                builder.PostBuild();
        }
    }
}