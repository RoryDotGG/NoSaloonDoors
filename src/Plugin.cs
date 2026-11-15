using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace NoSaloonDoors;

[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "RoryDotGG.NoSaloonDoors";
    public const string Name = "NoSaloonDoors";
    /// <summary>Set from Version in NoSaloonDoors.csproj at build time. Bump it there.</summary>
    public const string Version = BuildInfo.Version;

    internal static ManualLogSource Log = null!;
    internal static ConfigEntry<bool> Enabled = null!;
    internal static ConfigEntry<string> PrefabNames = null!;

    /// <summary>PrefabNames, split and trimmed. Kept up to date when REPOConfig changes the setting.</summary>
    internal static string[] Names { get; private set; } = [];

    private void Awake()
    {
        Log = Logger;
        Enabled = Config.Bind("General", "Enabled", true,
            "Leave the saloon doorways in Sunstone Valley Resort open. Only the host needs the mod: the doors are never spawned, so nobody in the lobby gets them. Applies from the next level.");
        PrefabNames = Config.Bind("General", "PrefabNames", DoorFilter.DefaultNames,
            "The doorway props to leave out, as a comma-separated list of prefab names, in any case. Saloon Doors is the only one this mod is meant for, but any door a level places with CustomConnectionObject can go here. Applies from the next level.");

        Names = DoorFilter.ParseNames(PrefabNames.Value);
        PrefabNames.SettingChanged += (_, _) => Names = DoorFilter.ParseNames(PrefabNames.Value);

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(SaloonDoorPatch));
        harmony.PatchAll(typeof(BakedDoorPatch));
        harmony.PatchAll(typeof(QuietDoorBreakPatch));
        Log.LogInfo($"{Name} {Version} loaded");
    }
}
