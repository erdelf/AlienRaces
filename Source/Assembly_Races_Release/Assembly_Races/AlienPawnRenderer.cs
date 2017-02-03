using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    internal static class AlienPawnRendererDetour
    {
        static FieldInfo pawnInfo = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo woundInfo = typeof(PawnRenderer).GetField("woundOverlays", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo statusInfo = typeof(PawnRenderer).GetField("statusOverlays", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo equipInfo = typeof(PawnRenderer).GetMethod("DrawEquipment", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public static void _RenderPawnInternal(this PawnRenderer _this, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, 
            Rot4 headFacing, RotDrawMode bodyDrawType = RotDrawMode.Fresh, bool portrait = false)
        {
            if (!_this.graphics.AllResolved)
            {
                _this.graphics.ResolveAllGraphics();
            }

            Mesh mesh = null;

            Pawn pawn = (Pawn) pawnInfo.GetValue(_this);

            if ((pawn is AlienPawn && (pawn as AlienPawn).bodySet == null))
                (pawn as AlienPawn).UpdateSets();

            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.005f;
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && _this.graphics.dessicatedGraphic != null && !portrait)
                {
                    _this.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        mesh = ((pawn is AlienPawn)? (pawn as AlienPawn).bodySet : MeshPool.humanlikeBodySet).MeshAt(bodyFacing); 
                    }
                    else
                    {
                        mesh = _this.graphics.nakedGraphic.MeshAt(bodyFacing);
                    }
                    List<Material> list = _this.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = _this.graphics.flasher.GetDamagedMat(list[i]);
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.005f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += 0.02f;
                        ((PawnWoundDrawer) woundInfo.GetValue(_this)).RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.03f;
                vector.y += 0.0249999985f;
            }
            else
            {
                a.y += 0.0249999985f;
                vector.y += 0.03f;
            }
            if (_this.graphics.headGraphic != null)
            {
                Vector3 b = quat * _this.BaseHeadOffsetAt(headFacing);
                Mesh mesh2 = ((pawn is AlienPawn) ? (pawn as AlienPawn).headSet:MeshPool.humanlikeHeadSet).MeshAt(headFacing);
                Material mat = _this.graphics.HeadMatAt(headFacing, bodyDrawType);
                GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, mat, portrait);
                Vector3 loc2 = rootLoc + b;
                loc2.y += 0.035f;
                bool flag = false;
                Mesh mesh3 = HairMeshSet(pawn).MeshAt(headFacing);
                List<ApparelGraphicRecord> apparelGraphics = _this.graphics.apparelGraphics;
                for (int j = 0; j < apparelGraphics.Count; j++)
                {
                    if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        flag = true;
                        Material material = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                        material = _this.graphics.flasher.GetDamagedMat(material);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material, portrait);
                    }
                }
                if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = HairMeshSet(pawn).MeshAt(headFacing);
                    Material mat2 = _this.graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat2, portrait);
                }
            }
            if (renderBody)
            {
                for (int k = 0; k < _this.graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = _this.graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material2 = _this.graphics.flasher.GetDamagedMat(material2);
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material2, portrait);
                    }
                }
            }
            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0)
            {
                Graphics.DrawMesh(mesh, vector, quat, _this.graphics.packGraphic.MatAt(pawn.Rotation, null), 0);
            }
            if (!portrait)
            {
                equipInfo.Invoke(_this, new object[] { rootLoc });
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int l = 0; l < wornApparel.Count; l++)
                    {
                        wornApparel[l].DrawWornExtras();
                    }
                }
                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += 0.0449999981f;
                ((PawnHeadOverlays) statusInfo.GetValue(_this)).RenderStatusOverlays(bodyLoc, quat, ((pawn is AlienPawn) ? (pawn as AlienPawn).headSet : MeshPool.humanlikeHeadSet).MeshAt(headFacing));
            }
        }

        static private GraphicMeshSet HairMeshSet(Pawn pawn)
        {
            CrownType crownType = pawn.story.crownType;
            if (crownType == CrownType.Average)
            {
                return pawn is AlienPawn?(pawn as AlienPawn).hairSetAverage:MeshPool.humanlikeHairSetAverage;
            }
            if (crownType == CrownType.Narrow)
            {
                return pawn is AlienPawn ? (pawn as AlienPawn).hairSetNarrow : MeshPool.humanlikeHairSetNarrow;
            }
            Log.Error("Unknown crown type: " + crownType);
            return pawn is AlienPawn ? (pawn as AlienPawn).hairSetAverage : MeshPool.humanlikeHairSetAverage;
        }
    }
}