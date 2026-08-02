using System;

namespace LevelImposter.Core.Models;

[Serializable]
public class LITrigger
{
    public string id { get; set; }
    public Guid? elemID { get; set; }
    public string? triggerID { get; set; }
}