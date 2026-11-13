using System;
using System.IO;
using System.Linq;
using System.Text;

namespace LevelImposter.FileIO.API;

public static class SafeFileAPI
{
    private static readonly char[] InvalidChars =
        Path.GetInvalidFileNameChars()
            .Concat(['/', '\\'])
            .Distinct()
            .ToArray();

    private static readonly string[] ReservedNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5",
        "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5",
        "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    /// <summary>
    ///     Sanitizes a given file name to ensure it doesn't conflict with any reserved names or contain invalid characters.
    ///     Invalid characters are replaced with underscores, and reserved names are prefixed with an underscore.
    /// </summary>
    /// <param name="input">The file name to sanitize.</param>
    /// <returns>A sanitized version of the file name that is safe to use in the filesystem.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "_";

        var builder = new StringBuilder(input.Length);

        foreach (var c in input)
            if (char.IsControl(c) || InvalidChars.Contains(c))
                builder.Append('_');
            else
                builder.Append(c);

        var result = builder.ToString().Trim().TrimEnd('.');

        if (string.IsNullOrWhiteSpace(result))
            return "_";

        // Windows reserves these names, even with extensions.
        var baseName = result.Split('.')[0];

        if (ReservedNames.Contains(
                baseName,
                StringComparer.OrdinalIgnoreCase))
            result = "_" + result;

        return result;
    }
}