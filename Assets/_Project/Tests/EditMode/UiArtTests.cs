using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The named HUD, effect and UI pictures that code asks for (UiArt) exist and are complete.
    public class UiArtTests
    {
        static UiArt Art() => Resources.Load<UiArt>(UiArt.ResourcePath);

        [Test]
        public void TheArtAsset_Exists_AndHoldsEveryHudFxAndUiPicture()
        {
            var art = Art();
            Assert.IsNotNull(art, "run Farm > Setup > Build UI Art");
            var files = Directory.GetFiles("Assets/_Project/Art/Placeholders", "*.png").Select(Path.GetFileNameWithoutExtension).Where(n => UiArtBuilder.Wanted(n + ".png"));
            foreach (var name in files) Assert.IsNotNull(art.Find(name), name + " is missing from UiArt: rebuild it");
        }

        [Test]
        public void EveryPictureTheCodeAsksFor_IsThere()
        {
            var art = Art();
            foreach (var name in new[] { "hud_clock_face", "hud_gold", "hud_health_icon", "hud_season_spring", "hud_season_summer", "hud_season_fall", "hud_season_winter",
                "hud_weather_sunny", "hud_weather_rain", "hud_weather_storm", "hud_weather_snow", "hud_weather_wind", "hud_weather_fog", "hud_weather_festival",
                "fx_puff_0", "fx_puff_1", "fx_puff_2", "fx_snowflake", "fx_wind_leaf" })
                Assert.IsNotNull(art.Find(name), name);
            foreach (var field in typeof(Farm.Gameplay.Fx).GetFields().Where(f => f.IsLiteral))
                Assert.IsNotNull(art.Find((string)field.GetRawConstantValue()), field.Name);
        }

        [Test]
        public void AnUnknownPicture_IsNull_NotAnError()
        {
            Assert.IsNull(UiArt.Get("hud_nothing_here"));
            Assert.IsNull(UiArt.Get(null));
        }
    }
}
