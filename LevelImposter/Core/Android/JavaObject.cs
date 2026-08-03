using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Il2CppObject = Il2CppSystem.Object;

namespace LevelImposter.Core.Android;

public class JavaObject : IDisposable
{
    /// <summary>
    ///     Creates a new instance of the target object by instantiating the specified Java class with the provided arguments.
    /// </summary>
    /// <param name="className">A fully-qualified Java class name</param>
    /// <param name="args">Args to be passed into the object's constructor</param>
    public JavaObject(
        string className,
        params Il2CppObject[] args
    )
    {
        BaseObject = new AndroidJavaObject(className, args);
    }

    /// <summary>
    ///     Creates a new instance of the target object by wrapping an existing AndroidJavaObject.
    /// </summary>
    /// <param name="baseObject">An existing AndroidJavaObject to wrap</param>
    public JavaObject(AndroidJavaObject? baseObject)
    {
        BaseObject = baseObject;
    }

    /// <summary>
    ///     Creates a new instance of the target object by wrapping an existing JavaObject.
    /// </summary>
    /// <param name="baseObject">An existing JavaObject to wrap</param>
    public JavaObject(JavaObject? baseObject)
    {
        BaseObject = baseObject?.BaseObject;
    }

    public AndroidJavaObject? BaseObject { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        BaseObject?.Dispose();
    }

    /// <summary>
    ///     Calls a method on this instance of the java object.
    /// </summary>
    /// <param name="method">Name of the method</param>
    /// <param name="args">Optional arguments to pass into the method</param>
    /// <returns>
    ///     A new JavaObject wrapping the result of the method call.
    ///     The JavaObject.BaseObject will be null if the result is void.
    /// </returns>
    public JavaObject CallReturn(string method, params Il2CppObject[] args)
    {
        var argsArray = new Il2CppReferenceArray<Il2CppObject>(args);
        var outputObject = JavaCallMethods<AndroidJavaObject>.CallReturn?.Invoke(
            BaseObject,
            [method, argsArray]
        ) as AndroidJavaObject;
        return new JavaObject(outputObject);
    }

    /// <summary>
    ///     Calls a method on this instance of the java object.
    /// </summary>
    /// <param name="method">Name of the method</param>
    /// <param name="args">Optional arguments to pass into the method</param>
    public void Call(string method, params Il2CppObject[] args)
    {
        var argsArray = new Il2CppReferenceArray<Il2CppObject>(args);
        JavaCallMethods<AndroidJavaObject>.Call?.Invoke(
            BaseObject,
            [method, argsArray]
        );
    }

    public static implicit operator AndroidJavaObject?(JavaObject javaObject)
    {
        return javaObject.BaseObject;
    }

    public override string? ToString()
    {
        return JavaCallMethods<AndroidJavaObject>.CallReturnString?.Invoke(
            BaseObject,
            ["toString", new Il2CppReferenceArray<Il2CppObject>(0L)]
        ) as string;
    }
}