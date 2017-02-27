using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    class ScenPart_StartingHumanlikes : ScenPart
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
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
            if (Widgets.ButtonText(scenPartRect.TopPart(0.45f), this.kindDef.label.CapitalizeFirst(), true, false, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.AddRange(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar => ar.alienRace.pawnKindSettings.startingColonists != null).SelectMany(ar => ar.alienRace.pawnKindSettings.startingColonists.SelectMany(ste => ste.pawnKindEntries.SelectMany(pke => pke.kindDefs))).Select(pkd => new FloatMenuOption(pkd.label.CapitalizeFirst(), () => kindDef = pkd)));
                list.Add(new FloatMenuOption("Villager", () => kindDef = PawnKindDefOf.Villager));
                list.Add(new FloatMenuOption("Slave", () => kindDef = PawnKindDefOf.Slave));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.TextFieldNumeric<int>(scenPartRect.BottomPart(0.45f), ref pawnCount, ref buffer, 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref pawnCount, "alienRaceScenPawnCount", 0);
            Scribe_Defs.LookDef<PawnKindDef>(ref kindDef, "PawnKindDefAlienRaceScen");
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
        }

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "PlayerStartsWith")
                yield return kindDef.LabelCap + " x" + pawnCount;
            yield break;
        }

        public override bool TryMerge(ScenPart other)
        {
            ScenPart_StartingHumanlikes others = other as ScenPart_StartingHumanlikes;
            if (others == null || others.kindDef != this.kindDef)
                return false;
            this.pawnCount += others.pawnCount;
            return true;
        }

        public IEnumerable<Pawn> GetPawns()
        {
            for (int i = 0; i < this.pawnCount; i++)
            {
                Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(this.kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, true, true, true, p => DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(wtd => wtd.requireCapableColonist).ToList().TrueForAll(w => !p.story.WorkTypeIsDisabled(w))));
                yield return newPawn;
            }
            yield break;
        }
    }
}