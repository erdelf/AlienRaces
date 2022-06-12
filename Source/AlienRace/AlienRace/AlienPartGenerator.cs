namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BodyAddonSupport;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public partial class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string> { "Average_Normal" };

        public List<BodyTypeDef> alienbodytypes = new List<BodyTypeDef>();

        public bool useGenderedHeads = true;
        public bool useGenderedBodies = false;

        public int getsGreyAt = 40;


        public List<ColorChannelGenerator> colorChannels  = new List<ColorChannelGenerator>();
        public List<OffsetNamed>      offsetDefaults = new List<OffsetNamed>();
        

        public List<WoundAnchorReplacement> anchorReplacements = new List<WoundAnchorReplacement>();

        public Vector2 headOffset = Vector2.zero;
        public DirectionOffset headOffsetDirectional = new DirectionOffset();

        public float borderScale = 1f;
        public int atlasScale = 1;

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

        public Graphic GetNakedGraphic(BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender, string maskPath) =>
            GraphicDatabase.Get(typeof(Graphic_Multi), GetNakedPath(bodyType, userpath, this.useGenderedBodies ? gender : ""), shader, Vector2.one, 
                                skinColor, skinColorSecond, data: null, shaderParameters: null, maskPath: maskPath);

        public static string GetNakedPath(BodyTypeDef bodyType, string userpath, string gender) => userpath + (!gender.NullOrEmpty() ? gender + "_" : "") + "Naked_" + bodyType;

        public Color SkinColor(Pawn alien, bool first = true)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();

            if (alienComp == null) 
                return PawnSkinColors.GetSkinColor(alien.story.melanin);

            ExposableValueTuple<Color, Color> skinColors = alienComp.GetChannel(channel: "skin");
            return first ? skinColors.first : skinColors.second;
        }

        private void GenerateMeshSets()
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
        }

        private void GenerateOffsetDefaults()
        {
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

        }

        public void GenerateMeshsAndMeshPools()
        {
            this.GenerateMeshsAndMeshPools(new DefaultGraphicsLoader());
        }
        
        public void GenerateMeshsAndMeshPools(IGraphicsLoader graphicsLoader)
        {
            this.GenerateMeshSets();
            this.GenerateOffsetDefaults();
            graphicsLoader.LoadAllGraphics(this.alienProps.defName, this.offsetDefaults, this.bodyAddons);
        }

        public class WoundAnchorReplacement
        {
            public string originalTag = string.Empty;
            public BodyPartGroupDef originalGroup;

            public BodyTypeDef.WoundAnchor replacement;
            public BodyAddonOffsets        offsets;

            public bool ValidReplacement(BodyTypeDef.WoundAnchor original)
            {
                if (original.rotation != this.replacement.rotation) 
                    return false;

                if (!this.originalTag.NullOrEmpty() && !original.tag.NullOrEmpty() && this.originalTag == original.tag)
                    return true;
                if (this.originalGroup != null && original.@group != null && this.originalGroup == original.@group)
                    return true;
                return false;
            }
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
            public int                 headMaskVariant = -1;
            public int                 bodyMaskVariant = -1;
            public List<Graphic>       addonGraphics;
            public List<int>           addonVariants;

            public int lastAlienMeatIngestedTick = 0;

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

                        Color tattooColor = skinColor;
                        tattooColor.a *= 0.8f;
                        this.colorChannels.Add(key: "tattoo", new ExposableValueTuple<Color, Color>(tattooColor, tattooColor));

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
                        
                        
                        if (alienProps.alienRace.generalSettings.alienPartGenerator.getsGreyAt <= pawn.ageTracker.AgeBiologicalYears)
                        {
                            if (Rand.Value < GenMath.SmootherStep(alienProps.alienRace.generalSettings.alienPartGenerator.getsGreyAt,
                                                                  pawn.RaceProps.ageGenerationCurve.Points.Count < 3
                                                                             ? alienProps.alienRace.generalSettings.alienPartGenerator.getsGreyAt + alienProps.race.lifeExpectancy / 3f
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
                Scribe_Values.Look(ref this.headMaskVariant, nameof(this.headMaskVariant), -1);
                Scribe_Values.Look(ref this.bodyMaskVariant, nameof(this.bodyMaskVariant), -1);

                if (this.colorChannelLinks == null)
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