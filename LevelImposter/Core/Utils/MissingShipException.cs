using System;

namespace LevelImposter.Core;

[Serializable]
public class MissingShipException() : Exception(EXCEPTION_STRING)
{
    public const string EXCEPTION_STRING = "Expected an instance of LIBaseShip, but none was found. " +
                                           "This usually indicates that LevelImposter is not properly initialized.";
}