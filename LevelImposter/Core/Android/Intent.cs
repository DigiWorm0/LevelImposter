namespace LevelImposter.Core.Android;

public class Intent(string action) : JavaObject("android.content.Intent", action)
{
    public void SetData(Uri uri)
    {
        using var _ = CallReturn("setData", uri.BaseObject);
    }

    public void AddFlags(int flags)
    {
        using var _ = CallReturn("addFlags", flags);
    }
}