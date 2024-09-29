using System;
using System.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class ValueBuilder : IElemBuilder
{
    public ValueBuilder()
    {
        AllBoolValues.Clear();
    }

    public static Dictionary<Guid, IBoolValue> AllBoolValues { get; } = new();

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("util-value"))
            return;

        switch (elem.type)
        {
            case "util-valuebool":
            {
                var defaultValue = elem.properties.defaultBoolValue ?? false;
                AllBoolValues.Add(elem.id, new BasicBoolValue(elem.id, defaultValue));
                break;
            }
            case "util-valuecomparator":
            {
                var operation = elem.properties.comparatorOperation switch
                {
                    "xor" => ComparatorValue.Operation.XOR,
                    "or" => ComparatorValue.Operation.OR,
                    "and" => ComparatorValue.Operation.AND,
                    "not" => ComparatorValue.Operation.NOT,
                    _ => ComparatorValue.Operation.AND
                };

                AllBoolValues.Add(
                    elem.id,
                    new ComparatorValue(
                        elem.properties.comparatorValueID1,
                        elem.properties.comparatorValueID2,
                        operation
                    )
                );
                break;
            }
            default:
                throw new Exception($"Invalid value type: {elem.type}");
        }
    }

    /// <summary>
    ///     Gets a bool value from the dictionary by ID
    /// </summary>
    /// <param name="id">GUID of the value's element</param>
    /// <returns>The cooresponding IBoolValue</returns>
    /// <exception cref="Exception">Thrown if the value can't be found</exception>
    public static IBoolValue GetBoolOfID(Guid? id)
    {
        // Check for null
        if (id == null)
            throw new Exception("Missing target value ID");

        // Get value from dictionary
        if (!AllBoolValues.TryGetValue(id.Value, out var value))
            throw new Exception("Bool value not found");

        // Return value
        return value;
    }
}