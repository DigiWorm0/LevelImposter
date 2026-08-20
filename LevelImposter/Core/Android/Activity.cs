using UnityEngine.Android;

namespace LevelImposter.Core.Android;

public class Activity(JavaObject? baseObject) : JavaObject(baseObject)
{
    public static Activity GetCurrent()
    {
        var currentActivity = new JavaObject(AndroidApp.Activity);
        return new Activity(currentActivity);
    }

    public void StartActivity(Intent intent)
    {
        Call("startActivity", intent);
    }
}