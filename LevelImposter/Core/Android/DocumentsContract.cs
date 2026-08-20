namespace LevelImposter.Core.Android;

public static class DocumentsContract
{
    private static readonly JavaClass ClassRef = new("android.provider.DocumentsContract");

    public static Uri BuildDocumentUri(string authority, string documentId)
    {
        return new Uri(ClassRef.CallStaticReturn("buildDocumentUri", authority, documentId));
    }
}