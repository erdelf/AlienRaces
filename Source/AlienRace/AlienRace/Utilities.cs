using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlienRace
{
    [DefOf]
    public static class AlienDefOf
    {
        public static TraitDef Xenophobia;
        public static ThoughtDef XenophobiaVsAlien;
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
             this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -25f : -15f;
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
            this.pawn.def != OtherPawn().def ? this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -30 : 
            this.OtherPawn().story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -15 : 0 : 0;
    }

    public class ThoughtWorker_XenophobiaVsAlien : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(p.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(otherPawn.def.defName)) &&
            !(otherPawn.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.ImmuneToXenophobia) ?
                p.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(0) :
                        ThoughtState.ActiveAtStage(1) :
                    false :
                false;
    }

    public class ThoughtWorker_AlienVsXenophobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (!p.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike)
                return false;
            List<ISocialThought> thoughts = new List<ISocialThought>();
            otherPawn.needs.mood.thoughts.GetSocialThoughts(p, thoughts);
            Thought_SituationalSocial thought_SituationalSocial;

            return (thought_SituationalSocial = thoughts.OfType<Thought_SituationalSocial>().FirstOrDefault(tss => tss.def == AlienDefOf.XenophobiaVsAlien)) != null ?
                thought_SituationalSocial.CurStageIndex == 0 ?
                ThoughtState.ActiveAtStage(0) :
                ThoughtState.ActiveAtStage(1) :
                false;
        }
    }
}
