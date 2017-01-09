using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public class BackstoryDef : Def
    {

        #region XML Data

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

        #endregion

        public static BackstoryDef Named(string defName)
        {
            return DefDatabase<BackstoryDef>.GetNamed(defName);
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (!this.addToDatabase) return;
            if (BackstoryDatabase.allBackstories.ContainsKey(this.UniqueSaveKey())) return;

            Backstory b = new Backstory();
            if (!this.title.NullOrEmpty())
                b.SetTitle(this.title);
            else
            {
                return;
            }
            if (!titleShort.NullOrEmpty())
                b.SetTitleShort(titleShort);
            else
                b.SetTitleShort(b.Title);

            if (!baseDescription.NullOrEmpty())
                b.baseDesc = baseDescription;
            else
            {
                b.baseDesc = "Empty.";
            }

            b.bodyTypeGlobal = bodyTypeGlobal;
            b.bodyTypeMale = bodyTypeMale;
            b.bodyTypeFemale = bodyTypeFemale;

            b.slot = slot;

            b.shuffleable = shuffleable;
            if (spawnCategories.NullOrEmpty())
            {
                return;
            }
            else
                b.spawnCategories = spawnCategories;

            if (workAllows.Count > 0)
            {
                foreach (WorkTags current in Enum.GetValues(typeof(WorkTags)))
                {
                    if (!workAllows.Contains(current))
                    {
                        b.workDisables |= current;
                    }
                }
            }
            else if (workDisables.Count > 0)
            {
                foreach (var tag in workDisables)
                {
                    b.workDisables |= tag;
                }
            }
            else
            {
                b.workDisables = WorkTags.None;
            }
            b.skillGains = skillGains.ToDictionary(i => i.defName, i => i.amount);

            if (forcedTraits.Count > 0)
            {
                b.forcedTraits = new List<TraitEntry>();
                foreach (var trait in forcedTraits)
                {
                    var newTrait = new TraitEntry(trait.def, trait.degree);
                    b.forcedTraits.Add(newTrait);
                }
            }

            if (disallowedTraits.Count > 0)
            {
                b.disallowedTraits = new List<TraitEntry>();
                foreach (var trait in disallowedTraits)
                {
                    var newTrait = new TraitEntry(trait.def, trait.degree);
                    b.disallowedTraits.Add(newTrait);
                }
            }

            b.ResolveReferences();
            b.PostLoad();
            b.identifier = this.UniqueSaveKey();

            bool flag = false;
            foreach (var s in b.ConfigErrors(false))
            {
                if (!flag)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                BackstoryDatabase.AddBackstory(b);
                //Log.Message("Added " + this.UniqueSaveKey() + " backstory");
                //CCL_Log.Message("Added " + this.UniqueSaveKey() + " backstory", "Backstories");
            }

        }
    }

    public static class BackstoryDefExt
    {
        public static string UniqueSaveKey(this BackstoryDef def)
        {
            if (def.saveKeyIdentifier.NullOrEmpty())
                return "CustomBackstory_" + def.defName;
            else
                return def.saveKeyIdentifier + "_" + def.defName;
        }
    }

    public struct BackstoryDefSkillListItem
    {
        public string defName;

        public int amount;
    }

}
