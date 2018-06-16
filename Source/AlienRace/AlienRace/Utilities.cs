using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlienRace
{
    using JetBrains.Annotations;

    [DefOf]
    public static class AlienDefOf
    {
#pragma warning disable IDE1006 // Benennungsstile
        // ReSharper disable InconsistentNaming
        public static TraitDef Xenophobia;
        public static ThoughtDef XenophobiaVsAlien;
        public static ThingCategoryDef alienCorpseCategory;
        // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Benennungsstile
    }

    [UsedImplicitly]
    public class ThinkNode_ConditionalIsMemberOfRace : ThinkNode_Conditional
    {
        public List<string> races;

        protected override bool Satisfied(Pawn pawn) => 
            this.races.Contains(item: pawn.def.defName);
    }

    [UsedImplicitly]
    public class Thought_XenophobeVsXenophile : Thought_SituationalSocial
    {
        public override float OpinionOffset() =>
             this.pawn.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) == 1 ? -25f : -15f;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobeVsXenophile : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) => 
            p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && p.story.traits.HasTrait(tDef: AlienDefOf.Xenophobia) && otherPawn.story.traits.HasTrait(tDef: AlienDefOf.Xenophobia) && 
            p.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) != otherPawn.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) && RelationsUtility.PawnsKnowEachOther(p1: p, p2: otherPawn);
    }

    [UsedImplicitly]
    public class Thought_XenophobiaVsAlien : Thought_SituationalSocial
    {
        public override float OpinionOffset() => 
            this.pawn.def != this.OtherPawn().def ? this.pawn.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) == 1 ? -30 : 
            this.OtherPawn().story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) == 1 ? -15 : 0 : 0;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobiaVsAlien : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p1: p, p2: otherPawn) &&
            !(p.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(item: otherPawn.def.defName)) &&
            !(otherPawn.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                p.story.traits.HasTrait(tDef: AlienDefOf.Xenophobia) ?
                    p.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_AlienVsXenophobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p1: p, p2: otherPawn) &&
            !(otherPawn.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(item: p.def.defName)) &&
            !(p.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                otherPawn.story.traits.HasTrait(tDef: AlienDefOf.Xenophobia) ?
                    otherPawn.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }
}
