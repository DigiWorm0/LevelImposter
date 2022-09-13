using System;
using System.Collections.Generic;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking;


namespace LevelImposter.Core
{
    public class LIDeepLink
    {
        public static void Init()
        {
            DeepLinkManager.Instance.add_LinkActivated((LinkActivationHandler)OnLinkActivation);
            DeepLinkManager.Instance.RegisterIfNecessary();
        }

        public static void OnLinkActivation(LinkActivation link)
        {
            // TODO Deep Linking w/ LevelImposter.net
        }
    }
}
