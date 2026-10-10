using Newtonsoft.Json.Linq;

namespace Farm.Gameplay
{
    // Save version 1 to 2 (2026-10-10): the farm was flipped left to right so that the farmhouse stands by the village end of the map (a playtest asked for it: the
    // walk from the house to the road was long). Everything a save remembers on the farm by position is mirrored with it: tilled soil and crops, trees, rocks and
    // weeds, placed objects, the farm's buildings and the fixtures (shipping bin, hay...). The numbers here are frozen: this is what the farm was when the save was made.
    public sealed class FarmMirrorMigration : ISaveMigration
    {
        const int FarmW = 88;
        const string Farm = "Farm";

        public int FromVersion => 1;

        // The size, in cells, of what the saves can hold on the farm (a fixture or a building is saved by its bottom-left cell, so its width is needed).
        static int FixtureWidth(string id, int turns)
        {
            var wide = id == "shipping_bin" || id == "bed" || id == "furn_bed" ? 2 : id == "furn_couch" ? 2 : id == "furn_ruglarge" ? 2 : id == "furn_fireplace" ? 2 : 1;
            var tall = id == "shipping_bin" || id == "bed" || id == "furn_bed" ? 2 : id == "furn_ruglarge" ? 2 : id == "furn_fireplace" ? 2 : 1;
            return (turns & 1) == 1 ? tall : wide;
        }

        static int BuildingWidth(string typeId) => typeId == "greenhouse" ? 9 : typeId == "coop" ? 5 : typeId == "barn" ? 7 : 1;

        public void Migrate(JObject root)
        {
            if (root["Maps"] is JObject maps && maps[Farm] is JObject farm)
                foreach (var list in new[] { "Tiles", "Nodes", "Objects" })
                    if (farm[list] is JArray items)
                        foreach (var item in items) Mirror(item, 1);

            if (root["FarmBuildings"] is JArray buildings)
                foreach (var b in buildings) Mirror(b, BuildingWidth((string)b["TypeId"]));

            if (root["Fixtures"] is JArray fixtures)
                foreach (var f in fixtures)
                    if ((string)f["Map"] == Farm) Mirror(f, FixtureWidth((string)f["Id"], (int?)f["Turns"] ?? 0));
        }

        // x of the leftmost cell of something `width` cells wide, after the mirror.
        static void Mirror(JToken item, int width)
        {
            if (item is JObject o && o["X"] != null) o["X"] = FarmW - 1 - ((int)o["X"] + width - 1);
        }
    }
}
