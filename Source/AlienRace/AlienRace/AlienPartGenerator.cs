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
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Gender = Verse.Gender;

    public partial class AlienPartGenerator
    {
        public List<HeadTypeDef> headTypes;
        public List<HeadTypeDef> HeadTypes => 
            this.headTypes ?? CachedData.DefaultHeadTypeDefs;

        public List<BodyTypeDef> bodyTypes = new List<BodyTypeDef>();

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

        public List<BodyAddon> bodyAddons = new List<BodyAddon>();

        public ThingDef_AlienRace alienProps;
        /*
        public static string GetAlienHead(string userpath, Gender gender, HeadTypeDef headType)
        {
            string path         = userpath;
            string headTypePath = Path.GetFileName(headType.graphicPath);

            if (gender == Gender.None && headType.gender != Gender.None)
                headTypePath = headTypePath.Substring(headTypePath.IndexOf('_')+1);
            else
                path = userpath + (userpath == GraphicPaths.VANILLA_HEAD_PATH ? gender + "/" : "");

            return userpath.NullOrEmpty() ? string.Empty : path + headTypePath;
        }*/
        /*
        public Graphic GetNakedGraphic(BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender, string maskPath) =>
            GraphicDatabase.Get(typeof(Graphic_Multi), GetNakedPath(bodyType, userpath, this.useGenderedBodies ? gender : ""), shader, Vector2.one, skinColor, skinColorSecond, data: null, shaderParameters: null, maskPath: maskPath);

        public static string GetNakedPath(BodyTypeDef bodyType, string userpath, string gender) => userpath + (!gender.NullOrEmpty() ? gender + "_" : "") + "Naked_" + (bodyType == BodyTypeDefOf.Baby ? BodyTypeDefOf.Child : bodyType);
        */
        public Color SkinColor(Pawn alien, bool first = true)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();

            if (alien.story.SkinColorOverriden)
                return alien.story.skinColorOverride!.Value;

            if (alienComp == null) 
                return CachedData.skinColorBase(alien.story) ?? Color.clear;

            ExposableValueTuple<Color, Color> skinColors = alienComp.GetChannel(channel: "skin");
            return first ? skinColors.first : skinColors.second;
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
            this.GenerateOffsetDefaults();

            {
                if (!this.alienProps.alienRace.graphicPaths.head.GetSubGraphics().MoveNext())
                {

                    ExtendedGraphicTop headGraphic = this.alienProps.alienRace.graphicPaths.head;
                    string             headPath    = headGraphic.path;

                    this.alienProps.alienRace.graphicPaths.head.headtypeGraphics = new List<ExtendedHeadtypeGraphic>();

                    foreach (HeadTypeDef headType in this.HeadTypes.Concat(DefDatabase<HeadTypeDef>.AllDefs.Where(htd => !htd.requiredGenes.NullOrEmpty())))
                    {
                        string headTypePath = Path.GetFileName(headType.graphicPath);

                        int  ind            = headTypePath.IndexOf('_');
                        bool genderIncluded = headType.gender != Gender.None && ind >= 0 && Enum.TryParse(headTypePath.Substring(0, ind), out Gender _);
                        headTypePath = genderIncluded ? headTypePath.Substring(ind + 1) : headTypePath;

                        ExtendedHeadtypeGraphic headtypeGraphic = new()
                                                                  {
                                                                      headType = headType,
                                                                      paths = new List<string>
                                                                              {
                                                                                  headPath.NullOrEmpty() ? string.Empty : headPath + headTypePath
                                                                              },
                                                                      genderGraphics = new List<ExtendedGenderGraphic>()
                                                                  };

                        if (!headType.requiredGenes.NullOrEmpty())
                            headtypeGraphic.pathsFallback.Add(headType.graphicPath);

                        Gender firstGender = genderIncluded ? headType.gender : Gender.Male;

                        headtypeGraphic.genderGraphics.Add(new ExtendedGenderGraphic
                                                           {
                                                               gender = firstGender,
                                                               path   = headPath + firstGender + "_" + headTypePath
                                                           });
                        if (!genderIncluded)
                            headtypeGraphic.genderGraphics.Add(new ExtendedGenderGraphic
                                                               {
                                                                   gender = Gender.Female,
                                                                   path   = headPath + Gender.Female + headTypePath
                                                               });

                        headGraphic.headtypeGraphics.Add(headtypeGraphic);
                    }
                }

                //Log.Message(string.Join("\n", this.alienProps.alienRace.graphicPaths.head.headtypeGraphics.Select(ehg => $"{ehg.headType.defName}: {ehg.path} | {string.Join("|", ehg.genderGraphics?.Select(egg => $"{egg.gender}: {egg.path}") ?? new []{string.Empty})}")));

                if (!this.alienProps.alienRace.graphicPaths.body.GetSubGraphics().MoveNext())
                {
                    ExtendedGraphicTop bodyGraphic = this.alienProps.alienRace.graphicPaths.body;
                    string             bodyPath    = bodyGraphic.path;

                    bodyGraphic.bodytypeGraphics = new List<ExtendedBodytypeGraphic>();

                    foreach (BodyTypeDef bodyTypeRaw in this.bodyTypes)
                    {
                        BodyTypeDef bodyType = bodyTypeRaw == BodyTypeDefOf.Baby ? BodyTypeDefOf.Child : bodyTypeRaw;

                        bodyGraphic.bodytypeGraphics.Add(new ExtendedBodytypeGraphic
                                                         {
                                                             bodytype = bodyTypeRaw,
                                                             path     = $"{bodyPath}Naked_{bodyType.defName}",
                                                             genderGraphics = new List<ExtendedGenderGraphic>()
                                                                              {
                                                                                  new()
                                                                                  {
                                                                                      gender = Gender.Male,
                                                                                      path   = $"{bodyPath}{Gender.Male}_Naked_{bodyType.defName}"
                                                                                  },
                                                                                  new()
                                                                                  {
                                                                                      gender = Gender.Female,
                                                                                      path   = $"{bodyPath}{Gender.Female}_Naked_{bodyType.defName}"
                                                                                  }
                                                                              }
                                                         });
                    }
                }
            }

            {
                
                foreach (ExtendedGraphicTop graphicTop in this.alienProps.alienRace.graphicPaths.apparel.individualPaths.Values)
                {
                    if (!graphicTop.GetSubGraphics().MoveNext())
                    {
                        string path = graphicTop.path;

                        graphicTop.bodytypeGraphics = new List<ExtendedBodytypeGraphic>();
                        foreach (BodyTypeDef bodyType in this.bodyTypes)
                            graphicTop.bodytypeGraphics.Add(new ExtendedBodytypeGraphic
                                                            {
                                                                bodytype = bodyType,
                                                                path     = $"{path}_{bodyType.defName}",
                                                                genderGraphics = new List<ExtendedGenderGraphic>
                                                                                 {
                                                                                     new()
                                                                                     {
                                                                                         gender = Gender.Male,
                                                                                         path   = $"{path}_{Gender.Male}_{bodyType.defName}"
                                                                                     },
                                                                                     new()
                                                                                     {
                                                                                         gender = Gender.Female,
                                                                                         path   = $"{path}_{Gender.Female}_{bodyType.defName}"
                                                                                     }
                                                                                 }
                                                            });
                    }
                }

                foreach (ApparelFallbackOption fallback in this.alienProps.alienRace.graphicPaths.apparel.fallbacks)
                {
                    foreach (ExtendedGraphicTop graphicTop in fallback.wornGraphicPaths.Concat(fallback.wornGraphicPath))
                    {

                        if (!graphicTop.GetSubGraphics().MoveNext())
                        {
                            string path = graphicTop.path;
                            graphicTop.bodytypeGraphics = new List<ExtendedBodytypeGraphic>();
                            foreach (BodyTypeDef bodyType in this.bodyTypes)
                                graphicTop.bodytypeGraphics.Add(new ExtendedBodytypeGraphic
                                                                {
                                                                    bodytype = bodyType,
                                                                    path     = $"{path}_{bodyType.defName}",
                                                                    genderGraphics = new List<ExtendedGenderGraphic>()
                                                                                     {
                                                                                         new()
                                                                                         {
                                                                                             gender = Gender.Male,
                                                                                             path   = $"{path}_{Gender.Male}_{bodyType.defName}"
                                                                                         },
                                                                                         new()
                                                                                         {
                                                                                             gender = Gender.Female,
                                                                                             path   = $"{path}_{Gender.Female}_{bodyType.defName}"
                                                                                         }
                                                                                     }
                                                                });
                        }
                    }
                }
            }

            this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.Init();
            if (!this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.GetPath().NullOrEmpty())
                this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.IncrementVariantCount();
            Stack<IEnumerator<IExtendedGraphic>> stack = new();
            
            stack.Push(this.alienProps.alienRace.graphicPaths.apparel.pathPrefix.GetSubGraphics());
            while (stack.Count > 0)
            {
                IEnumerator<IExtendedGraphic> currentGraphicSet = stack.Pop();

                while (currentGraphicSet.MoveNext())
                {
                    IExtendedGraphic current = currentGraphicSet.Current;
                    if (current != null)
                    {
                        current.Init();
                        if(!current.GetPath().NullOrEmpty())
                            current.IncrementVariantCount();
                        
                        stack.Push(current.GetSubGraphics());
                    }
                }
            }

            graphicsLoader.LoadAllGraphics(this.alienProps.defName, 
                                           this.alienProps.alienRace.graphicPaths.head,
                                           this.alienProps.alienRace.graphicPaths.body,
                                           this.alienProps.alienRace.graphicPaths.skeleton,
                                           this.alienProps.alienRace.graphicPaths.skull,
                                           this.alienProps.alienRace.graphicPaths.stump,
                                           this.alienProps.alienRace.graphicPaths.bodyMasks,
                                           this.alienProps.alienRace.graphicPaths.headMasks);
            
            graphicsLoader.LoadAllGraphics(this.alienProps.defName, 
                                           this.alienProps.alienRace.graphicPaths.apparel.individualPaths.Values.Concat(
                                                                                                                                this.alienProps.alienRace.graphicPaths.apparel.fallbacks.SelectMany(afo => 
                                                                                                                                    afo.wornGraphicPaths.Concat(afo.wornGraphicPath)) ).ToArray());
            
            graphicsLoader.LoadAllGraphics(this.alienProps.defName + " Addons", this.bodyAddons.Cast<ExtendedGraphicTop>().ToArray());

            foreach (BodyAddon bodyAddon in this.bodyAddons)
                // Initialise the offsets of each addon with the generic default offsets
                bodyAddon.defaultOffsets = this.offsetDefaults.Find(on => on.name == bodyAddon.defaultOffset).offsets;
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
            public string                              name = "";
            public List<ColorChannelGeneratorCategory> entries = new List<ColorChannelGeneratorCategory>();


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
            public  bool          fixGenderPostSpawn;
            public  Vector2       customDrawSize             = Vector2.one;
            public  Vector2       customHeadDrawSize         = Vector2.one;
            public  Vector2       customPortraitDrawSize     = Vector2.one;
            public  Vector2       customPortraitHeadDrawSize = Vector2.one;

            public  int           bodyVariant                = -1;
            public  int           headVariant                = -1;
            public  int           headMaskVariant            = -1;
            public  int           bodyMaskVariant            = -1;

            public  List<Graphic> addonGraphics;
            public  List<int>     addonVariants;
        

            public int lastAlienMeatIngestedTick = 0;

            private Dictionary<string, ExposableValueTuple<Color, Color>> colorChannels;
            private Dictionary<string, HashSet<ExposableValueTuple<ExposableValueTuple<string, int>, bool>>>   colorChannelLinks = new Dictionary<string, HashSet<ExposableValueTuple<ExposableValueTuple<string, int>, bool>>>();

            public Dictionary<string, ExposableValueTuple<Color, Color>> ColorChannels
            {
                get
                {
                    if (this.colorChannels == null || !this.colorChannels.Any())
                    {
                        Pawn               pawn       = (Pawn)this.parent;
                        ThingDef_AlienRace alienProps = ((ThingDef_AlienRace)this.parent.def);
                        AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                        this.colorChannels     = new Dictionary<string, ExposableValueTuple<Color, Color>>();
                        this.colorChannelLinks = new Dictionary<string, HashSet<ExposableValueTuple<ExposableValueTuple<string, int>, bool>>>();

                        this.colorChannels.Add(key: "base", new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                        this.colorChannels.Add(key: "hair", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        
                        Color skinColor;
                        try
                        {
                            skinColor = alienProps.alienRace.raceRestriction.blackEndoCategories.Contains(EndogeneCategory.Melanin) ?
                                            PawnSkinColors.RandomSkinColorGene(pawn).skinColorBase!.Value :
                                            pawn.story.SkinColorBase;
                        }
                        catch (InvalidOperationException)
                        {
                            skinColor = PawnSkinColors.RandomSkinColorGene(pawn).skinColorBase!.Value;
                        }

                        this.colorChannels.Add(key: "skin", new ExposableValueTuple<Color, Color>(skinColor, skinColor));

                        this.colorChannels.Add(key: "tattoo", new ExposableValueTuple<Color, Color>(Color.clear, Color.clear));
                        
                        foreach (ColorChannelGenerator channel in apg.colorChannels)
                        {
                            if (!this.colorChannels.ContainsKey(channel.name))
                                this.colorChannels.Add(channel.name, new ExposableValueTuple<Color, Color>(Color.white, Color.white));
                            this.colorChannels[channel.name] = this.GenerateChannel(channel, this.colorChannels[channel.name]);
                        }

                        pawn.story.SkinColorBase = this.colorChannels["skin"].first;

                        ExposableValueTuple<Color, Color> hairColors = this.colorChannels[key: "hair"];

                        if (hairColors.first == Color.clear)
                        {
                            Color color = PawnHairColors.RandomHairColor(pawn, pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                            hairColors.first  = color;
                            hairColors.second = color;
                        }

                        ExposableValueTuple<Color, Color> tattooColors = this.colorChannels[key: "tattoo"];

                        if (tattooColors.first == Color.clear)
                        {
                            Color tattooColor = skinColor;
                            tattooColor.a *= 0.8f;

                            tattooColors.first = tattooColors.second = tattooColor;
                        }


                        if (pawn.Corpse?.GetRotStage() == RotStage.Rotting)
                            this.colorChannels["skin"].first = PawnGraphicSet.RottingColorDefault;
                        pawn.story.HairColor = hairColors.first;

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
                                hairColors.first                 = new Color(grey, grey, grey);
                                pawn.story.HairColor = hairColors.first;
                            }
                        }
                    }

                    return this.colorChannels;
                }
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
                        string[] split = ac.colorChannel.Split('_');
                        if (!this.colorChannelLinks.ContainsKey(split[0]))
                            this.colorChannelLinks.Add(split[0], new HashSet<ExposableValueTuple<ExposableValueTuple<string, int>, bool >>());
                        if (this.colorChannelLinks[split[0]].All(evt => evt.first.first != channel.name))
                            this.colorChannelLinks[split[0]].Add(new ExposableValueTuple<ExposableValueTuple<string, int>, bool>(new ExposableValueTuple<string, int>(channel.name, channel.entries.IndexOf(category)), first));
                        return split[1] == "1" ? this.ColorChannels[split[0]].first : this.ColorChannels[split[0]].second;
                    case ColorGenerator_SkinColorMelanin cm:
                        return cm.naturalMelanin ? 
                                   ((Pawn) this.parent).story.SkinColorBase : 
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
                Scribe_Collections.Look(ref this.addonVariants, label: "addonVariants");
                Scribe_Collections.Look(ref this.colorChannels, label: "colorChannels");
                Scribe_NestedCollections.Look(ref this.colorChannelLinks, label: "colorChannelLinks", LookMode.Undefined, LookMode.Deep);

                Scribe_Values.Look(ref this.headVariant, nameof(this.headVariant), -1);
                Scribe_Values.Look(ref this.bodyVariant, nameof(this.bodyVariant), -1);
                Scribe_Values.Look(ref this.headMaskVariant, nameof(this.headMaskVariant), -1);
                Scribe_Values.Look(ref this.bodyMaskVariant, nameof(this.bodyMaskVariant), -1);

                Pawn   pawn          = (Pawn)this.parent;
                if (Scribe.mode is LoadSaveMode.ResolvingCrossRefs or LoadSaveMode.Saving)
                {
                    Color? skinColorBase = CachedData.skinColorBase(pawn.story);
                    Scribe_Values.Look(ref skinColorBase, nameof(skinColorBase));
                    pawn.story.SkinColorBase = skinColorBase ?? this.GetChannel("skin").first;
                }

                this.colorChannelLinks ??= new Dictionary<string, HashSet<ExposableValueTuple<ExposableValueTuple<string, int>, bool>>>();
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
                        this.ColorChannels.Add(channel, this.GenerateChannel(apgChannel));

                        return this.ColorChannels[channel];
                    }

                return new ExposableValueTuple<Color, Color>(Color.white, Color.white);
            }

            public void RegenerateColorChannelLinks()
            {
                foreach (KeyValuePair<string, HashSet<ExposableValueTuple<ExposableValueTuple<string, int>, bool>>> kvp in this.colorChannelLinks) 
                    this.RegenerateColorChannelLink(kvp.Key);
            }

            public void RegenerateColorChannelLink(string channel)
            {
                ThingDef_AlienRace alienProps = ((ThingDef_AlienRace)this.parent.def);
                AlienPartGenerator apg        = alienProps.alienRace.generalSettings.alienPartGenerator;

                if (this.colorChannelLinks.ContainsKey(channel))
                    foreach (ExposableValueTuple<ExposableValueTuple<string, int>, bool> link in this.colorChannelLinks[channel])
                    {
                        foreach (ColorChannelGenerator apgChannel in apg.colorChannels)
                        {
                            if (apgChannel.name == link.first.first)
                            {
                                if (link.second)
                                    this.ColorChannels[link.first.first].first = this.GenerateColor(apgChannel, apgChannel.entries[link.first.second], true);
                                else
                                    this.ColorChannels[link.first.first].second = this.GenerateColor(apgChannel, apgChannel.entries[link.first.second], false);
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
                p.Drawer?.renderer?.graphics?.SetAllGraphicsDirty();
            }
        }

        public class ExposableValueTuple<TK, TV> : IExposable, IEquatable<ExposableValueTuple<TK,TV>>
        {
            public TK first;
            public TV second;

            public ExposableValueTuple()
            {
            }

            public ExposableValueTuple(TK first, TV second)
            {
                this.first = first;
                this.second = second;
            }

            public bool Equals(ExposableValueTuple<TK, TV> other) => 
                other != null && this.first.Equals(other.first) && this.second.Equals(other.second);

            // ReSharper disable twice NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => this.first.GetHashCode() + this.second.GetHashCode();

            public void ExposeData()
            {
                if(typeof(TK).GetInterface(nameof(IExposable)) != null)
                    Scribe_Deep.Look(ref this.first, label: nameof(this.first));
                else
                    Scribe_Values.Look(ref this.first, label: "first");
                Scribe_Values.Look(ref this.second, label: "second");
            }
        }
    }
}