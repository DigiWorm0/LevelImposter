using System;
using System.Collections.Generic;
using System.Reflection;
using LevelImposter.Build.Attributes;

namespace LevelImposter.Builders;

public class BuildMethod(MethodInfo method, MapBuilderAttribute attribute)
{
    public MethodInfo Method => method;
    public MapBuilderAttribute Attribute => attribute;

    /// <summary>
    ///     Invokes the method using a given list of parameters.
    ///     If the method includes a parameter matching the name, it will be passed into the method.
    /// </summary>
    /// <param name="parameters">List of parameters to pass into the method</param>
    public void Invoke(Dictionary<string, object> parameters)
    {
        var orderedParameters = new List<object>();
        var methodParameters = Method.GetParameters();
        foreach (var parameter in methodParameters)
            if (parameters.TryGetValue(parameter.Name ?? "", out var value))
                orderedParameters.Add(value);
            else
                throw new ArgumentException($"Missing parameter '{parameter.Name}' for method '{Method.Name}'");

        Method.Invoke(null, orderedParameters.ToArray());
    }
}