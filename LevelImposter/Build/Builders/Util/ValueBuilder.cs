using System;
using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Trigger.Values;

namespace LevelImposter.Build.Builders.Util;

internal static class ValueBuilder
{
    private static Dictionary<Guid, IBoolValue> AllBoolValues { get; } = new();

    private static Dictionary<string, IBoolValue> PresetBoolValues { get; } = new()
    {
        { "isImposter", new DelegateBoolValue(() => GameState.IsLocalPlayerImpostor) },
        { "isInMeeting", new DelegateBoolValue(() => GameState.IsInMeeting) },
        { "isDead", new DelegateBoolValue(() => GameState.IsLocalPlayerDead) }
    };

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        AllBoolValues.Clear();
    }

    [ElementBuilder(ElementTypes =
    [
        "util-valuebool",
        "util-valueboolpreset",
        "util-valuecomparator"
    ])]
    public static void Build(LIElement element)
    {
        switch (element.type)
        {
            case "util-valuebool":
            {
                var defaultValue = element.properties.defaultBoolValue ?? false;
                AllBoolValues.Add(element.id, new BasicBoolValue(element.id, defaultValue));
                break;
            }
            case "util-valueboolpreset":
                var preset = element.properties.valuePresetType ?? "";
                if (!PresetBoolValues.TryGetValue(preset, out var value))
                    throw new Exception($"Invalid value preset: {preset}");

                AllBoolValues.Add(element.id, value);
                break;
            case "util-valuecomparator":
            {
                var operation = element.properties.comparatorOperation switch
                {
                    "xor" => ComparatorValue.Operation.XOR,
                    "or" => ComparatorValue.Operation.OR,
                    "and" => ComparatorValue.Operation.AND,
                    "not" => ComparatorValue.Operation.NOT,
                    _ => ComparatorValue.Operation.AND
                };

                AllBoolValues.Add(
                    element.id,
                    new ComparatorValue(
                        element.properties.comparatorValueID1,
                        element.properties.comparatorValueID2,
                        operation
                    )
                );
                break;
            }
            default:
                throw new Exception($"Invalid value type: {element.type}");
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