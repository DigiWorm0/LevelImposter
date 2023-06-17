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

        /// <summary>
        /// Renames a SystemType in the TranslationController
        /// </summary>
        /// <param name="system">System to rename</param>
        /// <param name="name">String to rename to</param>
        public void Add(SystemTypes system, string name)
        {
            _systemRenames[system] = name;
        }

        /// <summary>
        /// Renames a TaskTypes in the TranslationController
        /// </summary>
        /// <param name="task">Task to rename</param>
        /// <param name="name">String to rename to</param>
        public void Add(TaskTypes task, string name)
        {
            _taskRenames[task] = name;
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
        /// <returns>String top replace task with</returns>
        public string Get(TaskTypes task) => _taskRenames[task];

        /// <summary>
        /// Checks if the system should be renamed
        /// </summary>
        /// <param name="system">System to rename</param>
        /// <returns>True iff the system should be renamed</returns>
        public bool Contains(SystemTypes system) => _systemRenames.ContainsKey(system);

        /// <summary>
        /// Checks if the task should be renamed
        /// </summary>
        /// <param name="system">Task to rename</param>
        /// <returns>True iff the task should be renamed</returns>
        public bool Contains(TaskTypes task) => _taskRenames.ContainsKey(task);

        /// <summary>
        /// Clears all renamed values
        /// </summary>
        public void Clear()
        {
            _systemRenames.Clear();
            _taskRenames.Clear();
        }
    }
}
