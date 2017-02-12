using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public class BackstoryDef : Def
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
        public List<TraitEntry> forcedTraits = new List<TraitEntry>();
        public List<TraitEntry> disallowedTraits = new List<TraitEntry>();
        public string saveKeyIdentifier;
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


        public string UniqueSaveKey
        {
            get
            {
                return (saveKeyIdentifier.NullOrEmpty() ? "AlienBackstory_" : saveKeyIdentifier + "_") + defName;
            }
        }

        public override void ResolveReferences()
        {

            base.ResolveReferences();

            
            if (!addToDatabase || BackstoryDatabase.allBackstories.ContainsKey(UniqueSaveKey) || title.NullOrEmpty() || spawnCategories.NullOrEmpty())
                return;
            
            //Log.Message(DefDatabase<BackstoryDef>.DefCount.ToString());


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
                forcedTraits = forcedTraits.NullOrEmpty() ? null : forcedTraits.ConvertAll(trait => new TraitEntry(trait.def, trait.degree)),
                disallowedTraits = disallowedTraits.NullOrEmpty() ? null : disallowedTraits.ConvertAll(trait => new TraitEntry(trait.def, trait.degree)),
                workDisables = workAllows.NullOrEmpty() ? workDisables.NullOrEmpty() ? WorkTags.None : ((Func<WorkTags>)delegate
                {
                    WorkTags wt = WorkTags.None;
                    workDisables.ForEach(tag => wt |= tag);
                    return wt;
                })() : ((Func<WorkTags>)delegate
                {
                    WorkTags wt = WorkTags.None;
                    Enum.GetValues(typeof(WorkTags)).Cast<WorkTags>().ToList().ForEach(tag => { if (workAllows.Contains(tag)) wt |= tag; });
                    return wt;
                })(),
                identifier = UniqueSaveKey
            };

            b.SetTitle(title);
            if (!titleShort.NullOrEmpty())
                b.SetTitleShort(titleShort);
            else
                b.SetTitleShort(b.Title);

            b.ResolveReferences();
            b.PostLoad();

            if (!b.ConfigErrors(false).Any())
            {
                BackstoryDatabase.allBackstories.Add(UniqueSaveKey, b);
            } else
            {
                Log.Error(UniqueSaveKey + " has errors");
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