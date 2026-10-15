namespace Farm.Gameplay
{
    // Where a saved game lives. Stored per slot in saves/slotN/location.json; a slot without that file is Local, so every
    // save made before cloud saves existed stays valid and local.
    public enum SaveLocation
    {
        Local = 0,
        Cloud = 1,
    }
}
