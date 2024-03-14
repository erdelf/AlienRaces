namespace AlienRace
{
    using ExtendedGraphics;
    using UnityEngine;
    using Verse;

    public class AlienRaceMod : Mod
    {
        public static AlienRaceSettings settings;

        public override string SettingsCategory() => "Alien Race";

        public AlienRaceMod(ModContentPack content) : base(content)
        {
            settings = this.GetSettings<AlienRaceSettings>();
            /*
            if (CachedData.customDataLoadMethodCacheInfo().ContainsKey(typeof(AlienPartGenerator.BodyAddon)))
                CachedData.customDataLoadMethodCacheInfo()[typeof(AlienPartGenerator.BodyAddon)] = null;
            else
                CachedData.customDataLoadMethodCacheInfo().Add(typeof(AlienPartGenerator.BodyAddon), null);
            */

            XmlInheritance.allowDuplicateNodesFieldNames.Add(nameof(AbstractExtendedGraphic.extendedGraphics));
            XmlInheritance.allowDuplicateNodesFieldNames.Add(nameof(AlienPartGenerator.ExtendedConditionGraphic.conditions));
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled(label: "Display Texture Loading Logs", ref settings.textureLogs,
                                            tooltip: "True: Texture loading logs are displayed on startup.\nFalse: Texture loading logs and details are suppressed.\nNote: This is intended for race mod debugging and development.");
            listingStandard.CheckboxLabeled(label: "Randomize Starting Pawns on Reroll", ref settings.randomizeStartingPawnsOnReroll,
                                            tooltip: "True: Randomizing a starting pawn will allow for a complete reroll.\nFalse: All Settings stay on a reroll and only details will change.");
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
        public bool randomizeStartingPawnsOnReroll = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.textureLogs,                    label: nameof(this.textureLogs),                    defaultValue: false);
            Scribe_Values.Look(ref this.randomizeStartingPawnsOnReroll, label: nameof(this.randomizeStartingPawnsOnReroll), defaultValue: true);
        }

        public void UpdateSettings()
        {

        }
    }
}
