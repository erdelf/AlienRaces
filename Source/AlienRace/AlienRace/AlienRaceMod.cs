namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using ExtendedGraphics;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class AlienRaceMod : Mod
    {
        public static AlienRaceMod      instance;
        public static AlienRaceSettings settings;

        public override string SettingsCategory() => "Alien Race";

        public AlienRaceMod(ModContentPack content) : base(content)
        {
            instance = this;
            settings = this.GetSettings<AlienRaceSettings>();
            /*
            if (CachedData.customDataLoadMethodCacheInfo().ContainsKey(typeof(AlienPartGenerator.BodyAddon)))
                CachedData.customDataLoadMethodCacheInfo()[typeof(AlienPartGenerator.BodyAddon)] = null;
            else
                CachedData.customDataLoadMethodCacheInfo().Add(typeof(AlienPartGenerator.BodyAddon), null);
            */

            XmlInheritance.allowDuplicateNodesFieldNames.Add(nameof(AbstractExtendedGraphic.extendedGraphics));
            XmlInheritance.allowDuplicateNodesFieldNames.Add(nameof(AlienPartGenerator.ExtendedConditionGraphic.conditions));

            foreach (Type type in typeof(ConditionLogicCollection).AllSubclassesNonAbstract())
                XmlInheritance.allowDuplicateNodesFieldNames.Add(Traverse.Create(type).Field(nameof(ConditionLogicCollection.XmlNameParseKey)).GetValue<string>());

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml($"<extendedGraphics><li Class=\"{typeof(AlienPartGenerator.ExtendedGraphicTop).FullName}\"><path>here</path></li></extendedGraphics>");
            DirectXmlToObject.ObjectFromXml<List<AbstractExtendedGraphic>>(xmlDoc.DocumentElement, false);
            Func<XmlNode, object> originalFunc = CachedData.listFromXmlMethods()[typeof(List<AbstractExtendedGraphic>)];
            CachedData.listFromXmlMethods()[typeof(List<AbstractExtendedGraphic>)] = node => originalFunc(AbstractExtendedGraphic.CustomListLoader(node));

            bool xmlExtensionsActive  = ModLister.GetActiveModWithIdentifier("imranfish.xmlextensions", true) != null;
            AccessTools.FieldRef<Dictionary<Type, Func<XmlNode, XmlNode, string, object>>> listFromXmlMethodsXE = null;

            if (xmlExtensionsActive)
            {
                listFromXmlMethodsXE =
                    AccessTools.StaticFieldRefAccess<Dictionary<Type, Func<XmlNode, XmlNode, string, object>>>(AccessTools.Field("XmlExtensions.CustomXmlLoader:listFromXmlMethods"));
                
                AccessTools.Method("XmlExtensions.CustomXmlLoader:ObjectFromXml", generics: [typeof(List<AbstractExtendedGraphic>)]).Invoke(null, [xmlDoc.DocumentElement, false, null, null]);

                Func<XmlNode, XmlNode, string, object> originalFuncXE = listFromXmlMethodsXE()[typeof(List<AbstractExtendedGraphic>)];
                listFromXmlMethodsXE()[typeof(List<AbstractExtendedGraphic>)] = (node, _, _) => originalFuncXE(AbstractExtendedGraphic.CustomListLoader(node), null, null);
            }

            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<conditions></conditions>");
            DirectXmlToObject.ObjectFromXml<List<Condition>>(xmlDoc.DocumentElement, false);
            Func<XmlNode, object> originalFunc2 = CachedData.listFromXmlMethods()[typeof(List<Condition>)];
            CachedData.listFromXmlMethods()[typeof(List<Condition>)] = node => originalFunc2(Condition.CustomListLoader(node));

            if (xmlExtensionsActive)
            {
                AccessTools.Method("XmlExtensions.CustomXmlLoader:ObjectFromXml", generics: [typeof(List<Condition>)]).Invoke(null, [xmlDoc.DocumentElement, false, null, null]);

                Func<XmlNode, XmlNode, string, object> originalFuncXE = listFromXmlMethodsXE()[typeof(List<Condition>)];
                listFromXmlMethodsXE()[typeof(List<Condition>)] = (node, _, _) => originalFuncXE(Condition.CustomListLoader(node), null, null);
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("HAR.Options.TextureLoadingLogs_Label".Translate(), ref settings.textureLogs, "HAR.Options.TextureLoadingLogs_Tooltip".Translate());
            listingStandard.CheckboxLabeled("HAR.Options.RandomizeStartingPawns_Label".Translate(), ref settings.randomizeStartingPawnsOnReroll, "HAR.Options.RandomizeStartingPawns_Tooltip".Translate());
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
            Scribe_Values.Look(ref this.textureLogs,                    nameof(this.textureLogs),                    false);
            Scribe_Values.Look(ref this.randomizeStartingPawnsOnReroll, nameof(this.randomizeStartingPawnsOnReroll), true);
        }

        public void UpdateSettings()
        {

        }
    }
}
