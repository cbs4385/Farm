using System;

namespace Farm.Gameplay
{
    // How each villager feels about each other villager (docs/narrative/RELATIONSHIPS.md, section 4). Authored writing guidance kept as data so
    // tests can hold the gossip topics and the document to it; it is not read during play. Row = who feels it, column = about whom, in
    // NpcIds.All order. Cozy layer only: allegiances and anything secret live in Farm.Mythos.
    public enum Attitude { Unwritten, Distant, Cordial, Warm, Close, Rivalry, Shy }

    public static class VillagerRelations
    {
        // 3 close, 2 warm, 1 cordial, 0 polite distance, R affectionate rivalry, S shy or leaving alone, '.' not written yet, '-' themselves.
        //                                  Til Bra Ion Mar Pen Wre Fel Jun Haz Pip Dor Ela
        static readonly string[] Rows =
        {
            "-2111R331R11",   // tilda
            "2-1311231100",   // bram
            ".1-1212231SS",   // ione
            "231-R1.20.1.",   // marcus
            "1122-2..12.R",   // odalys
            "R22R2-233312",   // wren
            "321..2-..S.2",   // felix
            "3312.3.-2312",   // juno
            "00320302-122",   // hazel
            "R11.03S31-S2",   // piper
            "10S1.1.12.-S",   // dorian
            "2.S.R22222S-",   // elara
        };

        public static Attitude Of(string from, string about)
        {
            int row = Array.IndexOf(NpcIds.All, from), col = Array.IndexOf(NpcIds.All, about);
            if (row < 0 || col < 0 || row == col) return Attitude.Unwritten;
            switch (Rows[row][col])
            {
                case '0': return Attitude.Distant;
                case '1': return Attitude.Cordial;
                case '2': return Attitude.Warm;
                case '3': return Attitude.Close;
                case 'R': return Attitude.Rivalry;
                case 'S': return Attitude.Shy;
                default: return Attitude.Unwritten;
            }
        }

        public static bool IsWritten(string from, string about) => Of(from, about) != Attitude.Unwritten;

        public static string[] RawRows => (string[])Rows.Clone();
    }
}
