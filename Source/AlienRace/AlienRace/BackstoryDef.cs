using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public class BackstoryDef : Def
    {
        public string baseDescription;
        public BodyType bodyTypeGlobal = BodyType.Undefined;
        public BodyType bodyTypeMale = BodyType.Male;
        public BodyType bodyTypeFemale = BodyType.Female;
        public string title;
        public string titleShort;
        public BackstorySlot slot = BackstorySlot.Adulthood;
        public bool shuffleable = true;
        public bool addToDatabase = true;
        public List<WorkTags> workAllows = new List<WorkTags>();
        public List<WorkTags> workDisables = new List<WorkTags>();
        public List<WorkTags> requiredWorkTags = new List<WorkTags>();
        public List<BackstoryDefSkillListItem> skillGains = new List<BackstoryDefSkillListItem>();
        public List<string> spawnCategories = new List<string>();
        public List<AlienTraitEntry> forcedTraits = new List<AlienTraitEntry>();
        public List<AlienTraitEntry> disallowedTraits = new List<AlienTraitEntry>();
        public float maleCommonality = 100f;
        public float femaleCommonality = 100f;
        public string linkedBackstory;
        public RelationSettings relationSettings = new RelationSettings();
        public List<string> forcedHediffs = new List<string>();
        public IntRange bioAgeRange;
        public IntRange chronoAgeRange;
        public List<string> forcedItems = new List<string>();
        public Backstory backstory;

        public bool CommonalityApproved(Gender g) => Rand.Range(min: 0, max: 100) < (g == Gender.Female ? this.femaleCommonality : this.maleCommonality);

        public bool Approved(Pawn p) => this.CommonalityApproved(g: p.gender) && 
            (this.bioAgeRange == default(IntRange) || (this.bioAgeRange.min < p.ageTracker.AgeBiologicalYears && p.ageTracker.AgeBiologicalYears < this.bioAgeRange.max)) &&
            (this.chronoAgeRange == default(IntRange) || (this.chronoAgeRange.min < p.ageTracker.AgeBiologicalYears && p.ageTracker.AgeBiologicalYears < this.chronoAgeRange.max));

        public override void ResolveReferences()
        {

            base.ResolveReferences();

            
            if (!this.addToDatabase || BackstoryDatabase.allBackstories.ContainsKey(key: this.defName) || this.title.NullOrEmpty() || this.spawnCategories.NullOrEmpty()) return;

            this.backstory = new Backstory
            {
                bodyTypeGlobal = this.bodyTypeGlobal,
                bodyTypeFemale = this.bodyTypeFemale,
                bodyTypeMale = this.bodyTypeMale,
                slot = this.slot,
                shuffleable = this.shuffleable,
                spawnCategories = this.spawnCategories,
                skillGains = this.skillGains.ToDictionary(keySelector: i => i.defName, elementSelector: i => i.amount),
                forcedTraits = this.forcedTraits.NullOrEmpty() ? null : this.forcedTraits.Where(predicate: trait => Rand.Range(min: 0, max: 100) < trait.chance).ToList().ConvertAll(converter: trait => new TraitEntry(def: TraitDef.Named(defName: trait.defName), degree: trait.degree)),
                disallowedTraits = this.disallowedTraits.NullOrEmpty() ? null : this.disallowedTraits.Where(predicate: trait => Rand.Range(min: 0, max: 100) < trait.chance).ToList().ConvertAll(converter: trait => new TraitEntry(def: TraitDef.Named(defName: trait.defName), degree: trait.degree)),
                workDisables = this.workAllows.NullOrEmpty() ? this.workDisables.NullOrEmpty() ? WorkTags.None : ((Func<WorkTags>) delegate
                 {
                     WorkTags wt = WorkTags.None;
                     this.workDisables.ForEach(action: tag => wt |= tag);
                     return wt;
                 })() : ((Func<WorkTags>) delegate
                 {
                     WorkTags wt = WorkTags.None;
                     Enum.GetValues(enumType: typeof(WorkTags)).Cast<WorkTags>().Where(predicate: tag => !this.workAllows.Contains(item: tag)).ToList().ForEach(action: tag => wt |= tag);
                     return wt;
                 })(),
                identifier = this.defName,
                requiredWorkTags = ((Func<WorkTags>) delegate
                {
                    WorkTags wt = WorkTags.None;
                    this.requiredWorkTags.ForEach(action: tag => wt |= tag);
                    return wt;
                })()
            };

            UpdateTranslateableFields(bs: this);
            
            this.backstory.ResolveReferences();
            this.backstory.PostLoad();

            this.backstory.identifier = this.defName;

            IEnumerable<string> errors;
            if (!(errors = this.backstory.ConfigErrors(ignoreNoSpawnCategories: false)).Any())
                BackstoryDatabase.AddBackstory(bs: this.backstory);
            else
                Log.Error(text: this.defName + " has errors:\n" + string.Join(separator: "\n", value: errors.ToArray()));
        }

        internal static void UpdateTranslateableFields(BackstoryDef bs)
        {
            bs.backstory.baseDesc = bs.baseDescription.NullOrEmpty() ? "Empty." : bs.baseDescription;
            bs.backstory.SetTitle(newTitle: bs.title);
            bs.backstory.SetTitleShort(newTitleShort: bs.titleShort.NullOrEmpty() ? bs.backstory.Title : bs.titleShort);
        }



        public struct BackstoryDefSkillListItem
        {
#pragma warning disable CS0649
            public string defName;
            public int amount;
#pragma warning restore CS0649
        }
    }
}