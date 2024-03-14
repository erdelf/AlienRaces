namespace AlienRace
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using Verse;

    public class AlienBackstoryDef : BackstoryDef
    {
        public static HashSet<BackstoryDef> checkBodyType = new();

        public List<AlienChanceEntry<TraitWithDegree>> forcedTraitsChance     = [];
        public List<AlienChanceEntry<TraitWithDegree>> disallowedTraitsChance = [];
        public WorkTags                         workAllows             = WorkTags.AllWork;
        public float                            maleCommonality        = 100f;
        public float                            femaleCommonality      = 100f;
        public BackstoryDef                     linkedBackstory;
        public RelationSettings                 relationSettings = new();
        public List<HediffDef>                  forcedHediffs    = new();
        public List<SkillGain>                  passions         = new();
        public IntRange                         bioAgeRange;
        public IntRange                         chronoAgeRange;
        public List<ThingDefCountRangeClass>    forcedItems = new();

        public bool CommonalityApproved(Gender g) => Rand.Range(0, 100) < (g == Gender.Female ? this.femaleCommonality : this.maleCommonality);

        public bool Approved(Pawn p) => this.CommonalityApproved(p.gender)                                                                                                                              &&
                                        (this.bioAgeRange    == default || (this.bioAgeRange.min    < p.ageTracker.AgeBiologicalYears    && p.ageTracker.AgeBiologicalYears    < this.bioAgeRange.max)) &&
                                        (this.chronoAgeRange == default || (this.chronoAgeRange.min < p.ageTracker.AgeChronologicalYears && p.ageTracker.AgeChronologicalYears < this.chronoAgeRange.max));

        public override void ResolveReferences()
        {
            this.identifier = this.defName;
            base.ResolveReferences();

            this.workDisables = (this.workAllows & WorkTags.AllWork) != 0 ? this.workDisables : ~this.workAllows;

            if (this.bodyTypeGlobal == null && this.bodyTypeFemale == null && this.bodyTypeMale == null)
            {
                checkBodyType.Add(this);
                this.bodyTypeGlobal = DefDatabase<BodyTypeDef>.GetRandom();
            }
        }
    }
}