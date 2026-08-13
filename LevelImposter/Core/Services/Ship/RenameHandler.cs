using System.Collections.Generic;

namespace LevelImposter.Core.Services.Ship;

/// <summary>
///     Handles all modified strings
/// </summary>
public class RenameHandler
{
    private readonly Dictionary<StringNames, string> _stringRenames = new();
    private readonly Dictionary<SystemTypes, string> _systemRenames = new();
    private readonly Dictionary<TaskTypes, string> _taskRenames = new();

    /// <summary>
    ///     Renames a SystemType in the TranslationController
    /// </summary>
    /// <param name="system">System to rename</param>
    /// <param name="name">String to rename to</param>
    public void Add(SystemTypes system, string name)
    {
        _systemRenames[system] = name;

        // Also rename the string name
        var stringName = TranslationController.Instance.GetSystemName(system);
        _stringRenames[stringName] = name;
    }

    /// <summary>
    ///     Renames a TaskTypes in the TranslationController
    /// </summary>
    /// <param name="task">Task to rename</param>
    /// <param name="name">String to rename to</param>
    public void Add(TaskTypes task, string name)
    {
        _taskRenames[task] = name;

        // Also rename the string name
        var stringName = TranslationController.Instance.GetTaskName(task);
        _stringRenames[stringName] = name;
    }

    /// <summary>
    ///     Renames a StringNames in the TranslationController
    /// </summary>
    /// <param name="stringName">StringNames to rename</param>
    /// <param name="name">String to rename to</param>
    public void Add(StringNames stringName, string name)
    {
        _stringRenames[stringName] = name;
    }

    /// <summary>
    ///     Attempts to get a renamed string from the database.
    /// </summary>
    /// <param name="stringNames">Value to lookup</param>
    /// <param name="renamedString">The renamed string value</param>
    /// <returns>True if the value was found, false otherwise</returns>
    public bool TryGet(StringNames stringNames, out string? renamedString)
    {
        renamedString = null;
        return _stringRenames.TryGetValue(stringNames, out renamedString);
    }

    /// <summary>
    ///     Attempts to get a renamed string from the database.
    /// </summary>
    /// <param name="stringNames">Value to lookup</param>
    /// <param name="renamedString">The renamed string value</param>
    /// <returns>True if the value was found, false otherwise</returns>
    public bool TryGet(TaskTypes stringNames, out string? renamedString)
    {
        renamedString = null;
        return _taskRenames.TryGetValue(stringNames, out renamedString);
    }

    /// <summary>
    ///     Attempts to get a renamed string from the database.
    /// </summary>
    /// <param name="stringNames">Value to lookup</param>
    /// <param name="renamedString">The renamed string value</param>
    /// <returns>True if the value was found, false otherwise</returns>
    public bool TryGet(SystemTypes stringNames, out string? renamedString)
    {
        renamedString = null;
        return _systemRenames.TryGetValue(stringNames, out renamedString);
    }

    /// <summary>
    ///     Clears all renamed values
    /// </summary>
    public void Clear()
    {
        _systemRenames.Clear();
        _taskRenames.Clear();
        _stringRenames.Clear();
    }
}