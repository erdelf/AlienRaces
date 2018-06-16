using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    public class ScenPart_StartingHumanlikes : ScenPart
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
            DefDatabase<ScenPartDef>.Add(def: scenPart);
        }

        private PawnKindDef kindDef = PawnKindDefOf.Villager;
        private int pawnCount;
        private string buffer;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(part: this, height: RowHeight * 3f);
            if (Widgets.ButtonText(rect: scenPartRect.TopPart(pct: 0.45f), label: this.kindDef.label.CapitalizeFirst()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.AddRange(collection: DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(predicate: ar => ar.alienRace.pawnKindSettings.startingColonists != null).SelectMany(selector: ar => ar.alienRace.pawnKindSettings.startingColonists.SelectMany(selector: ste => ste.pawnKindEntries.SelectMany(selector: pke => pke.kindDefs))).Where(predicate: s => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName: s) != null).Select(selector: pkd => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName: pkd)).Select(selector: pkd => new FloatMenuOption(label: pkd.label.CapitalizeFirst(), action: () => this.kindDef = pkd)));
                list.Add(item: new FloatMenuOption(label: "Villager", action: () => this.kindDef = PawnKindDefOf.Villager));
                list.Add(item: new FloatMenuOption(label: "Slave", action: () => this.kindDef = PawnKindDefOf.Slave));
                Find.WindowStack.Add(window: new FloatMenu(options: list));
            }
            Widgets.TextFieldNumeric(rect: scenPartRect.BottomPart(pct: 0.45f), val: ref this.pawnCount, buffer: ref this.buffer);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref this.pawnCount, label: "alienRaceScenPawnCount");
            Scribe_Defs.Look(value: ref this.kindDef, label: "PawnKindDefAlienRaceScen");
        }

        public override string Summary(Scenario scen) => ScenSummaryList.SummaryWithList(scen: scen, tag: "PlayerStartsWith", intro: ScenPart_StartingThing_Defined.PlayerStartWithIntro);

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "PlayerStartsWith") yield return this.kindDef.LabelCap + " x" + this.pawnCount;
        }

        public override bool TryMerge(ScenPart other)
        {
            if (!(other is ScenPart_StartingHumanlikes others) || others.kindDef != this.kindDef)
            {
                return false;
            }

            this.pawnCount += others.pawnCount;
            return true;
        }

        public IEnumerable<Pawn> GetPawns()
        {
            bool PawnCheck(Pawn p) => p != null && DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(predicate: wtd => wtd.requireCapableColonist).ToList().TrueForAll(match: w => !p.story.WorkTypeIsDisabled(w: w));


            for (int i = 0; i < this.pawnCount; i++)
            {
                Pawn newPawn = null;
                for (int x = 0; x < 200; x++)
                {
                    newPawn = PawnGenerator.GeneratePawn(request: new PawnGenerationRequest(kind: this.kindDef, faction: Faction.OfPlayer, context: PawnGenerationContext.PlayerStarter, tile: -1, forceGenerateNewPawn: true, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, colonistRelationChanceFactor: 26f, forceAddFreeWarmLayerIfNeeded: true));
                    if (PawnCheck(p: newPawn))
                    {
                        x = 200;
                    }
                }
                yield return newPawn;
            }
        }
    }
}