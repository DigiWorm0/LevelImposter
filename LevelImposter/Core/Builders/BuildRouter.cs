using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class BuildRouter
    {
        private ElementBuilder elementBuilder = new ElementBuilder();
        private Builder[] builders = {
            new SpawnBuilder(),
            new RoomBuilder(),
            new DummyBuilder(),
            new CamBuilder(),
            new UtilBuilder(),
            new VentBuilder()
        };

        public GameObject Build(LIElement element)
        {
            GameObject gameObject = elementBuilder.Build(element);
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