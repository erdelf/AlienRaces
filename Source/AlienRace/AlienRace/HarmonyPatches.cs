using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.erdelf.alien_race.main");

            harmony.Patch(AccessTools.Method(typeof(PawnHairChooser), "RandomHairDefFor"), new HarmonyMethod(typeof(HarmonyPatches), "RandomHairDefForPrefix"), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) }), new HarmonyMethod(typeof(HarmonyPatches), "GeneratePawnPrefix"), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateBodyTypePostfix"));
            harmony.Patch(AccessTools.Property(typeof(Pawn_StoryTracker), "SkinColor").GetGetMethod(), null, new HarmonyMethod(typeof(HarmonyPatches), "SkinColorPostfix"));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateTraits"), new HarmonyMethod(typeof(HarmonyPatches), "GenerateTraitsPrefix"), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), new HarmonyMethod(typeof(HarmonyPatches), "ResolveAllGraphicsPrefix"), null);
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "TryGiveSolidBioTo"), new HarmonyMethod(typeof(HarmonyPatches), "TryGiveSolidBioToPrefix"), null);
            //harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "SetBackstoryInSlot"), new HarmonyMethod(typeof(HarmonyPatches), "SetBackstoryInSlotPrefix"), null);
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "GiveShuffledBioTo"), null, new HarmonyMethod(typeof(HarmonyPatches), "GiveShuffledBioToPostfix"));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] {typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool)}), new HarmonyMethod(typeof(HarmonyPatches), "RenderPawnInternalPrefix"), null);

            DefDatabase<HairDef>.GetNamed("Shaved").hairTags.Add("alienNoHair"); // needed because..... the original idea doesn't work and I spend enough time finding a good solution
        }

        public static void GiveShuffledBioToPostfix(ref Pawn pawn)
        {
            CompProperties_Alien alienProps = pawn.def.GetCompProperties<CompProperties_Alien>();

            if (alienProps != null && alienProps.PawnsSpecificBackstories && !pawn.kindDef.backstoryCategory.NullOrEmpty())
            {
                Pawn pawn2 = pawn;
                foreach (BackstorySlot slot in Enum.GetValues(typeof(BackstorySlot)))
                {
                    IEnumerable<Backstory> backstories = BackstoryDatabase.allBackstories.Where(kvp => kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawn2.kindDef.backstoryCategory) &&
                        kvp.Value.slot == slot && (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn2.story.childhood.workDisables))).Select(kvp => kvp.Value);
                    Backstory b;
                    if (backstories.TryRandomElement(out b))
                        if (slot == BackstorySlot.Childhood)
                            pawn.story.childhood = b;
                        else
                            pawn.story.adulthood = b;
                }
            }
        }
        
        public static bool SetBackstoryInSlotPrefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {

            CompProperties_Alien alienProps = pawn.def.GetCompProperties<CompProperties_Alien>();

            if (alienProps != null && alienProps.PawnsSpecificBackstories && !pawn.kindDef.backstoryCategory.NullOrEmpty())
            {
                /*
                IEnumerable<Backstory> backstories = BackstoryDatabase.allBackstories.Where(kvp => kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory) &&
                kvp.Value.slot == slot && (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))).Select(kvp => kvp.Value);
                if (backstories.TryRandomElement(out backstory))
                */


                /*if((from kvp in BackstoryDatabase.allBackstories
                 where kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory) && kvp.Value.slot == slot && (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))
                 select kvp.Value).TryRandomElement(out backstory))*/

                List<Backstory> backstories = new List<Backstory>();
                foreach (KeyValuePair<string, Backstory> kvp in BackstoryDatabase.allBackstories)
                {
                    if (kvp.Value.shuffleable)
                    {
                        if (kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory))
                        {
                            if (kvp.Value.slot == slot)
                            {
                                if (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))
                                {
                                    backstories.Add(kvp.Value);
                                }
                            }
                        }
                    }
                }


                if (!backstories.NullOrEmpty() && backstories.TryRandomElement(out backstory))
                    return false;
            }
            return true;
        }

        public static bool TryGiveSolidBioToPrefix(Pawn pawn, ref bool __result)
        {
            if(pawn.TryGetComp<CompAlien>() != null)
            {
                __result = false;
                return false;
            }
            return true;
        }

        public static void GiveShortHashPrefix(ref Def def)
        {
            if (def is BackstoryDef)
                if (def.shortHash != 0)
                    def.shortHash = 0;
        }

        public static bool ResolveAllGraphicsPrefix(PawnGraphicSet __instance)
        {
            CompAlien alienComps = __instance.pawn.TryGetComp<CompAlien>();
            CompProperties_Alien alienProps = __instance.pawn.def.GetCompProperties<CompProperties_Alien>();
            if (alienProps != null)
            {
                __instance.nakedGraphic = AlienRaceUtilties.GetNakedGraphic(__instance.pawn.story.bodyType, ShaderDatabase.Cutout, __instance.pawn.story.SkinColor, alienProps.NakedBodyGraphicLocation);
                __instance.rottingGraphic = AlienRaceUtilties.GetNakedGraphic(__instance.pawn.story.bodyType, ShaderDatabase.Cutout, PawnGraphicSet.RottingColor, alienProps.NakedBodyGraphicLocation);
                __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(alienProps.DesiccatedGraphicLocation, ShaderDatabase.Cutout);
                __instance.headGraphic = !alienProps.NakedHeadGraphicLocation.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.HeadGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, __instance.pawn.story.SkinColor) : null;
                __instance.desiccatedHeadGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.HeadGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, PawnGraphicSet.RottingColor);
                __instance.skullGraphic = GraphicDatabase.Get<Graphic_Multi>(alienProps.SkullGraphicLocation, ShaderDatabase.CutoutSkin, Vector2.one, Color.white);
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, __instance.pawn.story.hairColor);

                __instance.ResolveApparelGraphics();
                return false;
            }
            else
                return true;
        }

        public static void GenerateTraitsPrefix(Pawn pawn)
        {
            Log.Message("start");
            Log.Message((pawn != null).ToString());
            Log.Message((pawn.Name != null).ToString());
            Log.Message(pawn.Name?.ToStringFull);
            Log.Message("after check");
            CompProperties_Alien alienProps = pawn.def.GetCompProperties<CompProperties_Alien>();
            if (alienProps != null && !alienProps.ForcedRaceTraitEntries.NullOrEmpty())
            {
                foreach (AlienTraitEntry ate in alienProps.ForcedRaceTraitEntries)
                {
                    if (Rand.Range(0, 100) < ate.chance)
                        pawn.story.traits.GainTrait(new Trait(TraitDef.Named(ate.defname), ate.degree, true));
                }
            }
            Log.Message("end");
        }

        public static void SkinColorPostfix(Pawn_StoryTracker __instance, ref Color __result)
        {
            CompAlien alienComp = ((Pawn)AccessTools.Field(typeof(Pawn_StoryTracker), "pawn").GetValue(__instance)).TryGetComp<CompAlien>();
            if (alienComp != null)
                __result = alienComp.skinColor;
        }

        public static void GenerateBodyTypePostfix(ref Pawn pawn)
        {
            CompProperties_Alien alienProps = pawn.def.GetCompProperties<CompProperties_Alien>();
            if (alienProps != null && !alienProps.partgenerator.alienbodytypes.NullOrEmpty() && !alienProps.partgenerator.alienbodytypes.Contains(pawn.story.bodyType))
                pawn.story.bodyType = alienProps.partgenerator.alienbodytypes.RandomElement();
        }


        static FactionDef hairFaction = new FactionDef() { hairTags = new List<string>() { "alienNoHair" } };

        public static void RandomHairDefForPrefix(Pawn pawn, ref FactionDef factionType)
        {
            CompProperties_Alien alienProps = pawn.def.GetCompProperties<CompProperties_Alien>();
            if (alienProps != null && !alienProps.HasHair)
            {
                factionType = hairFaction;
                //Log.Message(pawn.def.defName);
            }
        }

        public static void GeneratePawnPrefix(ref PawnGenerationRequest request)
        {
            if ((request.KindDef == PawnKindDefOf.SpaceRefugee || request.KindDef == PawnKindDefOf.Slave) && Rand.Value > 0.4f)
            {
                IEnumerable<ThingDef> races = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasComp(typeof(CompAlien)) && d.GetCompProperties<CompProperties_Alien>().RandomlyGenerated);
                if (races.Any())
                    request.KindDef.race = races.RandomElement();
            }
            CompProperties_Alien alienProps = request.KindDef.race.GetCompProperties<CompProperties_Alien>();
            if (alienProps != null && request.KindDef.RaceProps.hasGenders)
            {
                Gender gender = request.FixedGender ?? Gender.Male;
                if (request.FixedGender.HasValue)
                {
                    if (alienProps.MaleGenderProbability <= 0f)
                    {
                        if (request.FixedGender.Value == Gender.Male)
                            gender = Gender.Female;
                    }
                    else if (alienProps.MaleGenderProbability >= 100f)
                        if (request.FixedGender.Value == Gender.Female)
                            gender = Gender.Male;
                }
                else
                    gender = Rand.RangeInclusive(0, 100) >= alienProps.MaleGenderProbability ? Gender.Female : Gender.Male;

                request = new PawnGenerationRequest(request.KindDef, request.Faction, request.Context, request.Map, request.ForceGenerateNewPawn, request.Newborn, 
                    request.AllowDead, request.AllowDead, request.CanGeneratePawnRelations, request.MustBeCapableOfViolence, request.ColonistRelationChanceFactor, 
                    request.ForceAddFreeWarmLayerIfNeeded, request.AllowGay, request.AllowFood, request.Validator, request.FixedBiologicalAge, 
                    request.FixedChronologicalAge, new Gender?(gender), request.FixedMelanin, request.FixedLastName);
            }
        }

        /**
         * Don't continue here... this implementation is so close to a detour that I hate it
         **/
        public static bool RenderPawnInternalPrefix(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(PawnRenderer), "pawn").GetValue(__instance);
            CompAlien alienComp = pawn.TryGetComp<CompAlien>();
            if (alienComp == null || portrait)
                return true;


            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.005f;

                if (bodyDrawType == RotDrawMode.Rotting)
                    __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                else
                    alienComp.bodySet.MeshAt(bodyFacing);

                List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                for (int i = 0; i < list.Count; i++)
                {
                    Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                    loc.y += 0.005f;
                }
                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += 0.02f;
                    ((PawnWoundDrawer)AccessTools.Field(typeof(PawnRenderer), "woundOverlays").GetValue(__instance)).RenderOverBody(drawLoc, mesh, quat, portrait);
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
            if (__instance.graphics.headGraphic != null)
            {
                Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                Mesh mesh2 = alienComp.headSet.MeshAt(headFacing);
                Material mat = __instance.graphics.HeadMatAt(headFacing, bodyDrawType);
                GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, mat, portrait);
                Vector3 loc2 = rootLoc + b;
                loc2.y += 0.035f;
                bool flag = false;
                Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                for (int j = 0; j < apparelGraphics.Count; j++)
                {
                    if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        flag = true;
                        Material material = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                        material = __instance.graphics.flasher.GetDamagedMat(material);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material, portrait);
                    }
                }
                if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat2 = __instance.graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat2, portrait);
                }
            }
            if (renderBody)
            {
                for (int k = 0; k < __instance.graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = __instance.graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material2, portrait);
                    }
                }
            }


            AccessTools.Method(typeof(PawnRenderer), "DrawEquipment").Invoke(__instance, new object[] { rootLoc });
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
            ((PawnHeadOverlays)AccessTools.Field(typeof(PawnRenderer), "statusOverlays").GetValue(__instance)).RenderStatusOverlays(bodyLoc, quat, alienComp.headSet.MeshAt(headFacing));

            return false;
        }
    }
}