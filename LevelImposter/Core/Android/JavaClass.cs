using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Il2CppObject = Il2CppSystem.Object;

namespace LevelImposter.Core.Android;

public class JavaClass(string className) : IDisposable
{
    public AndroidJavaClass BaseClass { get; } = new(className);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        BaseClass.Dispose();
    }

    /// <summary>
    ///     Calls a static method on this Java class and returns a new JavaObject wrapping the result.
    /// </summary>
    /// <param name="methodName">Name of the static method to call</param>
    /// <param name="args">Optional arguments to pass into the static method</param>
    /// <returns>
    ///     A new JavaObject wrapping the result of the static method call.
    ///     The JavaObject.BaseObject will be null if the result is void.
    /// </returns>
    public JavaObject CallStaticReturn(string methodName, params Il2CppObject[] args)
    {
        var argsArray = new Il2CppReferenceArray<Il2CppObject>(args);
        var outputObject = JavaCallMethods<AndroidJavaClass>.CallStaticReturn?.Invoke(
            BaseClass,
            [methodName, argsArray]
        ) as AndroidJavaObject;
        return new JavaObject(outputObject);
    }


    /// <summary>
    ///     Calls a static method on this Java class.
    /// </summary>
    /// <param name="methodName">Name of the static method to call</param>
    /// <param name="args">Optional arguments to pass into the static method</param>
    public void CallStatic(string methodName, params Il2CppObject[] args)
    {
        var argsArray = new Il2CppReferenceArray<Il2CppObject>(args);
        JavaCallMethods<AndroidJavaClass>.CallStatic?.Invoke(
            BaseClass,
            [methodName, argsArray]
        );
    }
}