using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public sealed class BackstoryDef : Def
    {
#pragma warning disable CS0649
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
        public List<BackstoryDefSkillListItem> skillGains = new List<BackstoryDefSkillListItem>();
        public List<string> spawnCategories = new List<string>();
        public List<AlienTraitEntry> forcedTraits = new List<AlienTraitEntry>();
        public List<AlienTraitEntry> disallowedTraits = new List<AlienTraitEntry>();
#pragma warning restore CS0649

        private static bool patched = false;

        public BackstoryDef()
        {
            if (!patched)
            {
                HarmonyInstance.Create("rimworld.erdelf.alien_race.backstory").Patch(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"), new HarmonyMethod(typeof(HarmonyPatches), "GiveShortHashPrefix"), null);
                patched = true;
            }
        }

        public override void ResolveReferences()
        {

            base.ResolveReferences();

            
            if (!addToDatabase || BackstoryDatabase.allBackstories.ContainsKey(defName) || title.NullOrEmpty() || spawnCategories.NullOrEmpty())
            {
                return;
            }

            Backstory b = new Backstory()
            {
                baseDesc = baseDescription.NullOrEmpty() ? "Empty." : baseDescription,
                bodyTypeGlobal = bodyTypeGlobal,
                bodyTypeFemale = bodyTypeFemale,
                bodyTypeMale = bodyTypeMale,
                slot = slot,
                shuffleable = shuffleable,
                spawnCategories = spawnCategories,
                skillGains = skillGains.ToDictionary(i => i.defName, i => i.amount),
                forcedTraits = forcedTraits.NullOrEmpty() ? null : forcedTraits.Where(trait => Rand.Range(0,100) < trait.chance).ToList().ConvertAll(trait => new TraitEntry(TraitDef.Named(trait.defname), trait.degree)),
                disallowedTraits = disallowedTraits.NullOrEmpty() ? null : disallowedTraits.Where(trait => Rand.Range(0,100) < trait.chance).ToList().ConvertAll(trait => new TraitEntry(TraitDef.Named(trait.defname), trait.degree)),
                workDisables = workAllows.NullOrEmpty() ? workDisables.NullOrEmpty() ? WorkTags.None : ((Func<WorkTags>)delegate
                {
                    WorkTags wt = WorkTags.None;
                    workDisables.ForEach(tag => wt |= tag);
                    return wt;
                })() : ((Func<WorkTags>)delegate
                {
                    WorkTags wt = WorkTags.None;
                    Enum.GetValues(typeof(WorkTags)).Cast<WorkTags>().ToList().ForEach(tag => { if (workAllows.Contains(tag)) { wt |= tag; } });
                    return wt;
                })(),
                identifier = defName
            };

            b.SetTitle(title);
            if (!titleShort.NullOrEmpty())
            {
                b.SetTitleShort(titleShort);
            }
            else
            {
                b.SetTitleShort(b.Title);
            }

            b.ResolveReferences();
            b.PostLoad();

            b.identifier = defName;


            if (!b.ConfigErrors(false).Any())
            {
                BackstoryDatabase.AddBackstory(b);
            } else
            {
                Log.Error(defName + " has errors");
            }
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