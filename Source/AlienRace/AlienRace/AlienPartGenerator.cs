namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public partial class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string> { "Average_Normal" };

        public List<BodyTypeDef> alienbodytypes = new List<BodyTypeDef>();

        public bool useGenderedHeads = true;
        public bool useGenderedBodies = false;

        public List<ColorChannelGenerator> colorChannels  = new List<ColorChannelGenerator>();
        public List<OffsetNamed>      offsetDefaults = new List<OffsetNamed>();

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

        public string RandomAlienHead(string userpath, Pawn pawn) => GetAlienHead(userpath, (this.useGenderedHeads ? pawn.gender.ToString() : ""), pawn.GetComp<AlienComp>().crownType = this.aliencrowntypes[Rand.Range(min: 0, this.aliencrowntypes.Count)]);

        public static string GetAlienHead(string userpath, string gender, string crowntype) => userpath.NullOrEmpty() ? "" : userpath + (userpath == GraphicPaths.VANILLA_HEAD_PATH ? gender + "/" : "") + (!gender.NullOrEmpty() ? gender + "_" : "") + crowntype;

        public Graphic GetNakedGraphic(BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender) =>
            GraphicDatabase.Get(typeof(Graphic_Multi), GetNakedPath(bodyType, userpath, this.useGenderedBodies ? gender : ""), shader, Vector2.one, 
                                skinColor, skinColorSecond, data: null, shaderParameters: null);
            //GraphicDatabase.Get<Graphic_Multi>(path: GetNakedPath(bodyType: bodyType, userpath: userpath, gender: this.useGenderedBodies ? gender : ""), shader: shader, drawSize: Vector2.one, color: skinColor, colorTwo: skinColorSecond);

        public static string GetNakedPath(BodyTypeDef bodyType, string userpath, string gender) => userpath + (!gender.NullOrEmpty() ? gender + "_" : "") + "Naked_" + bodyType;

        public Color SkinColor(Pawn alien, bool first = true)
        {
            
            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            ExposableValueTuple<Color, Color> skinColors = alienComp.GetChannel(channel: "skin");
            return first ? skinColors.first : skinColors.second;
        }

        public void GenerateMeshsAndMeshPools()
        {
            void AddMeshSet(Vector2 drawSize, Vector2 headDrawSize)
            {
                if (!meshPools.Keys.Any(predicate: v => v.Equals(drawSize)))
                {
                    meshPools.Add(drawSize, new AlienGraphicMeshSet
                                            {
                                                bodySet        = new GraphicMeshSet(1.5f * drawSize.x,     1.5f * drawSize.y),     // bodySet
                                                headSet        = new GraphicMeshSet(1.5f * headDrawSize.x, 1.5f * headDrawSize.y), // headSet
                                                hairSetAverage = new GraphicMeshSet(1.5f * headDrawSize.x, 1.5f * headDrawSize.y), // hairSetAverage
                                            });
                }
            }

            foreach (GraphicPaths graphicsPath in this.alienProps.alienRace.graphicPaths.Concat(
                                                                                                new GraphicPaths
                                                                                                {
                                                                                                    customDrawSize             = this.customDrawSize,
                                                                                                    customHeadDrawSize         = this.customHeadDrawSize,
                                                                                                    customPortraitDrawSize     = this.customPortraitDrawSize,
                                                                                                    customPortraitHeadDrawSize = this.customPortraitHeadDrawSize
                                                                                                }))
            {
                AddMeshSet(graphicsPath.customDrawSize,             graphicsPath.customDrawSize);
                AddMeshSet(graphicsPath.customHeadDrawSize,         graphicsPath.customHeadDrawSize);
                AddMeshSet(graphicsPath.customPortraitDrawSize,     graphicsPath.customPortraitDrawSize);
                AddMeshSet(graphicsPath.customPortraitHeadDrawSize, graphicsPath.customPortraitHeadDrawSize);
            }


            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Center",
                                        offsets = new BodyAddonOffsets()
                                    });
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Tail",
                                        offsets = new BodyAddonOffsets
                                                  {
                                                      south = new RotationOffset
                                                              {
                                                                  offset      = new Vector2(0.42f, -0.22f)
                                                              },
                                                      north = new RotationOffset
                                                              {
                                                                  offset      = new Vector2(0f, -0.55f)
                                                              },
                                                      east = new RotationOffset
                                                             {
                                                                 offset      = new Vector2(0.42f, -0.22f)
                                                             },
                                                      west = new RotationOffset
                                                             {
                                                                 offset = new Vector2(0.42f, -0.22f)
                                                             }
                                        }
                                    });
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Head",
                                        offsets = new BodyAddonOffsets
                                                  {
                                                      south = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0, 0.5f)
                                                              },
                                                      north = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0f, 0.35f)
                                                              },
                                                      east = new RotationOffset
                                                             {
                                                                 offset = new Vector2(-0.07f, 0.5f)
                                                             },
                                                      west = new RotationOffset
                                                             {
                                                                offset = new Vector2(-0.07f, 0.5f)
                                                             }
                                                  }
                                    });



            StringBuilder logBuilder = new StringBuilder();
            this.bodyAddons.Do(action: ba =>
            { 
                ba.defaultOffsets = this.offsetDefaults.Find(on => on.name == ba.defaultOffset).offsets;

                void AddToStringBuilder(string s)
                {
                    if (ba.debug)
                        logBuilder.AppendLine(s);
                }

                if (ba.variantCount != 0) return;

                AddToStringBuilder(s: $"Loading variants for {ba.path}");

                while (ContentFinder<Texture2D>.Get($"{ba.path}{(ba.variantCount == 0 ? "" : ba.variantCount.ToString())}_north", reportFailure: false) != null)
                    ba.variantCount++;

                AddToStringBuilder(s: $"Variants found for {ba.path}: {ba.variantCount}");

                if (ba.hediffGraphics != null)
                {
                    foreach (BodyAddonHediffGraphic bahg in ba.hediffGraphics.Where(predicate: bahg => bahg.variantCount == 0))
                    {
                        while (ContentFinder<Texture2D>.Get(bahg.path + (bahg.variantCount == 0 ? "" : bahg.variantCount.ToString()) + "_north", reportFailure: false) != null)
                            bahg.variantCount++;
                        AddToStringBuilder($"Variants found for {bahg.path}: {bahg.variantCount}");
                        if (bahg.variantCount == 0)
                            Log.Warning($"No hediff graphics found at {bahg.path} for hediff {bahg.hediff} in {this.alienProps.defName}");

                        if (bahg.severity != null)
                        {
                            foreach (BodyAddonHediffSeverityGraphic bahsg in bahg.severity)
                            {
                                while (ContentFinder<Texture2D>.Get(bahsg.path + (bahsg.variantCount == 0 ? "" : bahsg.variantCount.ToString()) + "_north", reportFailure: false) != null)
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
                    foreach (BodyAddonBackstoryGraphic babg in ba.backstoryGraphics.Where(predicate: babg => babg.variantCount == 0))
                    {
                        while (ContentFinder<Texture2D>.Get(babg.path + (babg.variantCount == 0 ? "" : babg.variantCount.ToString()) + "_north", reportFailure: false) != null)
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

        public class OffsetNamed
        {
            public string           name = "";
            public BodyAddonOffsets offsets;
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
            private Dictionary<string, HashSet<ExposableValueTuple<string, bool>>>   colorChannelLinks = new Dictionary<string, HashSet<ExposableValueTuple<string, bool>>>();

            public Dictionary<string, ExposableValueTuple<Color, Color>> ColorChannels
            {
                get
                {
                    if (this.colorChannels == null || !this.colorChannels.Any())
                    {
                        this.colorChannels     = new Dictionary<string, ExposableValueTuple<Color, Color>>();
                        this.colorChannelLinks = new Dictionary<string, HashSet<ExposableValueTuple<string, bool>>>();
                        Pawn               pawn       = (Pawn) this.parent;
                        ThingDef_AlienRace alienProps = ((ThingDef_AlienRace) this.parent.def);
                        AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                        this.colorChannels.Add(key: "base", new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                        this.colorChannels.Add(key: "hair", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        Color skinColor = PawnSkinColors.GetSkinColor(pawn.story.melanin);

                        this.colorChannels.Add(key: "skin", new ExposableValueTuple<Color, Color>(skinColor, skinColor));

                        foreach (ColorChannelGenerator channel in apg.colorChannels)
                        {
                            if (!this.colorChannels.ContainsKey(channel.name))
                                this.colorChannels.Add(channel.name, new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                            ExposableValueTuple<Color, Color> colors = this.colorChannels[channel.name];
                            if (channel.first != null)
                                colors.first = this.GenerateColor(channel, true);
                            if (channel.second != null)
                                colors.second = this.GenerateColor(channel, false);
                        }

                        ExposableValueTuple<Color, Color> hairColors = this.colorChannels[key: "hair"];
                        if (hairColors.first == Color.clear)
                        {
                            Color color = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                            hairColors.first  = color;
                            hairColors.second = color;
                        }

                        if (pawn.Corpse?.GetRotStage() == RotStage.Rotting)
                            this.colorChannels["skin"].first = PawnGraphicSet.RottingColorDefault;
                        pawn.story.hairColor = hairColors.first;

                        this.RegenerateColorChannelLink("skin");
                        
                        if (alienProps.alienRace.hairSettings.getsGreyAt <= pawn.ageTracker.AgeBiologicalYears)
                        {
                            if (Rand.Value < GenMath.SmootherStep(alienProps.alienRace.hairSettings.getsGreyAt,
                                                                  pawn.RaceProps.ageGenerationCurve.Points.Count < 3
                                                                             ? alienProps.alienRace.hairSettings.getsGreyAt + 35
                                                                             : pawn.RaceProps.ageGenerationCurve.Points.Skip(pawn.RaceProps.ageGenerationCurve.Points.Count - 3).First().x,
                                                                  pawn.ageTracker.AgeBiologicalYears))
                            {
                                float grey = Rand.Range(min: 0.65f, max: 0.85f);
                                pawn.story.hairColor = new Color(grey, grey, grey);
                                hairColors.first     = pawn.story.hairColor;
                            }
                        }
                    }

                    return this.colorChannels;
                }
            }

            public Color GenerateColor(ColorChannelGenerator channel, bool first)
            {
                ColorGenerator gen = first ? channel.first : channel.second;

                switch (gen)
                {
                    case ColorGenerator_CustomAlienChannel ac:
                        string[] split = ac.colorChannel.Split('_');
                        if (!this.colorChannelLinks.ContainsKey(split[0]))
                            this.colorChannelLinks.Add(split[0], new HashSet<ExposableValueTuple<string, bool>>());

                        this.colorChannelLinks[split[0]].Add(new ExposableValueTuple<string, bool>(channel.name, first));
                        return split[1] == "1" ? this.ColorChannels[split[0]].first : this.ColorChannels[split[0]].second;
                    case ColorGenerator_SkinColorMelanin cm:
                        return cm.naturalMelanin ? 
                                   PawnSkinColors.GetSkinColor(((Pawn) this.parent).story.melanin) : 
                                   gen.NewRandomizedColor();
                    default:
                        return gen.NewRandomizedColor();
                }
            }

            public override void PostSpawnSetup(bool respawningAfterLoad)
            {
                base.PostSpawnSetup(respawningAfterLoad);
                AlienPartGenerator apg = ((ThingDef_AlienRace) this.parent.def).alienRace.generalSettings.alienPartGenerator;
                this.customDrawSize             = apg.customDrawSize;
                this.customHeadDrawSize         = apg.customHeadDrawSize;
                this.customPortraitDrawSize     = apg.customPortraitDrawSize;
                this.customPortraitHeadDrawSize = apg.customPortraitHeadDrawSize;
            }

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.Look(ref this.fixGenderPostSpawn, label: "fixAlienGenderPostSpawn");
                Scribe_Values.Look(ref this.crownType,          label: "crownType");
                Scribe_Collections.Look(ref this.addonVariants, label: "addonVariants");
                Scribe_Collections.Look(ref this.colorChannels, label: "colorChannels");
                Scribe_NestedCollections.Look(ref this.colorChannelLinks, label: "colorChannelLinks", LookMode.Undefined, LookMode.Undefined);

                if(this.colorChannelLinks == null)
                    this.colorChannelLinks = new Dictionary<string, HashSet<ExposableValueTuple<string, bool>>>();
                    
            }

            public ExposableValueTuple<Color, Color> GetChannel(string channel)
            {
                if (this.ColorChannels.ContainsKey(channel))
                    return this.ColorChannels[channel];

                ThingDef_AlienRace alienProps = ((ThingDef_AlienRace)this.parent.def);
                AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                foreach (ColorChannelGenerator apgChannel in apg.colorChannels)
                    if (apgChannel.name == channel)
                    {
                        this.ColorChannels.Add(channel, new ExposableValueTuple<Color, Color>());
                        if (apgChannel.first != null)
                            this.ColorChannels[channel].first = this.GenerateColor(apgChannel, true);
                        if (apgChannel.second != null)
                            this.ColorChannels[channel].second = this.GenerateColor(apgChannel, false);

                        return this.ColorChannels[channel];
                    }

                return new ExposableValueTuple<Color, Color>(Color.white, Color.white);
            }

            internal void AssignProperMeshs()
            {
                this.alienGraphics             = meshPools[this.customDrawSize];
                this.alienHeadGraphics         = meshPools[this.customHeadDrawSize];
                this.alienPortraitGraphics     = meshPools[this.customPortraitDrawSize];
                this.alienPortraitHeadGraphics = meshPools[this.customPortraitHeadDrawSize];
            }

            public void RegenerateColorChannelLinks()
            {
                foreach (KeyValuePair<string, HashSet<ExposableValueTuple<string, bool>>> kvp in this.colorChannelLinks) 
                    this.RegenerateColorChannelLink(kvp.Key);
            }

            public void RegenerateColorChannelLink(string channel)
            {
                ThingDef_AlienRace alienProps = ((ThingDef_AlienRace)this.parent.def);
                AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                if (this.colorChannelLinks.ContainsKey(channel))
                    foreach (ExposableValueTuple<string, bool> link in this.colorChannelLinks[channel])
                    {
                        foreach (ColorChannelGenerator apgChannel in apg.colorChannels)
                        {
                            if (apgChannel.name == link.first)
                            {
                                if (link.second)
                                    this.ColorChannels[link.first].first = this.GenerateColor(apgChannel, true);
                                else
                                    this.ColorChannels[link.first].second = this.GenerateColor(apgChannel, false);
                                
                            }
                        }
                    }
            }

            public void OverwriteColorChannel(string channel, Color? first = null, Color? second = null)
            {
                if (!this.ColorChannels.ContainsKey(channel))
                    return;

                if (first.HasValue)
                    this.ColorChannels[channel].first = first.Value;
                if (second.HasValue)
                    this.ColorChannels[channel].second = second.Value;
            }

            [DebugAction(category: "AlienRace", name: "Regenerate all colorchannels", allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

        public class ExposableValueTuple<K, V> : IExposable, IEquatable<ExposableValueTuple<K,V>>
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

            public bool Equals(ExposableValueTuple<K, V> other) => 
                other != null && this.first.Equals(other.first) && this.second.Equals(other.second);

            public override int GetHashCode() => this.first.GetHashCode() + this.second.GetHashCode();

            public void ExposeData()
            {
                Scribe_Values.Look(ref this.first, label: "first");
                Scribe_Values.Look(ref this.second, label: "second");
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