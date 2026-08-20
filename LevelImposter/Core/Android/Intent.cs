namespace LevelImposter.Core.Android;

public class Intent(string action) : JavaObject("android.content.Intent", action)
{
    public const string ACTION_OPEN_DOCUMENT = "android.intent.action.OPEN_DOCUMENT";
    public const string CATEGORY_OPENABLE = "android.intent.category.OPENABLE";

    public void AddFlags(int flags)
    {
        using var _ = CallReturn("addFlags", flags);
    }

    public void AddCategory(string category)
    {
        using var _ = CallReturn("addCategory", category);
    }

    public void SetType(string type)
    {
        using var _ = CallReturn("setType", type);
    }

    public void PutExtra(string key, JavaObject value)
    {
        using var _ = CallReturn("putExtra", key, value.BaseObject);
    }
}