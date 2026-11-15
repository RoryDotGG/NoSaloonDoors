using System;
using System.Linq;

namespace NoSaloonDoors;

/// <summary>Pure name matching (no Unity), covered by tests/DoorFilterCheck.</summary>
internal static class DoorFilter
{
    public const string DefaultNames = "Saloon Doors";

    /// <summary>The comma-separated PrefabNames setting as a list, trimmed, with blanks dropped.</summary>
    public static string[] ParseNames(string? setting) =>
        (setting ?? "").Split(',').Select(n => n.Trim()).Where(n => n.Length > 0).ToArray();

    /// <summary>
    /// Whether a doorway prop should be left out. A prop matches by its prefab name (Saloon Doors) or by the
    /// last part of its resource path (Level/Desert/Other/Saloon Doors), in any case.
    /// </summary>
    public static bool Matches(string? prefabName, string? resourcePath, string[] names)
    {
        var path = resourcePath ?? "";
        var leaf = path.Substring(path.LastIndexOf('/') + 1);
        return names.Any(n =>
            string.Equals(prefabName, n, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(leaf, n, StringComparison.OrdinalIgnoreCase));
    }
}
