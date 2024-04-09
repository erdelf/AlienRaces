namespace AlienRace
{
    using HarmonyLib;
    using RimWorld;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using Verse;
    using System.Xml;
    using ExtendedGraphics;
    using JetBrains.Annotations;

    public class ThingDef_AlienRace : ThingDef
    {
        public AlienSettings alienRace;

        public override void ResolveReferences()
        {
            this.comps.Add(new CompProperties(typeof(AlienPartGenerator.AlienComp)));
            base.ResolveReferences();

            if (this.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize.Equals(Vector2.zero))
                this.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customDrawSize;
            if (this.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize.Equals(Vector2.zero))
                this.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize;

            this.alienRace.generalSettings.alienPartGenerator.headFemaleOffsetDirectional ??= this.alienRace.generalSettings.alienPartGenerator.headOffsetDirectional;

            this.alienRace.generalSettings.alienPartGenerator.alienProps = this;

            foreach (Type type in typeof(StyleItemDef).AllSubclassesNonAbstract())
                if(!this.alienRace.styleSettings.ContainsKey(type))
                    this.alienRace.styleSettings.Add(type, new StyleSettings());

            foreach (AlienPartGenerator.BodyAddon bodyAddon in this.alienRace.generalSettings.alienPartGenerator.bodyAddons)
                bodyAddon.offsets.west ??= bodyAddon.offsets.east;

            if (this.alienRace.generalSettings.minAgeForAdulthood < 0)
                this.alienRace.generalSettings.minAgeForAdulthood = (float) AccessTools.Field(typeof(PawnBioAndNameGenerator), name: "MinAgeForAdulthood").GetValue(obj: null);

            foreach (StatPartAgeOverride spao in this.alienRace.generalSettings.ageStatOverrides)
                this.alienRace.generalSettings.ageStatOverride[spao.stat] = spao.overridePart;

            for (int i = 0; i < this.race.lifeStageAges.Count; i++)
            {
                LifeStageAge lsa = this.race.lifeStageAges[i];

                if (lsa is not LifeStageAgeAlien lsaa)
                {
                    lsaa = new LifeStageAgeAlien
                           {
                               def           = lsa.def,
                               minAge        = lsa.minAge,
                               soundAmbience = lsa.soundAmbience,
                               soundAngry    = lsa.soundAngry,
                               soundCall     = lsa.soundCall,
                               soundDeath    = lsa.soundDeath,
                               soundWounded  = lsa.soundWounded
                           };

                    this.race.lifeStageAges[i] = lsaa;
                }

                if (lsaa.customDrawSize.Equals(Vector2.zero))
                    lsaa.customDrawSize = this.alienRace.generalSettings.alienPartGenerator.customDrawSize;

                if (lsaa.customPortraitDrawSize.Equals(Vector2.zero))
                    lsaa.customPortraitDrawSize = this.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize;

                if (lsaa.customHeadDrawSize.Equals(Vector2.zero))
                    lsaa.customHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize;

                if (lsaa.customPortraitHeadDrawSize.Equals(Vector2.zero))
                    lsaa.customPortraitHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize;

                if (lsaa.customFemaleDrawSize.Equals(Vector2.zero))
                    lsaa.customFemaleDrawSize = this.alienRace.generalSettings.alienPartGenerator.customFemaleDrawSize;

                if (lsaa.customFemalePortraitDrawSize.Equals(Vector2.zero))
                    lsaa.customFemalePortraitDrawSize = this.alienRace.generalSettings.alienPartGenerator.customFemalePortraitDrawSize;

                if (lsaa.customFemaleHeadDrawSize.Equals(Vector2.zero))
                    lsaa.customFemaleHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customFemaleHeadDrawSize;

                if (lsaa.customFemalePortraitHeadDrawSize.Equals(Vector2.zero))
                    lsaa.customFemalePortraitHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customFemalePortraitHeadDrawSize;

                if (lsaa.headOffset.Equals(Vector2.zero))
                    lsaa.headOffset = this.alienRace.generalSettings.alienPartGenerator.headOffset;

                if (lsaa.headFemaleOffset.Equals(Vector2.negativeInfinity))
                    lsaa.headFemaleOffset = this.alienRace.generalSettings.alienPartGenerator.headFemaleOffset;

                lsaa.headOffsetDirectional ??= this.alienRace.generalSettings.alienPartGenerator.headOffsetDirectional;
                lsaa.headOffsetDirectional.west ??= lsaa.headOffsetDirectional.east;

                lsaa.headFemaleOffsetDirectional ??= this.alienRace.generalSettings.alienPartGenerator.headFemaleOffsetDirectional;
                lsaa.headFemaleOffsetDirectional.west ??= lsaa.headFemaleOffsetDirectional.east;
            }

            //if (this.alienRace.graphicPaths.body.path == GraphicPaths.VANILLA_BODY_PATH && !this.alienRace.graphicPaths.body.GetSubGraphics().MoveNext())
                //this.alienRace.graphicPaths.body.debug = false;

            if (this.alienRace.graphicPaths.head.path == GraphicPaths.VANILLA_HEAD_PATH && !this.alienRace.graphicPaths.head.GetSubGraphics().Any())
            {
                foreach (HeadTypeDef headType in DefDatabase<HeadTypeDef>.AllDefs)
                {
                    AlienPartGenerator.ExtendedConditionGraphic headtypeGraphic = new()
                                                                                 {
                                                                                     conditions = [new ConditionHeadType {headType = headType}],
                                                                                     path = headType.graphicPath
                                                                                 };

                    this.alienRace.graphicPaths.head.extendedGraphics.Add(headtypeGraphic);
                    //this.alienRace.graphicPaths.head.debug = false;
                }
            }

            if (this.alienRace.graphicPaths.skeleton.path == GraphicPaths.VANILLA_SKELETON_PATH && !this.alienRace.graphicPaths.skeleton.GetSubGraphics().Any())
            {
                this.alienRace.graphicPaths.skeleton.path             = string.Empty;

                foreach (BodyTypeDef bodyType in this.alienRace.generalSettings.alienPartGenerator.bodyTypes)
                    this.alienRace.graphicPaths.skeleton.extendedGraphics.Add(new AlienPartGenerator.ExtendedConditionGraphic()
                                                                              {
                                                                                  conditions = [new ConditionBodyType { bodyType = bodyType }],
                                                                                  path       = bodyType.bodyDessicatedGraphicPath
                                                                              });
            }
            
            void RecursiveAttributeCheck(Type type, Traverse instance)
            {
                if (type == typeof(ThingDef_AlienRace))
                    return;

                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    Traverse instanceNew = instance.Field(field.Name);

                    if (typeof(IList).IsAssignableFrom(field.FieldType))
                    {
                        object value = instanceNew.GetValue();
                        if (value != null)
                            foreach (object o in (IList) value)
                            {
                                if (o.GetType().Assembly == typeof(ThingDef_AlienRace).Assembly)
                                    RecursiveAttributeCheck(o.GetType(), Traverse.Create(o));
                            }
                    }

                    if (field.FieldType.Assembly == typeof(ThingDef_AlienRace).Assembly)
                        RecursiveAttributeCheck(field.FieldType, instanceNew);

                    LoadDefFromField attribute = field.GetCustomAttribute<LoadDefFromField>();
                    if (attribute != null)
                        if (instanceNew.GetValue() == null)
                            instanceNew.SetValue(attribute.defName == "this" ? this : attribute.GetDef(field.FieldType));
                }
            }
            RecursiveAttributeCheck(typeof(AlienSettings), Traverse.Create(this.alienRace));
        }

        public class AlienSettings
        {
            public GeneralSettings                 generalSettings  = new();
            public GraphicPaths                    graphicPaths     = new();
            public Dictionary<Type, StyleSettings> styleSettings    = new();
            public ThoughtSettings                 thoughtSettings  = new();
            public RelationSettings                relationSettings = new();
            public RaceRestrictionSettings         raceRestriction  = new();
            public CompatibilityInfo               compatibility    = new();
        }
    }

    public class GeneralSettings
    {
        public float maleGenderProbability = 0.5f;
        public bool immuneToAge = false;
        public bool canLayDown = true;
        public float minAgeForAdulthood = -1f;

        public List<ThingDef>         validBeds;
        public List<ChemicalSettings> chemicalSettings;
        public List<AlienChanceEntry<TraitWithDegree>>   forcedRaceTraitEntries;
        public List<AlienChanceEntry<TraitWithDegree>>   disallowedTraits;

        public IntRange                           additionalTraits   = IntRange.zero;
        public AlienPartGenerator                 alienPartGenerator = new();
        public List<SkillGain>                    passions           = new();
        public List<AlienChanceEntry<AbilityDef>> abilities          = new();

        public List<FactionRelationSettings> factionRelations;
        public int                           maxDamageForSocialfight = int.MaxValue;
        public bool                          allowHumanBios          = false;
        public bool                          immuneToXenophobia      = false;
        public List<ThingDef>                notXenophobistTowards   = new();
        public bool                          humanRecipeImport       = false;

        [LoadDefFromField(nameof(AlienDefOf.HAR_AlienCorpseCategory))]
        public ThingCategoryDef corpseCategory;

        public SimpleCurve lovinIntervalHoursFromAge;
        public List<int>   growthAges = new() { 7, 10, 13 };
        public int[]       GrowthAges => this.growthAges?.ToArray();

        public SimpleCurve growthFactorByAge;

        public List<BackstoryCategoryFilter> childBackstoryFilter;
        public List<BackstoryCategoryFilter> adultBackstoryFilter;
        public List<BackstoryCategoryFilter> adultVatBackstoryFilter;
        public List<BackstoryCategoryFilter> newbornBackstoryFilter;

        public ReproductionSettings reproduction = new();

        public List<AlienChanceEntry<GeneDef>> raceGenes = new();

        internal List<StatPartAgeOverride> ageStatOverrides = [];

        [Unsaved]
        public Dictionary<StatDef, StatPart_Age> ageStatOverride = [];
    }

    public class ReproductionSettings
    {
        public PawnKindDef childKindDef;

        public SimpleCurve maleFertilityAgeFactor = new(new[]
                                                        {
                                                            new CurvePoint(14, 0),
                                                            new CurvePoint(18, 1),
                                                            new CurvePoint(50, 1),
                                                            new CurvePoint(90, 0)
                                                        });
        public SimpleCurve femaleFertilityAgeFactor = new(new[]
                                                          {
                                                              new CurvePoint(14, 0),
                                                              new CurvePoint(20, 1),
                                                              new CurvePoint(28, 1),
                                                              new CurvePoint(35, 0.5f),
                                                              new CurvePoint(40, 0.1f),
                                                              new CurvePoint(45, 0.02f),
                                                              new CurvePoint(50, 0),
                                                          });

        public List<HybridSpecificSettings> hybridSpecific = new();

        public GenderPossibility fertilizingGender   = GenderPossibility.Male;
        public GenderPossibility gestatingGender = GenderPossibility.Female;

        public static bool ApplicableGender(Pawn pawn, bool gestating)
        {
            ReproductionSettings reproduction = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.reproduction ?? new ReproductionSettings();
            return ApplicableGender(pawn.gender, reproduction, gestating);
        }

        public static bool ApplicableGender(Gender gender, ReproductionSettings reproduction, bool gestating) =>
            gestating switch
            {
                true when reproduction.gestatingGender.IsGenderApplicable(gender) => true,
                false when reproduction.fertilizingGender.IsGenderApplicable(gender) => true,
                _ => false
            };

        public static bool GenderReproductionCheck(Pawn pawn, Pawn partnerPawn)
        {
            ReproductionSettings pawnReproduction    = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.reproduction        ?? new ReproductionSettings();
            ReproductionSettings partnerReproduction = (partnerPawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.reproduction ?? new ReproductionSettings();

            return (ApplicableGender(pawn.gender, pawnReproduction, false) &&
                    ApplicableGender(partnerPawn.gender, partnerReproduction, true)) ||
                   (ApplicableGender(pawn.gender,        pawnReproduction,    true) &&
                    ApplicableGender(partnerPawn.gender, partnerReproduction, false));
        }
    }

    public class HybridSpecificSettings
    {
        public ThingDef    partnerRace;
        public float       probability = 100;
        public PawnKindDef childKindDef;
    }

    public class FactionRelationSettings
    {
        public List<FactionDef> factions;
        public IntRange goodwill;
    }

    public class ChemicalSettings
    {
        public ChemicalDef chemical;
        public bool ingestible = true;
        public List<IngestionOutcomeDoer> reactions;
    }

    public class AlienChanceEntry<T>
    {
        [LoadAlias("defName")]
        public T entry;
        public List<AlienChanceEntry<T>> options = [];
        public int count = 1;
        public float chance = 100;

        public float commonalityMale = -1f;
        public float commonalityFemale = -1f;

        [Unsaved]
        private readonly List<AlienChanceEntry<T>> shuffledOptions = [];

        public bool Approved() =>
            Rand.Range(0, 100) < this.chance;

        public bool Approved(Gender gender) =>
            (gender == Gender.Male   && (this.commonalityMale   < 0 || Rand.Range(0, 100) < this.commonalityMale)   ||
             gender == Gender.Female && (this.commonalityFemale < 0 || Rand.Range(0, 100) < this.commonalityFemale) ||
             gender == Gender.None) && 
            this.Approved();

        public bool Approved(Pawn pawn) =>
            this.Approved(pawn.gender);

        public IEnumerable<T> Select(Pawn pawn)
        {
            if (pawn != null)
            {
                if (!this.Approved(pawn))
                    yield break;
            } else if (!this.Approved())
            {
                yield break;
            }

            if (!Equals(this.entry, default(T))) 
                yield return this.entry;

            // Doing this instead of GenCollection.TakeRandom because TakeRandom allows repeats
            if (this.shuffledOptions.Count != this.options.Count)
            {
                this.shuffledOptions.Clear();
                this.shuffledOptions.AddRange(this.options);
            }

            this.shuffledOptions.Shuffle();
            int limit = Math.Min(this.shuffledOptions.Count, this.count);
            for (int i = 0; i < limit; i ++)
            {
                foreach (T entryInner in this.shuffledOptions[i].Select(pawn))
                    yield return entryInner;
            }
        }

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.Text)
            {
                if(typeof(T).IsSubclassOf(typeof(Def)))
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.entry), xmlRoot.FirstChild.Value);
                else
                    Utilities.SetFieldFromXmlNode(Traverse.Create(this), xmlRoot, this, nameof(this.entry));
            }
            else
            {
                Traverse traverse = Traverse.Create(this);
                foreach (XmlNode xmlNode in xmlRoot.ChildNodes) 
                    Utilities.SetFieldFromXmlNode(traverse, xmlNode, this, xmlNode.Name == "defName" ? nameof(this.entry) : xmlNode.Name);
            }
        }
    }

    public class TraitWithDegree
    {
        public TraitDef def;
        public int      degree = 0;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.def), xmlRoot?.FirstChild?.Value ?? xmlRoot?.Value ?? xmlRoot?.InnerText);
            int.TryParse(xmlRoot.Attributes?["Degree"]?.Value, out this.degree);
        }

        public override string ToString() => $"{nameof(TraitWithDegree)}: {this.def?.defName} | {this.degree}";
    }

    public class GraphicPaths
    {
        public const string VANILLA_HEAD_PATH     = "Things/Pawn/Humanlike/Heads/";
        public const string VANILLA_BODY_PATH     = "Things/Pawn/Humanlike/Bodies/";
        public const string VANILLA_SKELETON_PATH = "Things/Pawn/Humanlike/HumanoidDessicated";
        
        public AlienPartGenerator.ExtendedGraphicTop body              = new() { path = VANILLA_BODY_PATH};
        public AlienPartGenerator.ExtendedGraphicTop bodyMasks         = new() { path = string.Empty };
        public AlienPartGenerator.ExtendedGraphicTop head              = new() { path = VANILLA_HEAD_PATH };
        public AlienPartGenerator.ExtendedGraphicTop headMasks         = new() { path = string.Empty };

        public AlienPartGenerator.ExtendedGraphicTop skeleton = new() { path = VANILLA_SKELETON_PATH };
        public AlienPartGenerator.ExtendedGraphicTop skull    = new() { path = "Things/Pawn/Humanlike/Heads/None_Average_Skull" };
        public AlienPartGenerator.ExtendedGraphicTop stump    = new() { path = "Things/Pawn/Humanlike/Heads/None_Average_Stump" };
        public AlienPartGenerator.ExtendedGraphicTop swaddle  = new() { path = "Things/Pawn/Humanlike/Apparel/SwaddledBaby/Swaddled_Child" };

        public ApparelGraphics.ApparelGraphicsOverrides apparel = new();

        public ShaderTypeDef   skinShader;
        public Color           skinColor = new(1f, 0f, 0f, 1f);

        private ShaderParameter skinColoringParameter;
        public ShaderParameter SkinColoringParameter
        {
            get
            {
                if (this.skinColoringParameter == null)
                {
                    ShaderParameter parameter = new();
                    Traverse        traverse  = Traverse.Create(parameter);
                    traverse.Field("name").SetValue("_ShadowColor");
                    traverse.Field("value").SetValue(new Vector4(this.skinColor.r, this.skinColor.g, this.skinColor.b, this.skinColor.a));
                    traverse.Field("type").SetValue(1);
                    this.skinColoringParameter = parameter;
                }
                return this.skinColoringParameter;
            }
        }
    }

    public class DirectionOffset
    {
        public Vector2 north = Vector2.zero;
        public Vector2 west = Vector2.zero;
        public Vector2 east = Vector2.zero;
        public Vector2 south = Vector2.zero;

        public Vector2 GetOffset(Rot4 rot) => 
            rot == Rot4.North ? this.north : rot == Rot4.East ? this.east : rot == Rot4.West ? this.west : this.south;
    }

    public class StyleSettings
    {
        public bool          hasStyle        = true;
        public bool          genderRespected = true;
        public List<string>  styleTags;
        public List<string>  styleTagsOverride;
        public List<string>  bannedTags;
        public ShaderTypeDef shader;

        public bool IsValidStyle(StyleItemDef styleItemDef, Pawn pawn, bool useOverrides = false) =>
            !this.hasStyle ?
                styleItemDef.styleTags.Contains("alienNoStyle") :

                (useOverrides ?
                     this.styleTagsOverride.NullOrEmpty() || this.styleTagsOverride.Any(s => styleItemDef.styleTags.Contains(s)) :
                     this.styleTags.NullOrEmpty()         || this.styleTags.Any(s => styleItemDef.styleTags.Contains(s))) &&
                (this.bannedTags.NullOrEmpty() || !this.bannedTags.Any(s => styleItemDef.styleTags.Contains(s)))          &&

                (!this.genderRespected      ||
                 pawn.gender == Gender.None ||
                 (pawn.Ideo?.style.GetGender(styleItemDef) ?? styleItemDef.styleGender, pawn.gender) switch
                 {
                     (StyleGender.Any or StyleGender.MaleUsually or StyleGender.FemaleUsually, _) or (StyleGender.Male, Gender.Male) or (StyleGender.Female, Gender.Female) => true,
                     _ => false
                 });
    }

    public class ThoughtSettings
    {
        public List<ThoughtDef> cannotReceiveThoughts;
        public bool         cannotReceiveThoughtsAtAll = false;
        public List<ThoughtDef> canStillReceiveThoughts;

        public static Dictionary<ThoughtDef, List<ThingDef_AlienRace>> thoughtRestrictionDict = new();
        public        List<ThoughtDef>                                 restrictedThoughts     = new();

        public ThoughtDef ReplaceIfApplicable(ThoughtDef def) =>
            (this.replacerList == null || this.replacerList.Select(selector: tr => tr.replacer).Contains(def))
                ? def : this.replacerList.FirstOrDefault(predicate: tr => tr.original == def)?.replacer ?? def;

        public ButcherThought       butcherThoughtGeneral  = new();
        public List<ButcherThought> butcherThoughtSpecific = new();

        public AteThought       ateThoughtGeneral  = new();
        public List<AteThought> ateThoughtSpecific = new();

        public ThoughtDef GetAteThought(ThingDef race, bool cannibal, bool ingredient) =>
            (this.ateThoughtSpecific?.FirstOrDefault(predicate: at => at.raceList?.Contains(race) ?? false) ?? this.ateThoughtGeneral)?.GetThought(cannibal, ingredient);

        public bool CanGetThought(ThoughtDef def)
        {
            def = this.ReplaceIfApplicable(def);

            return (!this.cannotReceiveThoughtsAtAll || (this.canStillReceiveThoughts?.Contains(def) ?? false)) &&
                   (!(this.cannotReceiveThoughts?.Contains(def) ?? false));
        }

        private static readonly Dictionary<uint, bool> canGetThoughtCache = [];

        public static bool CanGetThought(ThoughtDef def, ThingDef race)
        {
            uint key = def.shortHash | ((uint)race.shortHash << 16);

            if (!canGetThoughtCache.TryGetValue(key, out bool canGetThought))
            {
                bool result = !(thoughtRestrictionDict.TryGetValue(def, out List<ThingDef_AlienRace> races));

                canGetThoughtCache.Add(key, canGetThought = race is not ThingDef_AlienRace alienProps ?
                                                                result :
                                                                (races?.Contains(alienProps) ?? true) && alienProps.alienRace.thoughtSettings.CanGetThought(def));
            }
            return canGetThought;
        }

        public static bool CanGetThought(ThoughtDef def, Pawn pawn) => 
            CanGetThought(def, pawn.def);

        public List<ThoughtReplacer> replacerList;
    }

    public class ButcherThought
    {
        public List<ThingDef> raceList;

        [LoadDefFromField(nameof(AlienDefOf.ButcheredHumanlikeCorpse))]
        public ThoughtDef thought;

        [LoadDefFromField(nameof(AlienDefOf.KnowButcheredHumanlikeCorpse))]
        public ThoughtDef knowThought;
    }

    public class AteThought
    {
        public List<ThingDef> raceList;
        [LoadDefFromField(nameof(AlienDefOf.AteHumanlikeMeatDirect))]
        public ThoughtDef thought;

        [LoadDefFromField(nameof(AlienDefOf.AteHumanlikeMeatDirectCannibal))]
        public ThoughtDef thoughtCannibal;

        [LoadDefFromField(nameof(AlienDefOf.AteHumanlikeMeatAsIngredient))]
        public ThoughtDef ingredientThought;

        [LoadDefFromField(nameof(AlienDefOf.AteHumanlikeMeatAsIngredientCannibal))]
        public ThoughtDef ingredientThoughtCannibal;

        public ThoughtDef GetThought(bool cannibal, bool ingredient) =>
            cannibal ? ingredient ? this.ingredientThoughtCannibal : this.thoughtCannibal : ingredient ? this.ingredientThought : this.thought;
    }

    public class ThoughtReplacer
    {
        public ThoughtDef original;
        public ThoughtDef replacer;
    }

    public class RelationSettings
    {
        public float relationChanceModifierChild = 1f;
        public float relationChanceModifierExLover = 1f;
        public float relationChanceModifierExSpouse = 1f;
        public float relationChanceModifierFiance = 1f;
        public float relationChanceModifierLover = 1f;
        public float relationChanceModifierParent = 1f;
        public float relationChanceModifierSibling = 1f;
        public float relationChanceModifierSpouse = 1f;

        public List<RelationRenamer> renamer;
    }

    public class RelationRenamer
    {
        public PawnRelationDef relation;
        public string label;
        public string femaleLabel;
    }

    public class RaceRestrictionSettings
    {
        public bool           onlyUseRaceRestrictedApparel = false;
        public List<ThingDef> apparelList                  = new();
        public List<ThingDef> whiteApparelList             = new();
        public List<ThingDef> blackApparelList             = new();

        public static HashSet<ThingDef> apparelRestricted = new();

        public static bool CanWear(ThingDef apparel, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool result = true;

            if (apparelRestricted.Contains(apparel) || (raceRestriction?.onlyUseRaceRestrictedApparel ?? false)) 
                result = raceRestriction?.whiteApparelList.Contains(apparel) ?? false;

            return result && !(raceRestriction?.blackApparelList.Contains(apparel) ?? false);
        }


        public        List<ResearchProjectRestrictions>                        researchList            = new();
        public static Dictionary<ResearchProjectDef, List<ThingDef_AlienRace>> researchRestrictionDict = new();

        public static bool CanResearch(IEnumerable<ThingDef> races, ResearchProjectDef project) =>
            !researchRestrictionDict.ContainsKey(project) || races.Any(predicate: ar => researchRestrictionDict[project].Contains(ar));


        public bool           onlyUseRaceRestrictedWeapons = false;
        public List<ThingDef> weaponList                   = new();
        public List<ThingDef> whiteWeaponList              = new();
        public List<ThingDef> blackWeaponList              = new();

        public static HashSet<ThingDef>                              weaponRestricted     = new();

        public static bool CanEquip(ThingDef weapon, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (weaponRestricted.Contains(weapon) || (raceRestriction?.onlyUseRaceRestrictedWeapons ?? false))
                result = raceRestriction?.whiteWeaponList.Contains(weapon)                          ?? false;

            return result && !(raceRestriction?.blackWeaponList.Contains(weapon) ?? false);
        }

        public bool           onlyBuildRaceRestrictedBuildings = false;
        public List<ThingDef> buildingList                     = new();
        public List<ThingDef> whiteBuildingList                = new();
        public List<ThingDef> blackBuildingList                = new();

        public static HashSet<ThingDef> buildingRestricted = new();

        public static HashSet<ThingDef> buildingsRestrictedWithCurrentColony = new();

        public static bool CanColonyBuild(BuildableDef building) => 
            !buildingsRestrictedWithCurrentColony.Contains(building);

        public static bool CanBuild(BuildableDef building, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (buildingRestricted.Contains(building) || (raceRestriction?.onlyBuildRaceRestrictedBuildings ?? false))
                result = raceRestriction?.whiteBuildingList.Contains(building)                          ?? false;

            return result && !(raceRestriction?.blackBuildingList.Contains(building) ?? false);
        }




        public bool            onlyDoRaceRestrictedRecipes = false;
        public List<RecipeDef> recipeList                  = new();
        public List<RecipeDef> whiteRecipeList             = new();
        public List<RecipeDef> blackRecipeList             = new();

        public static HashSet<RecipeDef> recipeRestricted = new();

        public static bool CanDoRecipe(RecipeDef recipe, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (recipeRestricted.Contains(recipe) || (raceRestriction?.onlyDoRaceRestrictedRecipes ?? false))
                result = raceRestriction?.whiteRecipeList.Contains(recipe)                              ?? false;

            return result && !(raceRestriction?.blackRecipeList.Contains(recipe) ?? false);
        }

        public bool           onlyDoRaceRestrictedPlants = false;
        public List<ThingDef> plantList                  = new();
        public List<ThingDef> whitePlantList             = new();
        public List<ThingDef> blackPlantList             = new();

        public static HashSet<ThingDef> plantRestricted = new();

        public static bool CanPlant(ThingDef plant, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (plantRestricted.Contains(plant) || (raceRestriction?.onlyDoRaceRestrictedPlants ?? false))
                result = raceRestriction?.whitePlantList.Contains(plant)                          ?? false;

            return result && !(raceRestriction?.blackPlantList.Contains(plant) ?? false);
        }

        public bool           onlyGetRaceRestrictedTraits = false;
        public List<TraitDef> traitList                   = new();
        public List<TraitDef> whiteTraitList              = new();
        public List<TraitDef> blackTraitList              = new();

        public static HashSet<TraitDef> traitRestricted = new();

        public static bool CanGetTrait(TraitDef trait, Pawn pawn, int degree = 0)
        {
            List<AlienChanceEntry<TraitWithDegree>> disallowedTraits = [];

            foreach (BackstoryDef backstory in pawn.story.AllBackstories)
                if (backstory is AlienBackstoryDef alienBackstory)
                    if(!alienBackstory.disallowedTraitsChance.NullOrEmpty()) 
                        disallowedTraits.AddRange(alienBackstory.disallowedTraitsChance);

            return CanGetTrait(trait, pawn.def, degree, disallowedTraits);
        }

        public static bool CanGetTrait(TraitDef trait, ThingDef race, int degree = 0, List<AlienChanceEntry<TraitWithDegree>> disallowedTraits = null)
        {
            ThingDef_AlienRace.AlienSettings           alienProps   = (race as ThingDef_AlienRace)?.alienRace;
            RaceRestrictionSettings raceRestriction = alienProps?.raceRestriction;
            bool                    result          = true;

            if (traitRestricted.Contains(trait) || (raceRestriction?.onlyGetRaceRestrictedTraits ?? false))
                result &= raceRestriction?.whiteTraitList.Contains(trait)                        ?? false;

            disallowedTraits ??= [];

            if (!(alienProps?.generalSettings.disallowedTraits.NullOrEmpty() ?? true))
                disallowedTraits.AddRange(alienProps.generalSettings.disallowedTraits);

            if (!disallowedTraits.NullOrEmpty())
                result &= disallowedTraits.All(ace => ace.Select(null).All(traitEntry => traitEntry.def != trait || degree != traitEntry.degree));
            

            return result && !(raceRestriction?.blackTraitList.Contains(trait) ?? false);
        }
        
        public bool           onlyEatRaceRestrictedFood = false;
        public List<ThingDef> foodList                  = new();
        public List<ThingDef> whiteFoodList             = new();
        public List<ThingDef> blackFoodList             = new();

        public static HashSet<ThingDef> foodRestricted = new();

        public static bool CanEat(ThingDef food, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (foodRestricted.Contains(food) || (raceRestriction?.onlyEatRaceRestrictedFood ?? false))
                result = raceRestriction?.whiteFoodList.Contains(food)                        ?? false;

            result &= !(raceRestriction?.blackFoodList.Contains(food) ?? false);

            ChemicalDef chemical = food.GetCompProperties<CompProperties_Drug>()?.chemical;
            return result && (chemical == null || ((race as ThingDef_AlienRace)?.alienRace.generalSettings.chemicalSettings?.TrueForAll(match: c => c.ingestible || c.chemical != chemical) ?? true));
        }

        public bool           onlyTameRaceRestrictedPets = false;
        public List<ThingDef> petList                    = new();
        public List<ThingDef> whitePetList               = new();
        public List<ThingDef> blackPetList               = new();

        public static HashSet<ThingDef> petRestricted = new();

        public static bool CanTame(ThingDef pet, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (petRestricted.Contains(pet) || (raceRestriction?.onlyTameRaceRestrictedPets ?? false))
                result = raceRestriction?.whitePetList.Contains(pet)                       ?? false;

            return result && !(raceRestriction?.blackPetList.Contains(pet) ?? false);
        }

        public List<ConceptDef> conceptList = new();

        public List<WorkGiverDef> workGiverList = new();

        public bool          onlyHaveRaceRestrictedGenes = false;
        public List<GeneDef> geneList                    = [];
        public List<GeneDef> whiteGeneList               = [];
        public List<string>  whiteGeneTags               = [];
        public List<GeneDef> blackGeneList               = [];
        public List<string>  blackGeneTags               = [];
        public List<EndogeneCategory> blackEndoCategories = [];

        public static HashSet<GeneDef> geneRestricted = [];

        public bool                   onlyHaveRaceRestrictedGenesXeno = false;
        public List<GeneDef>          geneListXeno                    = [];
        public List<GeneDef>          whiteGeneListXeno               = [];
        public List<string>           whiteGeneTagsXeno               = [];
        public List<GeneDef>          blackGeneListXeno               = [];
        public List<string>           blackGeneTagsXeno               = [];
        public List<EndogeneCategory> blackEndoCategoriesXeno         = [];

        public static HashSet<GeneDef> geneRestrictedXeno = [];

        public bool onlyHaveRaceRestrictedGenesEndo = false;
        public List<GeneDef> geneListEndo = [];
        public List<GeneDef> whiteGeneListEndo = [];
        public List<string> whiteGeneTagsEndo = [];
        public List<GeneDef> blackGeneListEndo = [];
        public List<string> blackGeneTagsEndo = [];
        public List<EndogeneCategory> blackEndoCategoriesEndo = [];

        public static HashSet<GeneDef> geneRestrictedEndo = [];

        public static bool CanHaveGene(GeneDef gene, ThingDef race, bool xeno)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (geneRestricted.Contains(gene) || (raceRestriction?.onlyHaveRaceRestrictedGenes ?? false))
                result = (raceRestriction?.whiteGeneList.Contains(gene)                                           ?? false) ||
                         (gene.exclusionTags?.Any(t => raceRestriction?.whiteGeneTags.Any(t.StartsWith) ?? false) ?? false);

            if (xeno)
            {
                if (geneRestrictedXeno.Contains(gene) || (raceRestriction?.onlyHaveRaceRestrictedGenesXeno ?? false))
                    result &= (raceRestriction?.whiteGeneListXeno.Contains(gene)                                           ?? false) ||
                              (gene.exclusionTags?.Any(t => raceRestriction?.whiteGeneTagsXeno.Any(t.StartsWith) ?? false) ?? false);

                result &= !(raceRestriction?.blackGeneListXeno.Contains(gene)                                           ?? false) &&
                          !(gene.exclusionTags?.Any(t => raceRestriction?.blackGeneTagsXeno.Any(t.StartsWith) ?? false) ?? false) &&
                          !(raceRestriction?.blackEndoCategoriesXeno.Contains(gene.endogeneCategory)                    ?? false);

            }
            else
            {
                if (geneRestrictedEndo.Contains(gene) || (raceRestriction?.onlyHaveRaceRestrictedGenesEndo ?? false))
                    result &= (raceRestriction?.whiteGeneListEndo.Contains(gene)                                           ?? false) ||
                              (gene.exclusionTags?.Any(t => raceRestriction?.whiteGeneTagsEndo.Any(t.StartsWith) ?? false) ?? false);

                result &= !(raceRestriction?.blackGeneListEndo.Contains(gene)                                           ?? false) &&
                          !(gene.exclusionTags?.Any(t => raceRestriction?.blackGeneTagsEndo.Any(t.StartsWith) ?? false) ?? false) &&
                          !(raceRestriction?.blackEndoCategoriesEndo.Contains(gene.endogeneCategory)                    ?? false);
            }


            return result                                                                                              &&
                   !(raceRestriction?.blackGeneList.Contains(gene)                                           ?? false) &&
                   !(gene.exclusionTags?.Any(t => raceRestriction?.blackGeneTags.Any(t.StartsWith) ?? false) ?? false) &&
                   !(raceRestriction?.blackEndoCategories.Contains(gene.endogeneCategory)                    ?? false);
        }

        public bool              onlyUseRaceRestrictedXenotypes = false;
        public List<XenotypeDef> xenotypeList                   = new();
        public List<XenotypeDef> whiteXenotypeList              = new();
        public List<XenotypeDef> blackXenotypeList              = new();

        public static HashSet<XenotypeDef> xenotypeRestricted = new();

        public static HashSet<XenotypeDef> FilterXenotypes(IEnumerable<XenotypeDef> xenotypes, ThingDef race, out HashSet<XenotypeDef> removedXenotypes)
        {
            HashSet<XenotypeDef> xenotypeDefs = new();
            removedXenotypes = new HashSet<XenotypeDef>();

            foreach (XenotypeDef xenotypeDef in xenotypes)
                if (CanUseXenotype(xenotypeDef, race))
                    xenotypeDefs.Add(xenotypeDef);
                else
                    removedXenotypes.Add(xenotypeDef);

            return xenotypeDefs;
        }

        public static bool CanUseXenotype(XenotypeDef xenotype, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (xenotypeRestricted.Contains(xenotype) || (raceRestriction?.onlyUseRaceRestrictedXenotypes ?? false))
                result = raceRestriction?.whiteXenotypeList.Contains(xenotype)                        ?? false;

            return result && !(raceRestriction?.blackXenotypeList.Contains(xenotype) ?? false);
        }

        public bool           canReproduce                     = true;
        public bool           canReproduceWithSelf             = true;
        public bool           onlyReproduceWithRestrictedRaces = false;
        public List<ThingDef> reproductionList                 = new();
        public List<ThingDef> whiteReproductionList            = new();
        public List<ThingDef> blackReproductionList            = new();

        public static HashSet<ThingDef> reproductionRestricted = new();

        public static bool CanReproduce(Pawn pawn, Pawn partnerPawn) =>
            ReproductionSettings.GenderReproductionCheck(pawn, partnerPawn) &&
            CanReproduce(pawn.def, partnerPawn.def);

        public static bool CanReproduce(ThingDef race, ThingDef partnerRace) => 
            CanReproduceWith(race, partnerRace) && CanReproduceWith(partnerRace, race);

        private static bool CanReproduceWith(ThingDef race, ThingDef partnerRace)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            if (!(raceRestriction?.canReproduce ?? true))
                return false;

            if (race == partnerRace)
                return raceRestriction?.canReproduceWithSelf ?? true;

            bool result = true;
            if (reproductionRestricted.Contains(partnerRace) || (raceRestriction?.onlyReproduceWithRestrictedRaces ?? false))
                result = raceRestriction?.whiteReproductionList.Contains(partnerRace)                              ?? false;

            return result && !(raceRestriction?.blackReproductionList.Contains(partnerRace) ?? false);
        }
    }

    public class ResearchProjectRestrictions
    {
        public List<ResearchProjectDef> projects = new();
        public List<ThingDef>           apparelList;
    }

    public class Info : DefModExtension
    {
        public bool allowHumanBios = true;
        public float maleGenderProbability = 0.5f;
    }

    public class LifeStageAgeAlien : LifeStageAge
    {
        public BodyDef body;

        public Vector2                              headOffset = Vector2.zero;
        public AlienPartGenerator.DirectionalOffset headOffsetDirectional;

        public Vector2                              headFemaleOffset = Vector2.negativeInfinity;
        public AlienPartGenerator.DirectionalOffset headFemaleOffsetDirectional;


        public Vector2 customDrawSize             = Vector2.zero;
        public Vector2 customPortraitDrawSize     = Vector2.zero;
        public Vector2 customHeadDrawSize         = Vector2.zero;
        public Vector2 customPortraitHeadDrawSize = Vector2.zero;

        public Vector2 customFemaleDrawSize             = Vector2.zero;
        public Vector2 customFemalePortraitDrawSize     = Vector2.zero;
        public Vector2 customFemaleHeadDrawSize         = Vector2.zero;
        public Vector2 customFemalePortraitHeadDrawSize = Vector2.zero;
    }

    public class StatPartAgeOverride
    {
        public StatDef stat;
        public StatPart_Age overridePart;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);
            this.overridePart = DirectXmlToObject.ObjectFromXml<StatPart_Age>(xmlRoot, false);
        }
    }

    public class CompatibilityInfo
    {
        protected bool isFlesh = true;

        public virtual bool IsFlesh
        {
            get => this.isFlesh;
            set => this.isFlesh = value;
        }

        public virtual bool IsFleshPawn(Pawn pawn) => this.IsFlesh;

        protected bool isSentient = true;

        public virtual bool IsSentient
        {
            get => this.isSentient;
            set => this.isSentient = value;
        }

        public virtual bool IsSentientPawn(Pawn pawn) => this.IsSentient;

        protected bool hasBlood = true;

        public virtual bool HasBlood
        {
            get => this.hasBlood;
            set => this.hasBlood = value;
        }

        public virtual bool HasBloodPawn(Pawn pawn) => this.HasBlood;
    }
}