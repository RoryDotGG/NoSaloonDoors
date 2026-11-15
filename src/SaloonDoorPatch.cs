using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace NoSaloonDoors;

/// <summary>
/// Sunstone Valley Resort fills some doorways between rooms with CustomConnectionObject, which spawns its prefab
/// in Setup (PhotonNetwork.InstantiateRoomObject in multiplayer). The game only calls Setup on the host or in
/// singleplayer, so skipping it for the saloon doors leaves them out for everyone in the lobby.
/// GameLibs predates the Sunstone update, so the class is found by name rather than with typeof.
/// </summary>
[HarmonyPatch]
internal static class SaloonDoorPatch
{
    private static MethodBase? Target => AccessTools.Method("CustomConnectionObject:Setup");

    private static bool Prepare()
    {
        if (Target != null) return true;
        Plugin.Log.LogWarning("CustomConnectionObject.Setup isn't in this game version, so the saloon doors stay. NoSaloonDoors needs the Sunstone update (v0.5.0) or later.");
        return false;
    }

    private static MethodBase TargetMethod() => Target!;

    private static bool Prefix(Component __instance, PrefabRef ___spawnObject, ref bool ___spawned)
    {
        if (!Plugin.Enabled.Value || ___spawnObject == null || !___spawnObject.IsValid()) return true;
        if (!DoorFilter.Matches(___spawnObject.PrefabName, ___spawnObject.ResourcePath, Plugin.Names)) return true;

        // Mark the doorway as filled. The connector on the room next door checks this and removes itself,
        // rather than spawning its own copy of the door.
        ___spawned = true;
        Plugin.Log.LogDebug($"Left out {___spawnObject.PrefabName} at {__instance.transform.position}");
        return false;
    }
}
