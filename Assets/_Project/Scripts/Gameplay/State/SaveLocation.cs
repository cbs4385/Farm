namespace Farm.Gameplay
{
    // Where a saved game lives. Stored per slot in saves/slotN/location.json; a slot without that file is Local, so every
    // save made before cloud saves existed stays valid and local.
    public enum SaveLocation
    {
        Local = 0,
        Cloud = 1,
    }

    // The new-game "Save to" row. It is always shown (a game started outside Steam used to show no row, and nothing said why the cloud was
    // not offered); without Steam Cloud it shows this computer and says so, and cannot be changed.
    public static class NewGameWhere
    {
        public static bool CanChoose(bool cloudAvailable) => cloudAvailable;

        public static SaveLocation Shown(bool cloudAvailable, SaveLocation wanted) => cloudAvailable ? wanted : SaveLocation.Local;

        public static string LabelKey(bool cloudAvailable, SaveLocation wanted) =>
            !cloudAvailable ? "newgame.where.unavailable"
            : wanted == SaveLocation.Cloud ? "newgame.where.cloud" : "newgame.where.local";
    }
}
