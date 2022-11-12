namespace AlienRace
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using Verse;

    public class AlienBackstoryDef : BackstoryDef
    {
        public static HashSet<BackstoryDef> checkBodyType = new HashSet<BackstoryDef>();

        public List<AlienTraitEntry>         forcedTraitsChance     = new List<AlienTraitEntry>();
        public List<AlienTraitEntry>         disallowedTraitsChance = new List<AlienTraitEntry>();
        public WorkTags                      workAllows             = WorkTags.AllWork;
        public float                         maleCommonality        = 100f;
        public float                         femaleCommonality      = 100f;
        public BackstoryDef                  linkedBackstory;
        public RelationSettings              relationSettings = new RelationSettings();
        public List<HediffDef>               forcedHediffs    = new List<HediffDef>();
        public List<SkillGain>               passions         = new List<SkillGain>();
        public IntRange                      bioAgeRange;
        public IntRange                      chronoAgeRange;
        public List<ThingDefCountRangeClass> forcedItems = new List<ThingDefCountRangeClass>();

        public bool CommonalityApproved(Gender g) => Rand.Range(min: 0, max: 100) < (g == Gender.Female ? this.femaleCommonality : this.maleCommonality);

        public bool Approved(Pawn p) => this.CommonalityApproved(p.gender)                                                                                                                              &&
                                        (this.bioAgeRange    == default || (this.bioAgeRange.min    < p.ageTracker.AgeBiologicalYears    && p.ageTracker.AgeBiologicalYears    < this.bioAgeRange.max)) &&
                                        (this.chronoAgeRange == default || (this.chronoAgeRange.min < p.ageTracker.AgeChronologicalYears && p.ageTracker.AgeChronologicalYears < this.chronoAgeRange.max));

        public override void ResolveReferences()
        {
            this.identifier = this.defName;
            base.ResolveReferences();

            this.forcedTraits = (this.forcedTraits ??= new List<BackstoryTrait>()).
                                Concat(this.forcedTraitsChance.Where(predicate: trait => Rand.Range(min: 0, max: 100) < trait.chance).ToList().ConvertAll(converter: trait => new BackstoryTrait { def = trait.defName, degree = trait.degree })).ToList();
            this.disallowedTraits = (this.disallowedTraits ??= new List<BackstoryTrait>()).
                                    Concat(this.disallowedTraitsChance.Where(predicate: trait => Rand.Range(min: 0, max: 100) < trait.chance).ToList().ConvertAll(converter: trait => new BackstoryTrait { def = trait.defName, degree = trait.degree })).ToList();
            this.workDisables = (this.workAllows & WorkTags.AllWork) != 0 ? this.workDisables : ~this.workAllows;

            if (this.bodyTypeGlobal == null && this.bodyTypeFemale == null && this.bodyTypeMale == null)
            {
                checkBodyType.Add(this);
                this.bodyTypeGlobal = DefDatabase<BodyTypeDef>.GetRandom();
            }
        }
    }
}