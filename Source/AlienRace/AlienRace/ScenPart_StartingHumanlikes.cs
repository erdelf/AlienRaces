using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    sealed class ScenPart_StartingHumanlikes : ScenPart
    {
        
        static ScenPart_StartingHumanlikes()
        {
            ScenPartDef scenPart = new ScenPartDef()
            {
                defName = "StartingHumanlikes",
                label = "Start with humanlikes",
                scenPartClass = typeof(ScenPart_StartingHumanlikes),
                category = ScenPartCategory.StartingImportant,
                selectionWeight = 1.0f,
                summaryPriority = 10
            };
            scenPart.ResolveReferences();
            scenPart.PostLoad();
            DefDatabase<ScenPartDef>.Add(scenPart);
        }

        PawnKindDef kindDef = PawnKindDefOf.Villager;
        int pawnCount = 0;

        string buffer;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, RowHeight * 3f);
            if (Widgets.ButtonText(scenPartRect.TopPart(0.45f), this.kindDef.label.CapitalizeFirst(), true, false, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.AddRange(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar => ar.alienRace.pawnKindSettings.startingColonists != null).SelectMany(ar => ar.alienRace.pawnKindSettings.startingColonists.SelectMany(ste => ste.pawnKindEntries.SelectMany(pke => pke.kindDefs))).Where(s => DefDatabase<PawnKindDef>.GetNamedSilentFail(s) != null).Select(pkd => DefDatabase<PawnKindDef>.GetNamedSilentFail(pkd)).Select(pkd => new FloatMenuOption(pkd.label.CapitalizeFirst(), () => this.kindDef = pkd)));
                list.Add(new FloatMenuOption("Villager", () => this.kindDef = PawnKindDefOf.Villager));
                list.Add(new FloatMenuOption("Slave", () => this.kindDef = PawnKindDefOf.Slave));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.TextFieldNumeric(scenPartRect.BottomPart(0.45f), ref this.pawnCount, ref this.buffer, 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.pawnCount, "alienRaceScenPawnCount", 0);
            Scribe_Defs.Look(ref this.kindDef, "PawnKindDefAlienRaceScen");
        }

        public override string Summary(Scenario scen) => ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "PlayerStartsWith")
            {
                yield return this.kindDef.LabelCap + " x" + this.pawnCount;
            }

            yield break;
        }

        public override bool TryMerge(ScenPart other)
        {
            ScenPart_StartingHumanlikes others = other as ScenPart_StartingHumanlikes;
            if (others == null || others.kindDef != this.kindDef)
            {
                return false;
            }

            this.pawnCount += others.pawnCount;
            return true;
        }

        public IEnumerable<Pawn> GetPawns()
        {
            bool pawnCheck(Pawn p) => p != null && DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(wtd => wtd.requireCapableColonist).ToList().TrueForAll(w => !p.story.WorkTypeIsDisabled(w));


            for (int i = 0; i < this.pawnCount; i++)
            {
                Pawn newPawn = null;
                for (int x = 0; x < 200; x++)
                {
                    newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(this.kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, -1, true, false, false, false, true, false, 26f, true));
                    if (pawnCheck(newPawn))
                    {
                        x = 200;
                    }
                }
                yield return newPawn;
            }
            yield break;
        }
    }
}