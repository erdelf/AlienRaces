namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using ApparelGraphics;
    using ExtendedGraphics;
    using JetBrains.Annotations;
    using LudeonTK;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Gender = Verse.Gender;

    public partial class AlienPartGenerator
    {
        public List<HeadTypeDef> headTypes;

        public List<HeadTypeDef> HeadTypes =>
            this.headTypes ?? CachedData.DefaultHeadTypeDefs;

        public List<BodyTypeDef> bodyTypes = new();

        [LoadDefFromField("Male")]
        public BodyTypeDef defaultMaleBodyType;

        [LoadDefFromField("Female")]
        public BodyTypeDef defaultFemaleBodyType;

        public FloatRange  oldHairAgeRange = new(-1f, -1f);
        public SimpleCurve oldHairAgeCurve = new();

        public ColorGenerator oldHairColorGen = new ColorGenerator_Options
                                                {
                                                    options = new List<ColorOption>
                                                              {
                                                                  new() { only = new Color(0.65f, 0.65f, 0.65f) },
                                                                  new() { only = new Color(0.70f, 0.70f, 0.70f) },
                                                                  new() { only = new Color(0.75f, 0.75f, 0.75f) },
                                                                  new() { only = new Color(0.80f, 0.80f, 0.80f) },
                                                                  new() { only = new Color(0.85f, 0.85f, 0.85f) },
                                                              }
                                                };


        public List<ColorChannelGenerator> colorChannels  = new();

        [Unsaved]
        public Dictionary<string, OffsetNamed> offsetDefaultsDictionary;
        public List<OffsetNamed>           offsetDefaults = new();


        public List<WoundAnchorReplacement> anchorReplacements = new();

        public Vector2 headOffset = Vector2.zero;

        public DirectionalOffset headOffsetDirectional = new();

        public Vector2           headFemaleOffset = Vector2.negativeInfinity;
        public DirectionalOffset headFemaleOffsetDirectional;

        public float borderScale = 1f;
        public int   atlasScale  = 1;

        public Vector2 customDrawSize             = Vector2.one;
        public Vector2 customPortraitDrawSize     = Vector2.one;
        public Vector2 customHeadDrawSize         = Vector2.zero;
        public Vector2 customPortraitHeadDrawSize = Vector2.zero;

        public Vector2 customFemaleDrawSize             = Vector2.zero;
        public Vector2 customFemalePortraitDrawSize     = Vector2.zero;
        public Vector2 customFemaleHeadDrawSize         = Vector2.zero;
        public Vector2 customFemalePortraitHeadDrawSize = Vector2.zero;

        public BodyPartDef headBodyPartDef;

        public List<BodyAddon> bodyAddons = new();

        public ThingDef_AlienRace alienProps;

        public Color SkinColor(Pawn alien, bool first = true)
        {
            if(alien.Drawer.renderer.StatueColor.HasValue)
                return alien.Drawer.renderer.StatueColor.Value;

            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            
            ExposableValueTuple<Color, Color> skinColors = alienComp.GetChannel(channel: "skin");
            return first ? skinColors.first : skinColors.second;
        }

        public void GenericOffsets() =>
            this.GenerateOffsetDefaults();

        private void GenerateOffsetDefaults()
        {
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name    = "Center",
                                        offsets = new DirectionalOffset()
                                    });
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Tail",
                                        offsets = new DirectionalOffset
                                                  {
                                                      south = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0.42f, -0.22f)
                                                              },
                                                      north = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0f, -0.55f)
                                                              },
                                                      east = new RotationOffset
                                                             {
                                                                 offset = new Vector2(0.42f, -0.22f)
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
                                        offsets = new DirectionalOffset
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
            if (this.oldHairAgeCurve.PointsCount <= 0)
            {
                float minAge = this.oldHairAgeRange.min <= 0 ?
                                   this.alienProps.race.lifeExpectancy / 2f :
                                   this.oldHairAgeRange.TrueMin;

                float maxAge = this.oldHairAgeRange.max <= 0 ?
                                   this.alienProps.race.lifeExpectancy * 0.95f :
                                   this.oldHairAgeRange.TrueMax;

                this.oldHairAgeCurve.Add(0f,     0f);
                this.oldHairAgeCurve.Add(minAge, 0f);


                float ageDiff = maxAge - minAge;
                float step    = ageDiff / 5f;
                for (float i = minAge + step; i < maxAge; i += ageDiff / step)
                    this.oldHairAgeCurve.Add(i, GenMath.SmootherStep(minAge, maxAge, i));

                this.oldHairAgeCurve.Add(maxAge, 1f);
            }

            this.GenerateOffsetDefaults();

            {
                if (!this.alienProps.alienRace.graphicPaths.head.GetSubGraphics().Any())
                {
                    ExtendedGraphicTop headGraphic = this.alienProps.alienRace.graphicPaths.head;
                    string             headPath    = headGraphic.path;

                    foreach (HeadTypeDef headType in DefDatabase<HeadTypeDef>.AllDefs) //.Where(htd => !htd.requiredGenes.NullOrEmpty())))
                    {
                        string headTypePath = Path.GetFileName(headType.graphicPath);

                        int  ind            = headTypePath.IndexOf('_');
                        bool genderIncluded = headType.gender != Gender.None && ind >= 0 && Enum.TryParse(headTypePath.Substring(0, ind), out Gender _);
                        headTypePath = genderIncluded ? headTypePath.Substring(ind + 1) : headTypePath;

                        ExtendedConditionGraphic headtypeGraphic = new()
                                                                   {
                                                                       conditions    = [new ConditionHeadType { headType = headType }],
                                                                       path          = headPath.NullOrEmpty() ? string.Empty : headPath + headTypePath,
                                                                       pathsFallback = [headType.graphicPath]
                                                                   };

                        Gender firstGender = genderIncluded ? headType.gender : Gender.Male;

                        headtypeGraphic.extendedGraphics.Add(new ExtendedConditionGraphic
                                                             {
                                                                 conditions = [new ConditionGender { gender = firstGender }],
                                                                 path       = headPath + firstGender + "_" + headTypePath
                                                             });

                        if (!genderIncluded)
                            headtypeGraphic.extendedGraphics.Add(new ExtendedConditionGraphic
                                                                 {
                                                                     conditions = [new ConditionGender { gender = Gender.Female }],
                                                                     path       = headPath + Gender.Female + headTypePath
                                                                 });

                        headGraphic.extendedGraphics.Add(headtypeGraphic);
                    }
                }
                this.alienProps.alienRace.graphicPaths.head.resolveData.head = true;

                //Log.Message(string.Join("\n", this.alienProps.alienRace.graphicPaths.head.headtypeGraphics.Select(ehg => $"{ehg.headType.defName}: {ehg.path} | {string.Join("|", ehg.genderGraphics?.Select(egg => $"{egg.gender}: {egg.path}") ?? new []{string.Empty})}")));

                if (!this.alienProps.alienRace.graphicPaths.body.GetSubGraphics().Any())
                {
                    ExtendedGraphicTop bodyGraphic = this.alienProps.alienRace.graphicPaths.body;
                    string             bodyPath    = bodyGraphic.path;


                    foreach (CreepJoinerFormKindDef formKindDef in DefDatabase<CreepJoinerFormKindDef>.AllDefsListForReading)
                    {
                        ExtendedConditionGraphic formGraphic = new()
                                                               {
                                                                   conditions = [new ConditionCreepJoinerFormKind { form = formKindDef }],
                                                                   path       = $"{bodyPath}_{formKindDef}"
                                                               };
                        foreach (BodyTypeGraphicData bodyTypeData in formKindDef.bodyTypeGraphicPaths)
                        {
                            formGraphic.extendedGraphics.Add(new ExtendedConditionGraphic()
                                                             {
                                                                 conditions    = [new ConditionBodyType { bodyType = bodyTypeData.bodyType }],
                                                                 path          = $"{bodyPath}_{formKindDef}_{bodyTypeData.bodyType}",
                                                                 pathsFallback = [bodyTypeData.texturePath]
                                                             });
                        }

                        bodyGraphic.extendedGraphics.Add(formGraphic);
                    }

                    foreach (MutantDef mutantDef in DefDatabase<MutantDef>.AllDefsListForReading)
                    {
                        ExtendedConditionGraphic mutantGraphic = new()
                                                                 {
                                                                     conditions = [new ConditionMutant { mutant = mutantDef }],
                                                                     path       = $"{bodyPath}_{mutantDef}"
                                                                 };
                        foreach (BodyTypeGraphicData bodyTypeData in mutantDef.bodyTypeGraphicPaths)
                            mutantGraphic.extendedGraphics.Add(new ExtendedConditionGraphic
                                                               {
                                                                   conditions    = [new ConditionBodyType { bodyType = bodyTypeData.bodyType }],
                                                                   path          = $"{bodyPath}_{mutantDef}_{bodyTypeData.bodyType}",
                                                                   pathsFallback = [bodyTypeData.texturePath]
                                                               });
                        bodyGraphic.extendedGraphics.Add(mutantGraphic);
                    }


                    foreach (BodyTypeDef bodyTypeRaw in this.bodyTypes)
                    {
                        BodyTypeDef bodyType = bodyTypeRaw == BodyTypeDefOf.Baby ? BodyTypeDefOf.Child : bodyTypeRaw;

                        bodyGraphic.extendedGraphics.Add(new ExtendedConditionGraphic
                                                         {
                                                             conditions = [new ConditionBodyType { bodyType = bodyTypeRaw }],
                                                             path       = $"{bodyPath}Naked_{bodyType.defName}",
                                                             extendedGraphics =
                                                             [
                                                                 new ExtendedConditionGraphic
                                                                 {
                                                                     conditions = [new ConditionGender { gender = Gender.Male }],
                                                                     path       = $"{bodyPath}{Gender.Male}_Naked_{bodyType.defName}"
                                                                 },

                                                                 new ExtendedConditionGraphic
                                                                 {
                                                                     conditions = [new ConditionGender { gender = Gender.Female }],
                                                                     path       = $"{bodyPath}{Gender.Female}_Naked_{bodyType.defName}"
                                                                 }
                                                             ]
                                                         });
                    }
                }
            }

            {
                foreach (ExtendedGraphicTop graphicTop in this.alienProps.alienRace.graphicPaths.apparel.individualPaths.Values)
                {
                    if (!graphicTop.GetSubGraphics().Any())
                    {
                        string path = graphicTop.path;

                        foreach (BodyTypeDef bodyType in this.bodyTypes)
                            graphicTop.extendedGraphics.Add(new ExtendedConditionGraphic()
                                                            {
                                                                conditions = [new ConditionBodyType { bodyType = bodyType }],
                                                                path       = $"{path}_{bodyType.defName}",
                                                                extendedGraphics =
                                                                [
                                                                    new ExtendedConditionGraphic()
                                                                    {
                                                                        conditions = [new ConditionGender { gender = Gender.Male }],
                                                                        path       = $"{path}_{Gender.Male}_{bodyType.defName}"
                                                                    },

                                                                    new ExtendedConditionGraphic
                                                                    {
                                                                        conditions = [new ConditionGender { gender = Gender.Female }],
                                                                        path       = $"{path}_{Gender.Female}_{bodyType.defName}"
                                                                    }
                                                                ]
                                                            });
                    }
                }

                foreach (ApparelReplacementOption fallback in this.alienProps.alienRace.graphicPaths.apparel.fallbacks)
                {
                    foreach (ExtendedGraphicTop graphicTop in fallback.wornGraphicPaths.Concat(fallback.wornGraphicPath))
                    {
                        if (!graphicTop.GetSubGraphics().Any())
                        {
                            string path = graphicTop.path;
                            foreach (BodyTypeDef bodyType in this.bodyTypes)
                                graphicTop.extendedGraphics.Add(new ExtendedConditionGraphic()
                                                                {
                                                                    conditions = [new ConditionBodyType { bodyType = bodyType }],
                                                                    path       = $"{path}_{bodyType.defName}",
                                                                    extendedGraphics =
                                                                    [
                                                                        new ExtendedConditionGraphic
                                                                        {
                                                                            conditions = [new ConditionGender { gender = Gender.Male }],
                                                                            path       = $"{path}_{Gender.Male}_{bodyType.defName}"
                                                                        },
                                                                        new ExtendedConditionGraphic
                                                                        {
                                                                            conditions = [new ConditionGender { gender = Gender.Female }],
                                                                            path       = $"{path}_{Gender.Female}_{bodyType.defName}"
                                                                        }
                                                                    ]
                                                                });
                        }
                    }
                }

                foreach (ApparelReplacementOption overrides in this.alienProps.alienRace.graphicPaths.apparel.overrides)
                {
                    foreach (ExtendedGraphicTop graphicTop in overrides.wornGraphicPaths.Concat(overrides.wornGraphicPath))
                    {
                        if (!graphicTop.GetSubGraphics().Any())
                        {
                            string path = graphicTop.path;
                            foreach (BodyTypeDef bodyType in this.bodyTypes)
                                graphicTop.extendedGraphics.Add(new ExtendedConditionGraphic()
                                                                {
                                                                    conditions = [new ConditionBodyType { bodyType = bodyType }],
                                                                    path       = $"{path}_{bodyType.defName}",
                                                                    extendedGraphics =
                                                                    [
                                                                        new ExtendedConditionGraphic
                                                                        {
                                                                            conditions = [new ConditionGender { gender = Gender.Male }],
                                                                            path       = $"{path}_{Gender.Male}_{bodyType.defName}"
                                                                        },
                                                                        new ExtendedConditionGraphic
                                                                        {
                                                                            conditions = [new ConditionGender { gender = Gender.Female }],
                                                                            path       = $"{path}_{Gender.Female}_{bodyType.defName}"
                                                                        }
                                                                    ]
                                                                });
                        }
                    }
                }
            }

            this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.Init();
            if (!this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.GetPath().NullOrEmpty())
                this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.IncrementVariantCount();
            
            Stack<IEnumerable<IExtendedGraphic>> stack = new();

            stack.Push(this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.GetSubGraphics());
            while (stack.Count > 0)
            {
                IEnumerable<IExtendedGraphic> currentGraphicSet = stack.Pop();

                foreach (IExtendedGraphic current in currentGraphicSet)
                    if (current != null)
                    {
                        current.Init();
                        if (!current.GetPath().NullOrEmpty())
                            current.IncrementVariantCount();

                        stack.Push(current.GetSubGraphics());
                    }
            }

            this.alienProps.alienRace.graphicPaths.skull.resolveData.head     = true;
            this.alienProps.alienRace.graphicPaths.stump.resolveData.head     = true;
            this.alienProps.alienRace.graphicPaths.headMasks.resolveData.head = true;

            if (!graphicsQueue.Any())
                Application.onBeforeRender += LoadGraphicsHook;

            //Log.Message("queueing graphics for: " + this.alienProps.defName);
            graphicsQueue.Add(this);

            this.offsetDefaultsDictionary = new Dictionary<string, OffsetNamed>();
            foreach (OffsetNamed offsetDefault in this.offsetDefaults)
                this.offsetDefaultsDictionary[offsetDefault.name] = offsetDefault;

            foreach (BodyAddon bodyAddon in this.bodyAddons)
                bodyAddon.defaultOffsets = this.offsetDefaultsDictionary[bodyAddon.defaultOffset].offsets;
        }

        public static readonly HashSet<AlienPartGenerator> graphicsQueue = [];

        private static readonly IGraphicsLoader graphicsLoader = new DefaultGraphicsLoader();

        public static void LoadGraphicsHook()
        {
            if (!AlienRaceMod.instance.Content.GetContentHolder<Texture2D>()?.contentList?.Any() ?? true)
                return;

            foreach (AlienPartGenerator apg in graphicsQueue)
            {
                //Log.Message("resolving graphics for: " + apg.alienProps.defName);
                graphicsLoader.LoadAllGraphics(apg.alienProps.defName, apg.alienProps.alienRace.graphicPaths.head, apg.alienProps.alienRace.graphicPaths.body, apg.alienProps.alienRace.graphicPaths.skeleton, apg.alienProps.alienRace.graphicPaths.skull, apg.alienProps.alienRace.graphicPaths.stump, apg.alienProps.alienRace.graphicPaths.bodyMasks, apg.alienProps.alienRace.graphicPaths.headMasks);

                graphicsLoader.LoadAllGraphics(apg.alienProps.defName, apg.alienProps.alienRace.graphicPaths.apparel.individualPaths.Values.Concat(
                                                apg.alienProps.alienRace.graphicPaths.apparel.fallbacks.SelectMany(afo => 
                                                                                                                       afo.wornGraphicPaths.Concat(afo.wornGraphicPath))).Concat(
                                                    apg.alienProps.alienRace.graphicPaths.apparel.overrides.SelectMany(afo => 
                                                                                                                           afo.wornGraphicPaths.Concat(afo.wornGraphicPath))).ToArray());

                graphicsLoader.LoadAllGraphics(apg.alienProps.defName + " Addons", apg.bodyAddons.Cast<ExtendedGraphicTop>().ToArray());
            }

            graphicsQueue.Clear();

            Application.onBeforeRender -= LoadGraphicsHook;
        }

        public class WoundAnchorReplacement
        {
            public string           originalTag = string.Empty;
            public BodyPartGroupDef originalGroup;

            public BodyTypeDef.WoundAnchor replacement;
            public DirectionalOffset       offsets;

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
            public string            name = "";
            public DirectionalOffset offsets;
        }

        public class ColorChannelGenerator
        {
            public string                              name    = "";
            public List<ColorChannelGeneratorCategory> entries = new();


            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    switch (xmlNode.Name)
                    {
                        case "name":
                            this.name = xmlNode.InnerText.Trim();
                            break;
                        case "first":
                            if (this.entries.NullOrEmpty())
                                this.entries.Add(new ColorChannelGeneratorCategory() { weight = 100f });
                            this.entries[0].first = DirectXmlToObject.ObjectFromXml<ColorGenerator>(xmlNode, false);
                            break;
                        case "second":
                            if (this.entries.NullOrEmpty())
                                this.entries.Add(new ColorChannelGeneratorCategory() { weight = 100f });
                            this.entries[0].second = DirectXmlToObject.ObjectFromXml<ColorGenerator>(xmlNode, false);
                            break;
                        case "entries":
                            this.entries = DirectXmlToObject.ObjectFromXml<List<ColorChannelGeneratorCategory>>(xmlNode, false);
                            break;
                    }
            }
        }

        public class ColorChannelGeneratorCategory
        {
            public float          weight = float.Epsilon;
            public ColorGenerator first;
            public ColorGenerator second;
        }


        public class AlienComp : ThingComp
        {
            private const string ScribeNodeName = "AlienRaces_AlienComp";

            public PawnKindDef originalKindDef;

            public bool    fixGenderPostSpawn;
            public Vector2 customDrawSize             = Vector2.one;
            public Vector2 customHeadDrawSize         = Vector2.one;
            public Vector2 customPortraitDrawSize     = Vector2.one;
            public Vector2 customPortraitHeadDrawSize = Vector2.one;

            public int bodyVariant     = -1;
            public int headVariant     = -1;
            public int headMaskVariant = -1;
            public int bodyMaskVariant = -1;

            public List<Graphic> addonGraphics;
            public List<int>     addonVariants;
            public List<ExposableValueTuple<Color?, Color?>>   addonColors = [];


            public int lastAlienMeatIngestedTick = 0;

            private Dictionary<string, ExposableValueTuple<Color, Color>>                                    colorChannels;
            private Dictionary<string, ColorChannelLinkData> colorChannelLinks = [];
            // originalChannelName, ((targetChannelName, targetChannelCategoryIndex), targetChannelFirst)

            public class ColorChannelLinkData : IExposable
            {
                public class ColorChannelLinkTargetData : IExposable
                {
                    public string targetChannel;
                    public int    categoryIndex;
                    public void   ExposeData()
                    {
                        Scribe_Values.Look(ref this.targetChannel, nameof(this.targetChannel));
                        Scribe_Values.Look(ref this.categoryIndex, nameof(this.categoryIndex));
                    }
                }

                public string originalChannel;

                public HashSet<ColorChannelLinkTargetData> targetsChannelOne = [];
                public HashSet<ColorChannelLinkTargetData> targetsChannelTwo = [];

                public HashSet<ColorChannelLinkTargetData> GetTargetDataFor(bool first) =>
                    first ? this.targetsChannelOne : this.targetsChannelTwo;

                public void ExposeData()
                {
                    Scribe_Values.Look(ref this.originalChannel, nameof(this.originalChannel));
                    Scribe_Collections.Look(ref this.targetsChannelOne, nameof(this.targetsChannelOne));
                    Scribe_Collections.Look(ref this.targetsChannelTwo, nameof(this.targetsChannelTwo));
                }
            }


            public Dictionary<string, ColorChannelLinkData> ColorChannelLinks => this.colorChannelLinks;

            private Pawn               Pawn       => (Pawn)this.parent;
            private ThingDef_AlienRace AlienProps => (ThingDef_AlienRace)this.Pawn.def;

            public Dictionary<string, ExposableValueTuple<Color, Color>> ColorChannels
            {
                get
                {
                    if (this.colorChannels == null || !this.colorChannels.Any())
                    {
                        AlienPartGenerator apg = this.AlienProps.alienRace.generalSettings.alienPartGenerator;

                        this.colorChannels     = new Dictionary<string, ExposableValueTuple<Color, Color>>();
                        this.colorChannelLinks = new Dictionary<string, ColorChannelLinkData>();

                        this.colorChannels.Add(key: "base", new ExposableValueTuple<Color, Color>(Color.white, Color.white));

                        {
                            this.colorChannels.Add(key: "hair", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        }

                        {
                            this.colorChannels.Add("skin",     new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                            this.colorChannels.Add("skinBase", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        }


                        this.colorChannels.Add("tattoo",   new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        this.colorChannels.Add("favorite", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        this.colorChannels.Add("ideo", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        this.colorChannels.Add("mech", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));

                        foreach (ColorChannelGenerator channel in apg.colorChannels)
                        {
                            if (!this.colorChannels.ContainsKey(channel.name))
                                this.colorChannels.Add(channel.name, new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                            this.colorChannels[channel.name] = this.GenerateChannel(channel, this.colorChannels[channel.name]);
                        }

                        try
                        {
                            if (!this.AlienProps.alienRace.raceRestriction.blackEndoCategories.Contains(EndogeneCategory.Melanin))
                                if (this.Pawn.story.SkinColorBase != Color.clear)
                                    this.OverwriteColorChannel("skin", this.Pawn.story.SkinColorBase);
                        }
                        catch (InvalidOperationException)
                        {
                            // No skin color gene on this person for one reason or another, without blocking category
                        }

                        ExposableValueTuple<Color, Color> skinColors = this.colorChannels["skin"];
                        this.OverwriteColorChannel("skinBase", skinColors.first, skinColors.second);
                        this.Pawn.story.SkinColorBase = skinColors.first;


                        if (this.colorChannels["hair"].first == Color.clear)
                            this.OverwriteColorChannel("hair", this.Pawn.story.HairColor);

                        if (this.colorChannels[key: "tattoo"].first == Color.clear)
                        {
                            Color tattooColor = skinColors.first;
                            tattooColor.a *= 0.8f;

                            Color tattooColorSecond = skinColors.second;
                            tattooColorSecond.a *= 0.8f;

                            this.OverwriteColorChannel("tattoo", tattooColor, tattooColorSecond);
                        }

                        if (this.Pawn.Corpse?.GetRotStage() == RotStage.Rotting)
                            this.OverwriteColorChannel("skin", PawnRenderUtility.GetRottenColor(this.colorChannels["skin"].first));
                        this.Pawn.story.HairColor = this.colorChannels["hair"].first;

                        this.RegenerateColorChannelLink("skin");


                        if (this.AlienProps.alienRace.generalSettings.alienPartGenerator.oldHairColorGen != null)
                        {
                            if (Rand.Value < this.AlienProps.alienRace.generalSettings.alienPartGenerator.oldHairAgeCurve.Evaluate(this.Pawn.ageTracker.AgeBiologicalYearsFloat))
                            {
                                Color oldAgeColor = this.GenerateColor(this.AlienProps.alienRace.generalSettings.alienPartGenerator.oldHairColorGen);

                                this.OverwriteColorChannel("hair", oldAgeColor);
                                this.Pawn.story.HairColor = this.colorChannels["hair"].first;
                            }
                        }
                    }

                    return this.colorChannels;
                }
                set => this.colorChannels = value;
            }

            public ExposableValueTuple<Color, Color> GenerateChannel(ColorChannelGenerator channel, ExposableValueTuple<Color, Color> colors = null)
            {
                colors ??= new ExposableValueTuple<Color, Color>();

                ColorChannelGeneratorCategory categoryEntry = channel.entries.RandomElementByWeight(ccgc => ccgc.weight);

                if (categoryEntry.first != null)
                    colors.first = this.GenerateColor(channel, categoryEntry, true);
                if (categoryEntry.second != null)
                    colors.second = this.GenerateColor(channel, categoryEntry, false);

                return colors;
            }

            public Color GenerateColor(ColorChannelGenerator channel, ColorChannelGeneratorCategory category, bool first)
            {
                ColorGenerator gen = first ? category.first : category.second;

                switch (gen)
                {
                    case ColorGenerator_CustomAlienChannel ac:

                        ac.GetInfo(out string channelName, out bool firstColor);

                        if (!this.ColorChannelLinks.ContainsKey(channelName))
                            this.ColorChannelLinks.Add(channelName, new ColorChannelLinkData { originalChannel = channelName});

                        HashSet<ColorChannelLinkData.ColorChannelLinkTargetData> linkTargetData = this.ColorChannelLinks[channelName].GetTargetDataFor(first);
                        if (linkTargetData.All(ccltd => ccltd.targetChannel != channel.name))
                            linkTargetData.Add(new ColorChannelLinkData.ColorChannelLinkTargetData
                                                            {
                                                                targetChannel = channel.name, 
                                                                categoryIndex = channel.entries.IndexOf(category)
                                                            });
                        return firstColor ? this.ColorChannels[channelName].first : this.ColorChannels[channelName].second;
                    default:
                        return this.GenerateColor(gen);
                }
            }

            public Color GenerateColor(ColorGenerator gen) =>
                gen switch
                {
                    ColorGenerator_SkinColorMelanin cm => cm.naturalMelanin ? ((Pawn)this.parent).story.SkinColorBase : gen.NewRandomizedColor(),
                    ChannelColorGenerator_PawnBased pb => pb.NewRandomizedColor(this.Pawn),
                    _ => gen.NewRandomizedColor()
                };

            public override void PostSpawnSetup(bool respawningAfterLoad)
            {
                base.PostSpawnSetup(respawningAfterLoad);
                AlienPartGenerator apg = ((ThingDef_AlienRace)this.parent.def).alienRace.generalSettings.alienPartGenerator;
                this.customDrawSize             = apg.customDrawSize;
                this.customHeadDrawSize         = apg.customHeadDrawSize;
                this.customPortraitDrawSize     = apg.customPortraitDrawSize;
                this.customPortraitHeadDrawSize = apg.customPortraitHeadDrawSize;
                this.originalKindDef            = this.Pawn.kindDef;
            }

            private bool saveIsAfter1_4;
            public override void PostExposeData()
            {
                base.PostExposeData();

                if (Scribe.mode == LoadSaveMode.LoadingVars) 
                    this.saveIsAfter1_4 = (Scribe.loader.curXmlParent is { } parentNode && parentNode[ScribeNodeName] != null);

                // If we're saving or the node name is found, use the new comp tag name
                if ((Scribe.mode == LoadSaveMode.Saving || this.saveIsAfter1_4) && Scribe.EnterNode(ScribeNodeName))
                {
                    try
                    {
                        this.ExposeDataInternal();
                    }
                    finally
                    {
                        Scribe.ExitNode();
                    }
                }
                // Pull data from legacy fields in the root tag
                else
                {
                    this.ExposeDataInternal();

                    this.colorChannelLinks = new Dictionary<string, ColorChannelLinkData>();

                    foreach (ColorChannelGenerator ccg in this.AlienProps.alienRace.generalSettings.alienPartGenerator.colorChannels)
                    {
                        foreach (ColorChannelGeneratorCategory ccgc in ccg.entries)
                        {
                            if (ccgc.first is ColorGenerator_CustomAlienChannel) 
                                this.GenerateColor(ccg, ccgc, true);
                            if (ccgc.second is ColorGenerator_CustomAlienChannel)
                                this.GenerateColor(ccg, ccgc, false);
                        }
                    }
                }
            }

            private void ExposeDataInternal()
            {
                Scribe_Values.Look(ref this.fixGenderPostSpawn, label: "fixAlienGenderPostSpawn");
                Scribe_Collections.Look(ref this.addonVariants, label: "addonVariants");
                Scribe_Collections.Look(ref this.addonColors,   label: nameof(this.addonColors), LookMode.Deep);
                Scribe_Collections.Look(ref this.colorChannels, label: "colorChannels");
                Scribe_Collections.Look(ref this.colorChannelLinks, label: "colorChannelLinks", LookMode.Value, LookMode.Deep);

                Scribe_Values.Look(ref this.headVariant,     nameof(this.headVariant),     -1);
                Scribe_Values.Look(ref this.bodyVariant,     nameof(this.bodyVariant),     -1);
                Scribe_Values.Look(ref this.headMaskVariant, nameof(this.headMaskVariant), -1);
                Scribe_Values.Look(ref this.bodyMaskVariant, nameof(this.bodyMaskVariant), -1);

                if (Scribe.mode is LoadSaveMode.ResolvingCrossRefs && this.Pawn != null)
                    this.Pawn.story.SkinColorBase = this.GetChannel("skin").first;

                
                this.colorChannelLinks = this.ColorChannelLinks ?? [];
            }

            public ExposableValueTuple<Color, Color> GetChannel(string channel)
            {
                if (this.ColorChannels.TryGetValue(channel, out ExposableValueTuple<Color, Color> colorChannel))
                    return colorChannel;

                AlienPartGenerator apg = this.AlienProps.alienRace.generalSettings.alienPartGenerator;

                foreach (ColorChannelGenerator apgChannel in apg.colorChannels)
                    if (apgChannel.name == channel)
                    {
                        this.ColorChannels.Add(channel, this.GenerateChannel(apgChannel));

                        return this.ColorChannels[channel];
                    }

                return new ExposableValueTuple<Color, Color>(Color.white, Color.white);
            }

            public void RegenerateColorChannelLinks()
            {
                foreach (string key in this.ColorChannelLinks.Keys)
                    this.RegenerateColorChannelLink(key);
            }

            public void RegenerateColorChannelLink(string channel)
            {
                ThingDef_AlienRace alienProps = ((ThingDef_AlienRace)this.parent.def);
                AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                if (this.ColorChannelLinks.TryGetValue(channel, out ColorChannelLinkData colorChannelLink))
                {
                    foreach (ColorChannelLinkData.ColorChannelLinkTargetData targetData in colorChannelLink.targetsChannelOne)
                    {
                        ColorChannelGenerator apgChannel = apg.colorChannels.FirstOrDefault(ccg => ccg.name == targetData.targetChannel);

                        if (apgChannel != null)
                            this.ColorChannels[targetData.targetChannel].first = this.GenerateColor(apgChannel, apgChannel.entries[targetData.categoryIndex], true);
                    }

                    foreach (ColorChannelLinkData.ColorChannelLinkTargetData targetData in colorChannelLink.targetsChannelTwo)
                    {
                        ColorChannelGenerator apgChannel = apg.colorChannels.FirstOrDefault(ccg => ccg.name == targetData.targetChannel);

                        if (apgChannel != null)
                            this.ColorChannels[targetData.targetChannel].second = this.GenerateColor(apgChannel, apgChannel.entries[targetData.categoryIndex], false);
                    }
                }
            }

            public void OverwriteColorChannel(string channel, Color? first = null, Color? second = null)
            {
                if (!this.ColorChannels.ContainsKey(channel))
                    this.ColorChannels.Add(channel, new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));

                if (first.HasValue)
                    this.ColorChannels[channel].first = first.Value;
                if (second.HasValue)
                    this.ColorChannels[channel].second = second.Value;

                this.RegenerateColorChannelLink(channel);
            }

            public static void CopyAlienData(Pawn original, Pawn clone)
            {
                AlienComp originalComp = original.TryGetComp<AlienComp>();
                AlienComp cloneComp = clone.TryGetComp<AlienComp>();

                CopyAlienData(originalComp, cloneComp);

                clone.Drawer.renderer.SetAllGraphicsDirty();
            }

            public static void CopyAlienData(AlienComp originalComp, AlienComp cloneComp)
            {
                foreach ((string channel, ExposableValueTuple<Color, Color> colors) in originalComp.ColorChannels)
                    cloneComp.OverwriteColorChannel(channel, colors.first, colors.second);

                cloneComp.addonVariants     = originalComp.addonVariants.ListFullCopy();
                cloneComp.addonColors       = originalComp.addonColors.Select(vt => new ExposableValueTuple<Color?, Color?>(vt.first, vt.second)).ToList();
                cloneComp.colorChannelLinks = [];
                foreach ((string key, ColorChannelLinkData originalData) in originalComp.ColorChannelLinks)
                {
                    ColorChannelLinkData cloneData = new()
                                                     {
                                                         originalChannel   = originalData.originalChannel,
                                                         targetsChannelOne = [],
                                                         targetsChannelTwo = []
                                                     };
                    foreach (ColorChannelLinkData.ColorChannelLinkTargetData targetData in originalData.targetsChannelOne)
                        cloneData.targetsChannelOne.Add(new ColorChannelLinkData.ColorChannelLinkTargetData { categoryIndex = targetData.categoryIndex, targetChannel = targetData.targetChannel });

                    foreach (ColorChannelLinkData.ColorChannelLinkTargetData targetData in originalData.targetsChannelTwo)
                        cloneData.targetsChannelTwo.Add(new ColorChannelLinkData.ColorChannelLinkTargetData { categoryIndex = targetData.categoryIndex, targetChannel = targetData.targetChannel });

                    cloneComp.ColorChannelLinks.Add(key, cloneData);
                }

                cloneComp.bodyVariant               = originalComp.bodyVariant;
                cloneComp.bodyMaskVariant           = originalComp.bodyMaskVariant;
                cloneComp.headVariant               = originalComp.headVariant;
                cloneComp.headMaskVariant           = originalComp.headMaskVariant;
                cloneComp.lastAlienMeatIngestedTick = originalComp.lastAlienMeatIngestedTick;
            }


            private List<AlienPawnRenderNodeProperties_BodyAddon> nodeProps = null;
            
            public override List<PawnRenderNode> CompRenderNodes()
            {
                List<PawnRenderNode>                nodes     = [];
                List<AlienPawnRenderNodeProperties_BodyAddon> nodePropsTemp = this.nodeProps ?? [];

                AlienComp alienComp = this;

                alienComp.addonGraphics = [];
                alienComp.addonVariants ??= [];
                alienComp.addonColors ??= [];

                int sharedIndex = 0;

                using IEnumerator<BodyAddon> bodyAddons = this.AlienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator();
                int                          addonIndex = 0;
                
                while (bodyAddons.MoveNext())
                {
                    BodyAddon addon = bodyAddons.Current!;

                    if (this.nodeProps == null)
                    {
                        AlienPawnRenderNodeProperties_BodyAddon node = new()
                                                                       {
                                                                           addon        = addon,
                                                                           addonIndex   = addonIndex,
                                                                           parentTagDef = addon.alignWithHead ? PawnRenderNodeTagDefOf.Head : PawnRenderNodeTagDefOf.Body,
                                                                           pawnType     = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                                                                           workerClass  = typeof(AlienPawnRenderNodeWorker_BodyAddon),
                                                                           nodeClass    = typeof(AlienPawnRenderNode_BodyAddon),
                                                                           drawData = DrawData.NewWithData(new DrawData.RotationalData { rotationOffset = addon.angle },
                                                                                                           new DrawData.RotationalData
                                                                                                           { rotationOffset = -addon.angle, rotation = Rot4.East },
                                                                                                           new DrawData.RotationalData { rotationOffset = 0, rotation = Rot4.North }),
                                                                           useGraphic = true,
                                                                           alienComp  = alienComp,
                                                                           debugLabel = addon.Name
                                                                       };
                        this.RegenerateAddonGraphic(node, addonIndex, ref sharedIndex, true);
                        nodePropsTemp.Add(node);
                    }

                    if (addon.CanDrawAddonStatic(this.Pawn))
                    {
                        AlienPawnRenderNodeProperties_BodyAddon nodeProp = nodePropsTemp[addonIndex];

                        PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(nodeProp.nodeClass, this.Pawn, nodeProp, this.Pawn.Drawer.renderer.renderTree);
                        nodeProp.node = pawnRenderNode as AlienPawnRenderNode_BodyAddon;
                        this.RegenerateAddonGraphic(nodeProp, addonIndex, ref sharedIndex);
                        nodes.Add(pawnRenderNode);
                    }

                    addonIndex++;
                }

                this.nodeProps ??= nodePropsTemp;
                return nodes;
            }

            public static void RegenerateAddonsForced(Pawn pawn) =>
                pawn.GetComp<AlienComp>()?.RegenerateAddonsForced();

            public void RegenerateAddonsForced()
            {
                if (!this.Pawn.Drawer.renderer.renderTree.Resolved || !this.Pawn.Spawned || this.nodeProps == null)
                    return;

                using IEnumerator<BodyAddon> bodyAddons  = this.AlienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator();
                int                          sharedIndex = 0;

                for (int i = 0; i < this.nodeProps.Count; i++) 
                    this.RegenerateAddonGraphic(this.nodeProps[i], i, ref sharedIndex, true);
            }

            public static void RegenerateAddonGraphicsWithCondition(Pawn pawn, HashSet<Type> types) => 
                pawn.GetComp<AlienComp>()?.RegenerateAddonGraphicsWithCondition(types);

            public void RegenerateAddonGraphicsWithCondition(HashSet<Type> types)
            {
                if (!this.Pawn.Drawer.renderer.renderTree.Resolved)
                    return;

                void Update()
                {
                    using IEnumerator<BodyAddon> bodyAddons = this.AlienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons)
                                                                  .GetEnumerator();
                    int addonIndex  = 0;
                    int sharedIndex = 0;
                    while (bodyAddons.MoveNext())
                    {
                        BodyAddon addon = bodyAddons.Current!;

                        if (addon.conditionTypes.Intersect(types).Any()) this.RegenerateAddonGraphic(this.nodeProps[addonIndex], addonIndex, ref sharedIndex);
                        addonIndex++;
                    }
                    Application.onBeforeRender -= Update;
                }
                
                Application.onBeforeRender += Update;
            }

            private void RegenerateAddonGraphic(AlienPawnRenderNodeProperties_BodyAddon addonProps, int addonIndex, ref int sharedIndex, bool force = false)
            {
                bool colorInsertActive = false;

                if (this.addonColors.Count > addonIndex)
                {
                    ExposableValueTuple<Color?, Color?> addonColor = this.addonColors[addonIndex];
                    if (addonColor.first.HasValue)
                    {
                        addonProps.addon.colorOverrideOne = addonColor.first;
                        colorInsertActive                 = true;
                    }

                    if (addonColor.second.HasValue)
                    {
                        addonProps.addon.colorOverrideTwo = addonColor.second;
                        colorInsertActive                 = true;
                    }
                }

                Graphic g = addonProps.addon.GetGraphic(this.Pawn, this, ref sharedIndex, this.addonVariants.Count > addonIndex ? this.addonVariants[addonIndex] : null, !force, addonProps.graphic);

                if(g == null)
                {
                    if (colorInsertActive)
                    {
                        addonProps.addon.colorOverrideOne = null;
                        addonProps.addon.colorOverrideTwo = null;
                    }
                    return;
                }

                this.addonGraphics.Add(g);
                if (this.addonVariants.Count <= addonIndex) 
                    this.addonVariants.Add(sharedIndex);

                if (this.addonColors.Count <= addonIndex)
                {
                    this.addonColors.Add(new ExposableValueTuple<Color?, Color?>(null, null));
                }
                else if (colorInsertActive)
                {
                    addonProps.addon.colorOverrideOne = null;
                    addonProps.addon.colorOverrideTwo = null;
                }

                addonProps.graphic = g;
                addonProps.node?.UpdateGraphic();
                //addonProps.node.requestRecache = true;
            }

            public void UpdateColors()
            {
                if (!this.Pawn.Drawer.renderer.renderTree.Resolved && !this.Pawn.Spawned)
                    return;

                this.OverwriteColorChannel("hair",     this.Pawn.story.HairColor);
                this.OverwriteColorChannel("skin",     this.Pawn.story.SkinColor);
                this.OverwriteColorChannel("skinBase", this.Pawn.story.SkinColorBase);
                this.OverwriteColorChannel("favorite", this.Pawn.story.favoriteColor?.color);
                this.OverwriteColorChannel("favorite", second: this.ColorChannels["favorite"].second != Color.clear ? null : this.Pawn.story.favoriteColor?.color);
                this.OverwriteColorChannel("ideo",     this.Pawn.Ideo?.Color, this.Pawn.Ideo?.ApparelColor);
                this.OverwriteColorChannel("mech",     this.Pawn.Faction?.MechColor);

                if (this.Pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Rotting)
                    this.OverwriteColorChannel("skin", PawnRenderUtility.GetRottenColor(this.Pawn.story.SkinColor));
            }

            public override void Notify_DefsHotReloaded()
            {
                base.Notify_DefsHotReloaded();
                LongEventHandler.QueueLongEvent(() => ReresolveGraphic(this.Pawn), $"Regenerate {this.Pawn.NameFullColored}", false, null);
            }

            [DebugAction(category: "AlienRace", name: "Regenerate all colorchannels", allowedGameStates = AllowedGameStates.PlayingOnMap)]
            // ReSharper disable once UnusedMember.Local
            private static void RegenerateColorchannels()
            {
                foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawns)
                {
                    AlienComp comp = pawn.TryGetComp<AlienComp>();
                    if (comp != null)
                        comp.colorChannels = null;
                }
            }

            [DebugAction("AlienRace", name: "Reresolve graphics", actionType = DebugActionType.ToolMapForPawns)]
            // ReSharper disable once UnusedMember.Local
            private static void ReresolveGraphic(Pawn p)
            {
                if (p != null)
                {
                    FleckMaker.ThrowSmoke(p.Position.ToVector3(), p.Map, 5f);
                    p.Drawer?.renderer?.SetAllGraphicsDirty();
                }
            }
        }

        public class ExposableValueTuple<TK, TV> : IExposable, IEquatable<ExposableValueTuple<TK, TV>>, ICloneable
        {
            public TK first;
            public TV second;

            public ExposableValueTuple()
            {
            }

            public ExposableValueTuple(TK first, TV second)
            {
                this.first  = first;
                this.second = second;
            }

            public bool Equals(ExposableValueTuple<TK, TV> other) =>
                other != null && this.first.Equals(other.first) && this.second.Equals(other.second);

            // ReSharper disable twice NonReadonlyMemberInGetHashCode
            public override int GetHashCode() =>
                this.first.GetHashCode() + this.second.GetHashCode();

            public object Clone() =>
                new ExposableValueTuple<TK, TV>(this.first, this.second);

            public void ExposeData()
            {
                if (typeof(TK).GetInterface(nameof(IExposable)) != null)
                    Scribe_Deep.Look(ref this.first, label: nameof(this.first));
                else
                    Scribe_Values.Look(ref this.first, label: "first");
                Scribe_Values.Look(ref this.second, label: "second");
            }
        }
    }
}