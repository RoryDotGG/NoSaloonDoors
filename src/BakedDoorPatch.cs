using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace NoSaloonDoors;

/// <summary>
/// Most of Sunstone Valley Resort's saloon doors aren't spawned by CustomConnectionObject: they're part of the room
/// prefab itself (…/Desert Shack Wall - 1x1 - Door/Door 01 Frame/Saloon Doors), so SaloonDoorPatch never sees them.
/// Once the level has generated, the host breaks each of their hinges the way an enemy or explosion would, through
/// the game's own RPCs, so the doors go for every player whether or not they have the mod.
/// GameLibs predates the Sunstone update, so the newer methods are found by name rather than called directly.
/// </summary>
[HarmonyPatch]
internal static class BakedDoorPatch
{
    // Runs on every player once all of them have the level loaded. Generated is set by the time the postfix runs.
    private static MethodBase? Target => AccessTools.Method("LevelGenerator:GenerateDoneApply");

    // DestroyObject(bool effects, bool forced) since Sunstone; DestroyObject(bool effects) before it.
    private static readonly MethodInfo? DestroyObject =
        AccessTools.Method(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.DestroyObject));

    private static bool Prepare()
    {
        if (Target != null) return true;
        Plugin.Log.LogWarning("LevelGenerator.GenerateDoneApply isn't in this game version, so saloon doors built into rooms stay. NoSaloonDoors needs the Sunstone update (v0.5.0) or later.");
        return false;
    }

    private static MethodBase TargetMethod() => Target!;

    private static void Postfix()
    {
        if (!Plugin.Enabled.Value || Plugin.Names.Length == 0 || !SemiFunc.IsMasterClientOrSingleplayer()) return;

        var removed = 0;
        foreach (var hinge in Object.FindObjectsOfType<PhysGrabHinge>())
        {
            if (!IsLeftOut(hinge)) continue;

            // DestroyHinge marks the hinge dead on every player, which clears the doorway's wall tag and sound
            // muffling. Left alone it would then destroy the door with a crash and camera shake, so destroy it
            // straight away without effects instead. The hinge's own later call does nothing once it's dead.
            hinge.DestroyHinge();
            DestroyQuietly(hinge.GetComponent<PhysGrabObjectImpactDetector>());
            removed++;
        }
        if (removed > 0) Plugin.Log.LogInfo($"Took out {removed} saloon door hinge(s) built into the level");
    }

    /// <summary>Whether a hinge belongs to one of the doors in PrefabNames, by the name of the object holding it.</summary>
    internal static bool IsLeftOut(PhysGrabHinge hinge)
    {
        var parent = hinge.transform.parent;
        return parent != null && DoorFilter.MatchesObjectName(parent.name, Plugin.Names);
    }

    private static void DestroyQuietly(PhysGrabObjectImpactDetector? detector)
    {
        if (detector == null || DestroyObject == null) return;
        // effects: false skips the crash sound and camera shake. forced: true goes ahead even if the door is marked
        // as indestructible. Anything else keeps its default.
        var args = DestroyObject.GetParameters()
            .Select(p => p.Name switch { "effects" => false, "forced" => true, _ => p.DefaultValue })
            .ToArray();
        DestroyObject.Invoke(detector, args);
    }
}

/// <summary>
/// The door still bursts into wood bits when it's destroyed, even without effects. One of the doors is on the start
/// room's entrance, in full view as the level starts, so skip the burst for these doors on every player with the mod.
/// Players without it see the bits, which clear in a few seconds.
/// </summary>
[HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.DestroyObjectRPC))]
internal static class QuietDoorBreakPatch
{
    private static void Prefix(PhysGrabObjectImpactDetector __instance, ref PhysObjectParticles ___particles)
    {
        if (!Plugin.Enabled.Value || ___particles == null) return;
        var hinge = __instance.GetComponent<PhysGrabHinge>();
        if (hinge != null && BakedDoorPatch.IsLeftOut(hinge)) ___particles = null!;
    }
}
