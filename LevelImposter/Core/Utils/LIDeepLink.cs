using ImaginationOverflow.UniversalDeepLinking;


namespace LevelImposter.Core
{
    /// <summary>
    /// Handles a Deep Link between LevelImposter.net and Among Us.
    /// </summary>
    public class LIDeepLink
    {
        /// <summary>
        /// Initializes on application start
        /// </summary>
        public static void Init()
        {
            DeepLinkManager.Instance.add_LinkActivated((LinkActivationHandler)onLinkActivation);
            DeepLinkManager.Instance.RegisterIfNecessary();
        }

        private static void onLinkActivation(LinkActivation link)
        {
            // TODO Deep Linking w/ LevelImposter.net
        }
    }
}
