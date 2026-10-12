using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-12 (0.5.2 and after): the world's light follows the day between a tenth and full, the mine is dark enough to need a torch, the village lamps burn from
    // dusk to dawn, the mallet takes furniture into the backpack, and bait in fishing does more than hurry the bite.
    public class Release053Tests
    {
        [Test]
        public void TheLightRangeIsATenthToFull_WithADawnAndADusk()
        {
            Assert.AreEqual(0.1f, DayNightLighting.NightBrightness);
            Assert.Less(DayNightLighting.Brightness(7 * 60), 0.8f, "dawn");
            Assert.Greater(DayNightLighting.Brightness(7 * 60), 0.1f);
            Assert.AreEqual(1f, DayNightLighting.Brightness(12 * 60), 0.001f, "noon");
            Assert.Less(DayNightLighting.Brightness(19 * 60), 0.7f, "dusk");
            Assert.Greater(DayNightLighting.Brightness(19 * 60), 0.1f);
        }

        [Test]
        public void TheLight_IsGivenInLinearTerms_SoThatATenthLooksLikeATenth()
        {
            Assert.AreEqual(Color.white, DayNightLighting.AsLight(Color.white), "full light is unchanged");
            var night = DayNightLighting.AsLight(DayNightLighting.ColorAt(24 * 60));
            var strongest = Mathf.Max(night.r, night.g, night.b);
            Assert.AreEqual(Mathf.Pow(0.1f, DayNightLighting.ScreenPower), strongest, 0.0005f, "a tenth on the screen is far less in linear light");
            Assert.Greater(night.b, night.r, "the cool tint is kept");
            var dark = DayNightLighting.AsLight(DayNightLighting.Cave);
            Assert.Less(Mathf.Max(dark.r, dark.g, dark.b), 0.01f, "the mine");
            Assert.AreEqual(new Color(0f, 0f, 0f, 1f), DayNightLighting.AsLight(Color.black));
        }

        [Test]
        public void ADarkTenthInTheMine_SoThatTheTorchIsNeeded()
        {
            Assert.AreEqual(0.1f, DayNightLighting.CaveBrightness);
            var cave = DayNightLighting.Cave;
            Assert.LessOrEqual(Mathf.Max(cave.r, cave.g, cave.b), 0.101f);
            Assert.Greater(cave.b, cave.r, "still tinted blue");
        }

        [Test]
        public void AStoryScenesNightMood_IsNotAsDarkAsTheWorld()
        {
            var night = DayNightLighting.PresetColor("night");
            Assert.AreEqual(DayNightLighting.SceneNightBrightness, Mathf.Max(night.r, night.g, night.b), 0.001f);
            Assert.Greater(DayNightLighting.SceneNightBrightness, DayNightLighting.NightBrightness);
        }

        [Test]
        public void TheLamps_BurnFromDuskToDawn()
        {
            Assert.IsFalse(DayNightLighting.LampsOn(12 * 60), "noon");
            Assert.IsFalse(DayNightLighting.LampsOn(10 * 60), "morning");
            Assert.IsFalse(DayNightLighting.LampsOn(17 * 60 + 30), "late afternoon");
            Assert.IsTrue(DayNightLighting.LampsOn(20 * 60), "dusk");
            Assert.IsTrue(DayNightLighting.LampsOn(23 * 60), "night");
            Assert.IsTrue(DayNightLighting.LampsOn(26 * 60), "small hours");
            Assert.IsTrue(DayNightLighting.LampsOn(6 * 60 + 30), "dawn, still dark");
            Assert.IsFalse(DayNightLighting.LampsOn(9 * 60), "day");
        }

        [Test]
        public void TheMallet_TakesFurnitureDecorationsSprinklersAndScarecrowsIntoTheBackpack_NotChestsOrMachines()
        {
            Assert.IsTrue(HammerMode.TakesToBackpack(PlaceableKind.Decor));
            Assert.IsTrue(HammerMode.TakesToBackpack(PlaceableKind.Sprinkler));
            Assert.IsTrue(HammerMode.TakesToBackpack(PlaceableKind.Scarecrow));
            Assert.IsFalse(HammerMode.TakesToBackpack(PlaceableKind.Chest));
            Assert.IsFalse(HammerMode.TakesToBackpack(PlaceableKind.Machine));
        }

        static List<FishRow> Fish() => new List<FishRow>
        {
            new FishRow("common", "Common", "pond", SeasonMask.All, 10f, 0.1f, 10, Color.white),
            new FishRow("rare", "Rare", "pond", SeasonMask.All, 1f, 0.6f, 100, Color.white),
        };

        [Test]
        public void Bait_WidensTheZone_AndTheBiteComesSooner()
        {
            Assert.Greater(FishingModel.ZoneWidth(1, 0.3f, true), FishingModel.ZoneWidth(1, 0.3f, false));
            Assert.LessOrEqual(FishingModel.ZoneWidth(20, 0f, true), 0.6f, "never wider than the bar allows");
            Assert.Less(FishingModel.BiteDelay(1, true, 0.5f), FishingModel.BiteDelay(1, false, 0.5f));
        }

        [Test]
        public void Bait_MakesTheRarerFishLikelier()
        {
            var table = Fish();
            var rareWithout = 0;
            var rareWith = 0;
            for (var i = 0; i < 1000; i++)
            {
                var roll = i / 1000f;
                if (FishingModel.Pick(table, 0f, roll).Value.Id == "rare") rareWithout++;
                if (FishingModel.Pick(table, FishingModel.BaitLuck, roll).Value.Id == "rare") rareWith++;
            }
            Assert.Greater(rareWith, rareWithout);
        }

        [Test]
        public void ASessionRemembersWhetherItWasBaited_AndUsesIt()
        {
            var table = new List<FishRow> { new FishRow("common", "Common", "pond", SeasonMask.All, 1f, 0.3f, 10, Color.white) };
            var plain = new FishingSession(table, 1, 0f, false, 0.5f, 0.5f, 0.5f, 0.5f);
            var baited = new FishingSession(table, 1, 0f, true, 0.5f, 0.5f, 0.5f, 0.5f);
            Assert.IsFalse(plain.Baited);
            Assert.IsTrue(baited.Baited);
            Assert.Greater(baited.ZoneWidth, plain.ZoneWidth);
        }
    }
}
