using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Builds the dressing of the deep mine (DepthsDecor's plan) into the scene when a mine floor from the relic chest's down loads (the map hook). Everything is made at run time
    // from "Crawling Depths" pictures; nothing is saved, and the floor is rebuilt (and the dressing with it) each day. At horror off nothing is built.
    public static class MythosDepths
    {
        public static void OnMapLoaded(MapLoadedContext ctx)
        {
            var s = ctx.Session;
            if (ctx.MapId != MapIds.Mine || !MythosLevel.On(s) || s.State.Mine.Floor < DepthsDecor.FirstFloor) return;
            var mine = Object.FindAnyObjectByType<MineController>();
            if (mine == null || mine.Floor == null) return;
            Build(ctx.Map, DepthsDecor.Plan(mine.Floor, s.State.WorldSeed, MythosLevel.Of(s), s.GetVar(MythosIds.Vars.Dread)));
        }

        public static Transform Build(FarmMap map, IReadOnlyList<DepthsPiece> plan)
        {
            var root = new GameObject("Depths").transform;
            for (var i = 0; i < plan.Count; i++) Spawn(root, map, plan[i], i);
            return root;
        }

        static void Spawn(Transform root, FarmMap map, DepthsPiece p, int index)
        {
            var go = new GameObject(p.Kind.ToString());
            go.transform.SetParent(root, false);
            go.transform.position = map.CellCenter(new Vector3Int(p.X, p.Y, 0));
            var art = new GameObject("Art", typeof(SpriteRenderer));
            art.transform.SetParent(go.transform, false);
            var sr = art.GetComponent<SpriteRenderer>();
            LitSprites.Apply(sr);
            sr.sortingOrder = 5;
            var phase = index * 1.7f;
            var foot = new Vector3(0f, -0.5f, 0f);              // the foot of a standing picture at the foot of its cell
            switch (p.Kind)
            {
                case DepthsKind.FloorPatch:
                    sr.sprite = DepthsArt.Tile(p.Variant);
                    sr.sortingOrder = 1;
                    break;
                case DepthsKind.Vein:
                    sr.sprite = DepthsArt.Frame(VeinSheet(p.Variant), DepthsArt.VeinSize, DepthsArt.VeinSize, 0);
                    go.AddComponent<DepthsProp>().Init(sr, VeinSheet(p.Variant), DepthsArt.VeinSize, DepthsArt.VeinSize, 6f, phase);
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.LittleEyes:
                    go.AddComponent<DepthsProp>().Init(sr, "littleeyes_" + (p.Variant % 2 + 1), DepthsArt.LittleSize, DepthsArt.LittleSize, 6f, phase);
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.Eye:
                    go.AddComponent<DepthsProp>().Init(sr, "eye_" + (p.Variant % 2 + 1), DepthsArt.EyeSize, DepthsArt.EyeSize, 5f, phase);
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.Urn:
                    sr.sprite = DepthsArt.Whole("urn_" + (p.Variant % 2 + 1));
                    sr.sortingOrder = 2;
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.Statue:
                    sr.sprite = DepthsArt.Whole(StatueSheet(p.Variant));
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.ColumnShort:
                    sr.sprite = DepthsArt.Whole("column_short_" + (p.Variant % 2 + 1));
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.ColumnTall:
                    sr.sprite = DepthsArt.Whole("column_tall_" + (p.Variant % 2 + 1));
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.Arc:
                    sr.sprite = DepthsArt.Whole("arc_" + (p.Variant % 2 + 1));
                    sr.transform.localPosition = foot;
                    break;
                case DepthsKind.BookAltar:
                    sr.sprite = DepthsArt.Whole("book_altar");
                    sr.transform.localPosition = foot;
                    break;
            }
            if (p.Solid) go.AddComponent<BoxCollider2D>().size = Vector2.one;
            if (sr.sprite == null && go.GetComponent<DepthsProp>() == null) Object.Destroy(go);          // a missing sheet leaves the piece out
        }

        public static string VeinSheet(int variant) => new[] { "vein_a1", "vein_a2", "vein_b1", "vein_b2" }[((variant % 4) + 4) % 4];

        public static string StatueSheet(int variant) => new[] { "statue_a1", "statue_a2", "statue_b1", "statue_b2", "statue_c1", "statue_c2" }[((variant % 6) + 6) % 6];
    }
}
