using System;

namespace LevelImposter.Core;

[Serializable]
public class MissingShipException() : Exception(EXCEPTION_STRING)
{
    public const string EXCEPTION_STRING = "LIShipStatus.GetInstanceOrNull() or LIShipStatus.ShipStatus is null";
}