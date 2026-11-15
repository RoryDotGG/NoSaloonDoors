using NoSaloonDoors;
using static NoSaloonDoors.DoorFilter;

// Assert-based self-check for the door name matching. Run: dotnet run --project tests/DoorFilterCheck
void Check(bool ok, string what) { if (!ok) throw new Exception("FAIL: " + what); Console.WriteLine("ok  " + what); }
bool Same(IEnumerable<string> a, params string[] b) => a.SequenceEqual(b);

var defaults = ParseNames(DefaultNames);
const string SaloonPath = "Level/Desert/Other/Saloon Doors";

// ---- parsing the setting ----

Check(Same(defaults, "Saloon Doors"), "the default is just the saloon doors");
Check(Same(ParseNames(" Saloon Doors , Desert Door "), "Saloon Doors", "Desert Door"), "names are split on commas and trimmed");
Check(ParseNames("").Length == 0 && ParseNames(null).Length == 0 && ParseNames(" , ,").Length == 0, "an empty setting names nothing");

// ---- matching ----

// The saloon doors, as the Sunstone update's level data names them.
Check(Matches("Saloon Doors", SaloonPath, defaults), "matches the saloon doors by name and path");
Check(Matches("", SaloonPath, defaults), "matches by path when the name is blank");
Check(Matches("Saloon Doors", null, defaults), "matches by name when there's no path");
Check(Matches("SALOON DOORS", "level/desert/other/saloon doors", defaults), "matching ignores case");

// Sunstone Valley's other doors, and doors on other maps, stay.
foreach (var door in new[] { "Desert Adobe Door", "Desert Adobe Door Mine", "Desert Adobe Door STAFF", "Desert Adobe Door WC", "Desert Door" })
    Check(!Matches(door, "Level/Desert/Other/" + door, defaults), $"leaves {door} alone");
Check(!Matches("Saloon Doors Frame", "Level/Desert/Other/Saloon Doors Frame", defaults), "a longer name isn't a match");
Check(!Matches("Doors", "Level/Desert/Other/Doors", defaults), "a shorter name isn't a match");
Check(!Matches("Desert Door", "Level/Desert/Other/Desert Door", ParseNames("Other")), "only the last part of the path counts");
Check(!Matches(null, null, defaults), "a prop with no name or path is left alone");

// An empty setting turns the mod into a no-op, and extra names add more doors.
Check(!Matches("Saloon Doors", SaloonPath, ParseNames("")), "an empty setting leaves every door");
var both = ParseNames("Saloon Doors, Desert Adobe Door WC");
Check(Matches("Desert Adobe Door WC", "Level/Desert/Other/Desert Adobe Door WC", both) && Matches("Saloon Doors", SaloonPath, both),
    "every listed name is left out");

Console.WriteLine("all checks passed");
