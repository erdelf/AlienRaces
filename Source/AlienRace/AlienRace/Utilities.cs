using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlienRace
{
    [DefOf]
    public static class AlienDefOf
    {
        public static TraitDef Xenophobia;
    }

    public class ThinkNode_ConditionalIsMemberOfRace : ThinkNode_Conditional
    {
        public List<string> races;

        protected override bool Satisfied(Pawn pawn) => 
            this.races.Contains(pawn.def.defName);
    }

    public class Thought_XenophobeVsXenophile : Thought_SituationalSocial
    {
        public override float OpinionOffset() =>
             -30f;
    }

    public class ThoughtWorker_XenophobeVsXenophile : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) => 
            p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && p.story.traits.HasTrait(AlienDefOf.Xenophobia) && otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) && 
            p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) != otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) && RelationsUtility.PawnsKnowEachOther(p, otherPawn);
    }

    public class Thought_XenophobiaVsAlien : Thought_SituationalSocial
    {
        public override float OpinionOffset() => 
            this.pawn.def != OtherPawn().def ? this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 || 
            this.OtherPawn().story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -30 : 30 : 0;
    }

    public class ThoughtWorker_XenophobiaVsAlien : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) => 
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && (p.story.traits.HasTrait(AlienDefOf.Xenophobia) || otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia)) && RelationsUtility.PawnsKnowEachOther(p, otherPawn) ? 
                p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 || otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? 
                    ThoughtState.ActiveAtStage(1) : 
                    ThoughtState.ActiveAtStage(0) : 
                false;
    }
}
