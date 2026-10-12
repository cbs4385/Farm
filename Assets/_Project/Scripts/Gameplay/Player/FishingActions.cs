using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Casting a line and landing the catch.
    public sealed class FishingActions
    {
        public const int FishingEnergy = 5;

        readonly ActionContext _c;

        public FishingActions(ActionContext context) { _c = context; }

        Vector3Int _castCell;

        public void Cast(Vector3Int cell)
        {
            var spot = FishSpots.ForMap(_c.Map.MapId);
            if (spot == null || !_c.Map.IsWater(cell)) { _c.Session.Toast(L.Get("fishing.no_water")); return; }
            var ui = _c.Ui;
            if (ui == null || !_c.SpendEnergy(FishingEnergy)) return;
            var bait = _c.Session.Backpack.Has(FishDefaults.Bait);
            if (bait) _c.Session.Backpack.Remove(FishDefaults.Bait, 1);
            _castCell = cell;
            ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.75f, 0.9f, 1f), 1, Fx.Splash);          // the line hits the water
            var n = _c.Session.AddVar("stat.casts", 1);
            float R(int k) => WeatherRoller.Unit(n * 97 + k * 13, _c.Session.State.WorldSeed ^ 0x3C6EF372);
            var eligible = FishingModel.Eligible(FishDefaults.Rows, spot, _c.Session.World);
            var fishing = new FishingSession(eligible, _c.Session.GetSkillLevel(SkillIds.Fishing),
                _c.Session.Luck + Professions.LuckBonus(_c.Session.State, ProfessionEffect.LuckFishing), bait, R(1), R(2), R(3), R(4), Professions.BiteSpeed(_c.Session.State));
            FishingPose.For(_c.Body).Show(_c.Map.CellCenter(cell));
            ui.ShowFishing(fishing, OnFished);
        }

        void OnFished(FishingSession done)
        {
            FishingPose.For(_c.Body).Hide();
            if (done == null) return;
            if (!done.Caught || !done.Fish.HasValue) { _c.Session.Toast(L.Get("fishing.escaped")); return; }
            var fish = done.Fish.Value;
            ActionPuff.Burst(_c.Map.CellCenter(_castCell), new Color(0.75f, 0.9f, 1f), 4, Fx.WaterDrop);          // a fish breaks the surface
            if (_c.Session.Backpack.Add(fish.ItemId, 1, done.Quality) > 0) { _c.Session.Toast(L.Get("toast.inventory_full")); return; }
            _c.Hoist(fish.ItemId);
            _c.Session.AddVar("stat.fished", 1);
            _c.Session.AddSkillXp(SkillIds.Fishing, FishingModel.Xp(fish));
            _c.Session.Toast(L.Get(done.Perfect ? "fishing.perfect" : "fishing.caught", L.Get("item." + fish.ItemId + ".name")));
        }
    }
}
