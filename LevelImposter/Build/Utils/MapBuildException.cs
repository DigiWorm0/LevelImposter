using System;

namespace LevelImposter.Build.Utils;

public class MapBuildException(string text) : Exception(text)
{
}