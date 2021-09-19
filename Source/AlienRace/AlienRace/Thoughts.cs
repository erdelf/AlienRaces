using System.Collections.Generic;

namespace AlienRace
{
    using JetBrains.Annotations;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.AI.Group;

    [UsedImplicitly]
    public class Thought_XenophobeVsXenophile : Thought_SituationalSocial
    {
        public override float OpinionOffset() =>
             this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -25f : -15f;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobeVsXenophile : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && p.story.traits.HasTrait(AlienDefOf.Xenophobia) && otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) &&
            p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) != otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) && RelationsUtility.PawnsKnowEachOther(p, otherPawn);
    }

    [UsedImplicitly]
    public class Thought_XenophobiaVsAlien : Thought_SituationalSocial
    {
        public override float OpinionOffset() =>
            this.pawn.def != this.OtherPawn().def ? this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -30 :
            this.OtherPawn().story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -15 : 0 : 0;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobiaVsAlien : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(p.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(otherPawn.def)) &&
            !(otherPawn.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                p.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_AlienVsXenophobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(otherPawn.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(p.def)) &&
            !(p.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_Precept_AlienRaces : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => c.def != p.def))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => c.def != p.def))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def))
                        return true;
                }
                else if (map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c)))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [UsedImplicitly]
    public class ThoughtWorker_Precept_AlienRaces_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def;
    }

    public class ThoughtWorker_Precept_SlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => c.def != p.def && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => c.def != p.def && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def && c.IsSlave))
                        return true;
                }
                else if (map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_Slavery_NoSlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (!lord.ownedPawns.Any(c => c.def != p.def && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (!car.PawnsListForReading.Any(c => c.def != p.def && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (!map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def && c.IsSlave))
                        return true;
                }
                else if (!map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_NoRecentAlienMeat : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            int num = Mathf.Max(0, p.GetComp<AlienPartGenerator.AlienComp>()?.lastAlienMeatIngestedTick ?? GenTicks.TicksGame);
            return Find.TickManager.TicksGame - num > 8 * GenDate.TicksPerDay;
        }

        public IEnumerable<NamedArgument> GetDescriptionArgs()
        {
            yield return 8.Named("HUMANMEATREQUIREDINTERVAL");
        }
    }
}
