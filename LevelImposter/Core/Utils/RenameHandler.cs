using System.Collections.Generic;

namespace LevelImposter.Core
{
    /// <summary>
    /// Handles all modified strings
    /// </summary>
    public class RenameHandler
    {
        private Dictionary<SystemTypes, string> _systemRenames = new();
        private Dictionary<TaskTypes, string> _taskRenames = new();
        private Dictionary<StringNames, string> _stringRenames = new();

        /// <summary>
        /// Renames a SystemType in the TranslationController
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
        /// Renames a TaskTypes in the TranslationController
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
        /// Renames a StringNames in the TranslationController
        /// </summary>
        /// <param name="task">String name to rename</param>
        /// <param name="name">String to rename to</param>
        public void Add(StringNames stringName, string name)
        {
            _stringRenames[stringName] = name;
        }

        /// <summary>
        /// Gets a SystemType to rename
        /// </summary>
        /// <param name="system">System to rename</param>
        /// <returns>String to replace SystemType with</returns>
        public string Get(SystemTypes system) => _systemRenames[system];

        /// <summary>
        /// Gets a TaskType to rename
        /// </summary>
        /// <param name="task">Task to rename</param>
        /// <returns>String to replace task with</returns>
        public string Get(TaskTypes task) => _taskRenames[task];

        /// <summary>
        /// Gets a StringName to rename
        /// </summary>
        /// <param name="task">StringName to rename</param>
        /// <returns>String to replace text with</returns>
        public string Get(StringNames stringNames) => _stringRenames[stringNames];


        /// <summary>
        /// Checks if the system should be renamed
        /// </summary>
        /// <param name="system">System to rename</param>
        /// <returns>True iff the system should be renamed</returns>
        public bool Contains(SystemTypes system) => _systemRenames.ContainsKey(system);

        /// <summary>
        /// Checks if the task should be renamed
        /// </summary>
        /// <param name="task">Task to rename</param>
        /// <returns>True iff the task should be renamed</returns>
        public bool Contains(TaskTypes task) => _taskRenames.ContainsKey(task);

        /// <summary>
        /// Checks if the task should be renamed
        /// </summary>
        /// <param name="stringName">StringNames to rename</param>
        /// <returns>True iff the task should be renamed</returns>
        public bool Contains(StringNames stringName) => _stringRenames.ContainsKey(stringName);

        /// <summary>
        /// Clears all renamed values
        /// </summary>
        public void Clear()
        {
            _systemRenames.Clear();
            _taskRenames.Clear();
            _stringRenames.Clear();
        }
    }
}
