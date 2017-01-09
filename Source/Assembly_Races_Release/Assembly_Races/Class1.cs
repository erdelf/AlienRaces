using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Assembly_Races
{
    class PawnGraphicSetAlien : PawnGraphicSet
    {
        public void ResolveAllGraphicsAlien()
        {
            this.ClearCache();
            if (this.pawn.RaceProps.Humanlike)
            {
                this.nakedGraphic = GraphicGetter_NakedAlien.GetNakedBodyGraphicAlien(this.pawn.story.BodyType, ShaderDatabase.CutoutSkin, this.pawn.story.SkinColor, ThingDef_AlienRace.);
                this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(this.pawn.story.BodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor);
                this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, this.pawn.story.SkinColor);
                this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, PawnGraphicSet.RottingColor);
                this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(this.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, this.pawn.story.hairColor);
                this.ResolveApparelGraphics();
            }
            else
            {
                PawnKindLifeStage curKindLifeStage = this.pawn.ageTracker.CurKindLifeStage;
                if (this.pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
                {
                    this.nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
                }
                else
                {
                    this.nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
                }
                this.rottingGraphic = this.nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor);
                if (curKindLifeStage.dessicatedBodyGraphicData != null)
                {
                    this.dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(this.pawn);
                }
            }
        }



    }
}
