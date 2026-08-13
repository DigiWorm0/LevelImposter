using System;

namespace LevelImposter.Core.Models;

[Serializable]
public class LITrigger
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public Guid? elemID { get; set; }
    public string? triggerID { get; set; }
}