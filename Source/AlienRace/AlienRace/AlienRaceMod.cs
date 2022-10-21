namespace AlienRace
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class AlienRaceMod : Mod
    {
        public static AlienRaceSettings settings;

        public override string SettingsCategory() => "Alien Race";

        public AlienRaceMod(ModContentPack content) : base(content)
        {
            settings = this.GetSettings<AlienRaceSettings>();

            if (CachedData.customDataLoadMethodCacheInfo().ContainsKey(typeof(AlienPartGenerator.BodyAddon)))
                CachedData.customDataLoadMethodCacheInfo()[typeof(AlienPartGenerator.BodyAddon)] = null;
            else
                CachedData.customDataLoadMethodCacheInfo().Add(typeof(AlienPartGenerator.BodyAddon), null);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled(label: "Use central melanin for factions", ref settings.centralMelanin, tooltip: "True: Pawns of the same factions will have more or less the same skin color.\nFalse: Skin color is not bound by factions.\nNote: Race authors may decide to override skin colors.");
            listingStandard.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.UpdateSettings();
        }
    }

    public class AlienRaceSettings : ModSettings
    {
        public bool centralMelanin;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.centralMelanin, label: "centralMelanin", defaultValue: false);
        }

        public void UpdateSettings()
        {
            ((ThingDef_AlienRace)ThingDefOf.Human).alienRace.generalSettings.alienPartGenerator.colorChannels.Find(match: ccg => ccg.name == "skin").entries[0].first = 
                new ColorGenerator_SkinColorMelanin { maxMelanin = 1f, minMelanin = 0f, naturalMelanin = this.centralMelanin };
        }
    }
}
