using System;

namespace LevelImposter.Builders;

public class MapBuildException(string text) : Exception(text)
{
}