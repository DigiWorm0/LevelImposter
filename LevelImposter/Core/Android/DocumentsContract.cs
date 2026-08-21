namespace LevelImposter.Core.Android;

public static class DocumentsContract
{
    private static readonly JavaClass ClassRef = new("android.provider.DocumentsContract");

    public static Uri BuildRootUri(string authority, string rootID)
    {
        return new Uri(ClassRef.CallStaticReturn("buildRootUri", authority, rootID));
    }
}