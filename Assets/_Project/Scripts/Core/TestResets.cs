using System;
using System.Collections.Generic;

namespace Farm.Core
{
    // Static caches and registries that tests must start from empty. A class with such state registers its reset here from its static constructor (so only classes
    // that were used have anything to reset), and Bootstrapper.ResetForTests runs them all: a new cache is cleared by one line next to it, not by every test.
    public static class TestResets
    {
        static readonly List<Action> Resets = new List<Action>();

        public static void Add(Action reset)
        {
            lock (Resets) { if (!Resets.Contains(reset)) Resets.Add(reset); }
        }

        public static void RunAll()
        {
            Action[] all;
            lock (Resets) all = Resets.ToArray();
            foreach (var reset in all) reset();
        }
    }
}
