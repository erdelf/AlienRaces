namespace AlienRace
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class AlienRaceMod : Mod
    {
        public static AlienRaceSettings settings;

        public override string SettingsCategory() => "Alien Race";

        public AlienRaceMod(ModContentPack content) : base(content) => 
            settings = this.GetSettings<AlienRaceSettings>();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled(label: "Use central melanin for factions", ref settings.centralMelanin, tooltip: "True: Pawns of the same factions will have more or less the same skin color.\nFalse: Skin color is not bound by factions.\nNote: Race authors may decide to override skin colors.");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled(label: "Enable detailed addon logging", ref settings.addonLogging, tooltip: "True: Logging will have a detailed output of which path is not being found and what type of addon it is.\nFalse: Logging will only provide a notification of a missing path instead of detailing which path is not being found.\nNote: Race authors may have intentionally left a branch path blank to designate a different texture in the selection tree should be used.");
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
        public bool addonLogging;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.centralMelanin, label: "centralMelanin", defaultValue: false);
            Scribe_Values.Look(ref this.addonLogging, label: "addonLogging", defaultValue: false);
        }

        public void UpdateSettings()
        {
            ((ThingDef_AlienRace)ThingDefOf.Human).alienRace.generalSettings.alienPartGenerator.colorChannels.Find(match: ccg => ccg.name == "skin").first =
            new ColorGenerator_SkinColorMelanin { maxMelanin = 1f, minMelanin = 0f, naturalMelanin = this.centralMelanin };
            BodyAddonSupport.DefaultGraphicsLoader.logAddons = this.addonLogging;
        }
    }
}
