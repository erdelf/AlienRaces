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
            Listing_Standard listingStandard = new();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled(label: "Display Texture Loading Logs", ref settings.textureLogs, tooltip: "True: Texture loading logs are displayed on startup.\nFalse: Texture loading logs and details are suppressed.\nNote: This is intended for race mod debugging and development.");
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
        public bool textureLogs;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.textureLogs, label: "TextureLogs", defaultValue: false);
        }

        public void UpdateSettings()
        {

        }
    }
}
