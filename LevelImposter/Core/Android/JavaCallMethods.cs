using System;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace LevelImposter.Core.Android;

/// <summary>
///     Helper class that uses reflection to collect MethodInfos for AndroidJavaObject's Call and CallStatic methods.
///     This is necessary to prevent compile-time issues w/ Roslyn & Il2CppInterop.
/// </summary>
public static class JavaCallMethods<T>
{
    // ReSharper disable StaticMemberInGenericType
    private static readonly MethodInfo[] AllMethods = typeof(T)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance);


    private static readonly Type[] CallParameterTypes =
    [
        typeof(string),
        typeof(Il2CppReferenceArray<Object>)
    ];

    public static readonly MethodInfo? Call = FindVoid(AllMethods, "Call");
    public static readonly MethodInfo? CallStatic = FindVoid(AllMethods, "CallStatic");

    public static readonly MethodInfo? CallReturn = FindNonVoid<AndroidJavaObject>(AllMethods, "Call");
    public static readonly MethodInfo? CallReturnString = FindNonVoid<string>(AllMethods, "Call");
    public static readonly MethodInfo? CallStaticReturn = FindNonVoid<AndroidJavaObject>(AllMethods, "CallStatic");

    private static MethodInfo? FindVoid(MethodInfo[] methods, string name)
    {
        return methods.FirstOrDefault(m =>
            m.Name == name &&
            !m.IsGenericMethod &&
            m.ReturnType == typeof(void) &&
            m.GetParameters().Select(p => p.ParameterType).SequenceEqual(CallParameterTypes));
    }

    private static MethodInfo? FindNonVoid<TReturnType>(MethodInfo[] methods, string name)
    {
        return methods.FirstOrDefault(m =>
            m.Name == name &&
            m.IsGenericMethod &&
            m.ReturnType != typeof(void) &&
            m.GetParameters().Select(p => p.ParameterType).SequenceEqual(CallParameterTypes)
        )?.MakeGenericMethod(typeof(TReturnType));
    }
    // ReSharper restore StaticMemberInGenericType
}