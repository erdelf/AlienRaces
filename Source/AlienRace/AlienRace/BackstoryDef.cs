﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    using HarmonyLib;

    public class BackstoryDef : Def
    {
        public static HashSet<Backstory> checkBodyType = new HashSet<Backstory>();



        public string baseDescription;
        public BodyTypeDef bodyTypeGlobal;
        public BodyTypeDef bodyTypeMale;
        public BodyTypeDef bodyTypeFemale;
        public string title;
        public string titleFemale;
        public string titleShort;
        public string titleShortFemale;
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
        public List<ThingDefCountRangeClass> forcedItems = new List<ThingDefCountRangeClass>();
        public Backstory backstory;

        public bool CommonalityApproved(Gender g) => Rand.Range(min: 0, max: 100) < (g == Gender.Female ? this.femaleCommonality : this.maleCommonality);

        public bool Approved(Pawn p) => this.CommonalityApproved(g: p.gender) && 
            (this.bioAgeRange == default || (this.bioAgeRange.min < p.ageTracker.AgeBiologicalYears && p.ageTracker.AgeBiologicalYears < this.bioAgeRange.max)) &&
            (this.chronoAgeRange == default || (this.chronoAgeRange.min < p.ageTracker.AgeChronologicalYears && p.ageTracker.AgeChronologicalYears < this.chronoAgeRange.max));

        public override void ResolveReferences()
        {

            base.ResolveReferences();

            
            if (!this.addToDatabase || BackstoryDatabase.allBackstories.ContainsKey(key: this.defName) || this.title.NullOrEmpty() || this.spawnCategories.NullOrEmpty()) return;

            this.backstory = new Backstory
            {
                slot = this.slot,
                shuffleable = this.shuffleable,
                spawnCategories = this.spawnCategories,
                forcedTraits = this.forcedTraits.NullOrEmpty() ? null : this.forcedTraits.Where(predicate: trait => Rand.Range(min: 0, max: 100) < trait.chance).ToList().ConvertAll(converter: trait => new TraitEntry(def: trait.defName, degree: trait.degree)),
                disallowedTraits = this.disallowedTraits.NullOrEmpty() ? null : this.disallowedTraits.Where(predicate: trait => Rand.Range(min: 0, max: 100) < trait.chance).ToList().ConvertAll(converter: trait => new TraitEntry(def: trait.defName, degree: trait.degree)),
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

            if(this.bodyTypeGlobal == null && this.bodyTypeFemale == null && this.bodyTypeMale == null)
            {
                checkBodyType.Add(this.backstory);
                this.bodyTypeGlobal = DefDatabase<BodyTypeDef>.GetRandom();
            }

            Traverse.Create(root: this.backstory).Field(name: "bodyTypeGlobalResolved").SetValue(value: this.bodyTypeGlobal);
            Traverse.Create(root: this.backstory).Field(name: "bodyTypeFemaleResolved").SetValue(value: this.bodyTypeFemale);
            Traverse.Create(root: this.backstory).Field(name: "bodyTypeMaleResolved").SetValue(value: this.bodyTypeMale);

                Traverse.Create(root: this.backstory).Field(name: nameof(this.skillGains)).SetValue(value: this.skillGains.ToDictionary(keySelector: i => i.defName, elementSelector: i => i.amount));

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
            if (bs.backstory == null) return;

            bs.backstory.baseDesc = bs.baseDescription.NullOrEmpty() ? "Empty." : bs.baseDescription;
            bs.backstory.SetTitle(newTitle: bs.title, newTitleFemale: bs.titleFemale);
            bs.backstory.SetTitleShort(newTitleShort: bs.titleShort.NullOrEmpty() ? bs.backstory.title : bs.titleShort,
                newTitleShortFemale: bs.titleShortFemale.NullOrEmpty() ? bs.backstory.titleFemale : bs.titleShortFemale);
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