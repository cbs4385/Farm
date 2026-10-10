using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // A withered crop (blight, frost) is drawn as a withered plant, does not sway, and disappears when cleared.
    public class WitheredCropFlowTests : PlayModeFixture
    {

        [UnityTest]
        public IEnumerator AWitheredCrop_ShowsTheBlightSprite_AndClearingRemovesIt()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var view = UnityEngine.Object.FindAnyObjectByType<FarmMapView>();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var grid = session.GetGrid(MapIds.Farm);
            session.Db.TryGetCrop("parsnip", out var parsnip);
            grid.Till(21, 10);
            grid.Plant(21, 10, parsnip, Season.Spring);
            grid.TryGetTile(21, 10, out var tile);
            tile.Crop.Stage = parsnip.MatureStage;
            var cell = new Vector3Int(21, 10, 0);
            view.RefreshAll();
            Assert.AreEqual(parsnip.SpriteForStage(parsnip.MatureStage), map.Crops.GetSprite(cell), "alive: the ordinary picture");

            grid.Wither(21, 10);
            view.RefreshCell(cell);
            var shown = map.Crops.GetSprite(cell);
            Assert.AreEqual(parsnip.SpriteForWithered(parsnip.MatureStage), shown);
            StringAssert.StartsWith("crop_blight_", shown.name, "it is the blight art, not the live plant");

            view.ApplySway(0.4f);
            Assert.AreEqual(Matrix4x4.identity, map.Crops.GetTransformMatrix(cell), "a withered plant does not sway");

            grid.ClearCrop(21, 10);
            view.RefreshCell(cell);
            Assert.IsNull(map.Crops.GetSprite(cell));
        }
    }
}
