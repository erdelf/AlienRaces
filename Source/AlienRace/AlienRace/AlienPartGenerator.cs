﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    using System.Text;

    public partial class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string> { "Average_Normal" };

        public List<BodyTypeDef> alienbodytypes = new List<BodyTypeDef>();

        public bool useGenderedHeads = true;
        public bool useGenderedBodies = false;

        public List<ColorChannelGenerator> colorChannels = new List<ColorChannelGenerator>();

        public Vector2 headOffset = Vector2.zero;
        public DirectionOffset headOffsetDirectional = new DirectionOffset();

        public Vector2 customDrawSize = Vector2.one;
        public Vector2 customPortraitDrawSize = Vector2.one;
        public Vector2 customHeadDrawSize = Vector2.zero;
        public Vector2 customPortraitHeadDrawSize = Vector2.zero;

        public BodyPartDef headBodyPartDef;

        private static readonly Dictionary<Vector2, AlienGraphicMeshSet> meshPools = new Dictionary<Vector2, AlienGraphicMeshSet>();

        public List<BodyAddon> bodyAddons = new List<BodyAddon>();

        public ThingDef_AlienRace alienProps;

        public string RandomAlienHead(string userpath, Pawn pawn) => GetAlienHead(userpath: userpath, gender: (this.useGenderedHeads ? pawn.gender.ToString() : ""), crowntype: pawn.GetComp<AlienComp>().crownType = this.aliencrowntypes[index: Rand.Range(min: 0, max: this.aliencrowntypes.Count)]);

        public static string GetAlienHead(string userpath, string gender, string crowntype) => userpath.NullOrEmpty() ? "" : userpath + (userpath == GraphicPaths.VANILLA_HEAD_PATH ? gender + "/" : "") + (!gender.NullOrEmpty() ? gender + "_" : "") + crowntype;

        public Graphic GetNakedGraphic(BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender) => GraphicDatabase.Get<Graphic_Multi>(path: GetNakedPath(bodyType: bodyType, userpath: userpath, gender: this.useGenderedBodies ? gender : ""), shader: shader, drawSize: Vector2.one, color: skinColor, colorTwo: skinColorSecond);

        public static string GetNakedPath(BodyTypeDef bodyType, string userpath, string gender) => userpath + (!gender.NullOrEmpty() ? gender + "_" : "") + "Naked_" + bodyType;

        public Color SkinColor(Pawn alien, bool first = true)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            ExposableValueTuple<Color, Color> skinColors = alienComp.GetChannel("skin");
            return first ? skinColors.first : skinColors.second;
        }

        public void GenerateMeshsAndMeshPools()
        {
            void AddMeshSet(Vector2 drawSize, Vector2 headDrawSize)
            {
                if (!meshPools.Keys.Any(predicate: v => v.Equals(other: drawSize)))
                {
                    meshPools.Add(key: drawSize, value: new AlienGraphicMeshSet()
                    {
                        bodySet = new GraphicMeshSet(width: 1.5f * drawSize.x, height: 1.5f * drawSize.y), // bodySet
                        headSet = new GraphicMeshSet(width: 1.5f * headDrawSize.x, height: 1.5f * headDrawSize.y), // headSet
                        hairSetAverage = new GraphicMeshSet(width: 1.5f * headDrawSize.x, height: 1.5f * headDrawSize.y), // hairSetAverage
                    });
                }
            }

            foreach (GraphicPaths graphicsPath in this.alienProps.alienRace.graphicPaths.Concat(
                    rhs: new GraphicPaths() { customDrawSize = this.customDrawSize, customHeadDrawSize = this.customHeadDrawSize, customPortraitDrawSize = this.customPortraitDrawSize, customPortraitHeadDrawSize = this.customPortraitHeadDrawSize }))
            {
                AddMeshSet(drawSize: graphicsPath.customDrawSize, headDrawSize: graphicsPath.customDrawSize);
                AddMeshSet(drawSize: graphicsPath.customHeadDrawSize, headDrawSize: graphicsPath.customHeadDrawSize);
                AddMeshSet(drawSize: graphicsPath.customPortraitDrawSize, headDrawSize: graphicsPath.customPortraitDrawSize);
                AddMeshSet(drawSize: graphicsPath.customPortraitHeadDrawSize, headDrawSize: graphicsPath.customPortraitHeadDrawSize);
            }


            StringBuilder logBuilder = new StringBuilder();
            this.bodyAddons.Do(action: ba =>
            {

                void AddToStringBuilder(string s)
                {
                    if (ba.debug)
                        logBuilder.AppendLine(s);
                }

                if (ba.variantCount != 0) return;

                AddToStringBuilder($"Loading variants for {ba.path}");

                while (ContentFinder<Texture2D>.Get(itemPath: $"{ba.path}{(ba.variantCount == 0 ? "" : ba.variantCount.ToString())}_north", reportFailure: false) != null)
                    ba.variantCount++;

                AddToStringBuilder($"Variants found for {ba.path}: {ba.variantCount}");

                if (ba.hediffGraphics != null)
                {
                    foreach (BodyAddonHediffGraphic bahg in ba.hediffGraphics.Where(bahg => bahg.variantCount == 0))
                    {
                        while (ContentFinder<Texture2D>.Get(itemPath: bahg.path + (bahg.variantCount == 0 ? "" : bahg.variantCount.ToString()) + "_north", reportFailure: false) != null)
                            bahg.variantCount++;
                        AddToStringBuilder($"Variants found for {bahg.path}: {bahg.variantCount}");
                        if (bahg.variantCount == 0)
                            Log.Warning($"No hediff graphics found at {bahg.path} for hediff {bahg.hediff} in {this.alienProps.defName}");

                        if (bahg.severity != null)
                        {
                            foreach (BodyAddonHediffSeverityGraphic bahsg in bahg.severity)
                            {
                                while (ContentFinder<Texture2D>.Get(itemPath: bahsg.path + (bahsg.variantCount == 0 ? "" : bahsg.variantCount.ToString()) + "_north", reportFailure: false) != null)
                                    bahsg.variantCount++;
                                AddToStringBuilder($"Variants found for {bahsg.path} at severity {bahsg.severity}: {bahsg.variantCount}");
                                if (bahsg.variantCount == 0)
                                    Log.Warning($"No hediff graphics found at {bahsg.path} at severity {bahsg.severity} for hediff {bahg.hediff} in {this.alienProps.defName}");
                            }
                        }
                    }
                }

                if (ba.backstoryGraphics != null)
                {
                    foreach (BodyAddonBackstoryGraphic babg in ba.backstoryGraphics.Where(babg => babg.variantCount == 0))
                    {
                        while (ContentFinder<Texture2D>.Get(itemPath: babg.path + (babg.variantCount == 0 ? "" : babg.variantCount.ToString()) + "_north", reportFailure: false) != null)
                            babg.variantCount++;
                        AddToStringBuilder($"Variants found for {babg.path}: {babg.variantCount}");
                        if (babg.variantCount == 0)
                            Log.Warning($"no backstory graphics found at {babg.path} for backstory {babg.backstory} in {this.alienProps.defName}");
                    }
                }


            });
            if (logBuilder.Length > 0)
                 Log.Message($"Loaded body addon variants for {this.alienProps.defName}\n{logBuilder}"); 
        }

        public class ColorChannelGenerator
        {
            public string         name = "";
            public ColorGenerator first;
            public ColorGenerator second;
        }


        public class AlienComp : ThingComp
        {
            public bool                fixGenderPostSpawn;
            public string              crownType;
            public Vector2             customDrawSize             = Vector2.one;
            public Vector2             customHeadDrawSize         = Vector2.one;
            public Vector2             customPortraitDrawSize     = Vector2.one;
            public Vector2             customPortraitHeadDrawSize = Vector2.one;
            public AlienGraphicMeshSet alienGraphics;
            public AlienGraphicMeshSet alienHeadGraphics;
            public AlienGraphicMeshSet alienPortraitGraphics;
            public AlienGraphicMeshSet alienPortraitHeadGraphics;
            public List<Graphic>       addonGraphics;
            public List<int>           addonVariants;

            private Dictionary<string, ExposableValueTuple<Color, Color>> colorChannels;

            public Dictionary<string, ExposableValueTuple<Color, Color>> ColorChannels
            {
                get
                {
                    if (this.colorChannels == null || !this.colorChannels.Any())
                    {
                        this.colorChannels = new Dictionary<string, ExposableValueTuple<Color, Color>>();
                        Pawn               pawn       = (Pawn) this.parent;
                        ThingDef_AlienRace alienProps = ((ThingDef_AlienRace) this.parent.def);
                        AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                        this.colorChannels.Add("base", new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                        this.colorChannels.Add("hair", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        Color skinColor = PawnSkinColors.GetSkinColor(pawn.story.melanin);
                        this.colorChannels.Add("skin", new ExposableValueTuple<Color, Color>(skinColor, skinColor));

                        foreach (ColorChannelGenerator channel in apg.colorChannels)
                        {
                            if (!this.colorChannels.ContainsKey(channel.name))
                                this.colorChannels.Add(channel.name, new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                            ExposableValueTuple<Color, Color> colors = this.colorChannels[channel.name];
                            if (channel.first != null)
                                colors.first = this.GenerateColor(channel.first);
                            if (channel.second != null)
                                colors.second = this.GenerateColor(channel.second);
                        }

                        ExposableValueTuple<Color, Color> hairColors = this.colorChannels["hair"];
                        if (hairColors.first == Color.clear)
                        {
                            Color color = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                            hairColors.first  = color;
                            hairColors.second = color;
                        }

                        pawn.story.hairColor = hairColors.first;

                        if (alienProps.alienRace.hairSettings.getsGreyAt <= pawn.ageTracker.AgeBiologicalYears)
                        {
                            if (Rand.Value < GenMath.SmootherStep(alienProps.alienRace.hairSettings.getsGreyAt,
                                                                  pawn.RaceProps.ageGenerationCurve.Points.Count < 3
                                                                      ? alienProps.alienRace.hairSettings.getsGreyAt + 35
                                                                      : pawn.RaceProps.ageGenerationCurve.Points.Skip(pawn.RaceProps.ageGenerationCurve.Points.Count - 3).First().x,
                                                                  pawn.ageTracker.AgeBiologicalYears))
                            {
                                float grey = Rand.Range(min: 0.65f, max: 0.85f);
                                pawn.story.hairColor = new Color(r: grey, g: grey, b: grey);
                                hairColors.first     = pawn.story.hairColor;
                            }
                        }
                    }

                    return this.colorChannels;
                }
            }

            public Color GenerateColor(ColorGenerator gen)
            {
                switch (gen)
                {
                    case ColorGenerator_CustomAlienChannel ac:
                        string[] split = ac.colorChannel.Split('_');
                        return split[1] == "1" ? this.ColorChannels[split[0]].first : this.ColorChannels[split[0]].second;
                    default:
                        return gen.NewRandomizedColor();
                }
            }

            public override void PostSpawnSetup(bool respawningAfterLoad)
            {
                base.PostSpawnSetup(respawningAfterLoad: respawningAfterLoad);
                AlienPartGenerator apg = ((ThingDef_AlienRace) this.parent.def).alienRace.generalSettings.alienPartGenerator;
                this.customDrawSize             = apg.customDrawSize;
                this.customHeadDrawSize         = apg.customHeadDrawSize;
                this.customPortraitDrawSize     = apg.customPortraitDrawSize;
                this.customPortraitHeadDrawSize = apg.customPortraitHeadDrawSize;
            }

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.Look(value: ref this.fixGenderPostSpawn, label: "fixAlienGenderPostSpawn");
                Scribe_Values.Look(value: ref this.crownType,          label: "crownType");
                Scribe_Collections.Look(list: ref this.addonVariants, label: "addonVariants");
                Scribe_Collections.Look(dict: ref this.colorChannels, label: "colorChannels");
            }

            public ExposableValueTuple<Color, Color> GetChannel(string channel) =>
                this.ColorChannels[channel];

            internal void AssignProperMeshs()
            {
                this.alienGraphics             = meshPools[key: this.customDrawSize];
                this.alienHeadGraphics         = meshPools[key: this.customHeadDrawSize];
                this.alienPortraitGraphics     = meshPools[key: this.customPortraitDrawSize];
                this.alienPortraitHeadGraphics = meshPools[key: this.customPortraitHeadDrawSize];
            }

            [DebugAction("AlienRace", "Regenerate all colorchannels", allowedGameStates = AllowedGameStates.PlayingOnMap)]
            private static void RegenerateColorchannels()
            {
                foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawns)
                {
                    AlienComp comp = pawn.TryGetComp<AlienComp>();
                    if (comp != null)
                        comp.colorChannels = null;
                }
            }
        }

        public class ExposableValueTuple<K, V> : IExposable
        {
            public K first;
            public V second;

            public ExposableValueTuple()
            {
            }

            public ExposableValueTuple(K first, V second)
            {
                this.first = first;
                this.second = second;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref this.first, "first");
                Scribe_Values.Look(ref this.second, "second");
            }
        }

        public class AlienGraphicMeshSet
        {
            public GraphicMeshSet bodySet;
            public GraphicMeshSet headSet;
            public GraphicMeshSet hairSetAverage;
        }
    }
}