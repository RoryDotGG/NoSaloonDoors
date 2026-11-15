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

    /// <summary>
    /// Whether a door already in the level should be taken out, by the name of the GameObject holding its hinges.
    /// Doors baked into a room keep the prefab's name (Saloon Doors). Spawned ones get Unity's " (Clone)" suffix.
    /// </summary>
    public static bool MatchesObjectName(string? objectName, string[] names)
    {
        var name = (objectName ?? "").Trim();
        const string clone = "(Clone)";
        if (name.EndsWith(clone, StringComparison.Ordinal)) name = name.Substring(0, name.Length - clone.Length).TrimEnd();
        return name.Length > 0 && names.Any(n => string.Equals(name, n, StringComparison.OrdinalIgnoreCase));
    }
}
