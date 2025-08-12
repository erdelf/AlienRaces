namespace AlienRace
{
    using ExtendedGraphics;
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using static UnityEngine.Networking.UnityWebRequest;

    [UsedImplicitly]
    internal class Comp_OutfitStandHAR : ThingComp
    {
        public Building_OutfitStand OutfitStand => this.parent as Building_OutfitStand;

        private ThingDef    race;
        private BodyTypeDef bodyType;
        private HeadTypeDef headType;

        [Unsaved]
        public Graphic_Multi bodyGraphic;
        [Unsaved]
        public Graphic_Multi headGraphic;

        public ThingDef Race
        {
            get => this.race ?? ThingDefOf.Human;
            set
            {
                if (this.race == value || value == null) 
                    return;
                this.race     = value;

                this.blockRecache = true;

                IEnumerable<BodyTypeDef> bodies = this.BodyTypesAvailable.ToList();
                if(!bodies.Contains(this.BodyType))
                    this.BodyType = bodies.RandomElement();
                IEnumerable<HeadTypeDef> heads = this.HeadTypesAvailable.ToList();
                if(!heads.Contains(this.HeadType))
                    this.HeadType = heads.RandomElement();

                this.blockRecache = false;
                this.RecacheGraphics();
            }
        }

        private IEnumerable<BodyTypeDef> BodyTypesAvailable
        {
            get
            {
                if(this.Race is ThingDef_AlienRace alienProps                               &&
                    alienProps.alienRace.generalSettings.alienPartGenerator is { } parts &&
                    !parts.bodyTypes.NullOrEmpty())
                    return parts.bodyTypes;

                return DefDatabase<BodyTypeDef>.AllDefsListForReading;
            }
        }

        private IEnumerable<HeadTypeDef> HeadTypesAvailable
        {
            get
            {
                if (this.Race is ThingDef_AlienRace alienProps                           &&
                    alienProps.alienRace.generalSettings.alienPartGenerator is { } parts &&
                    !parts.HeadTypes.NullOrEmpty())
                    return parts.HeadTypes;
                return CachedData.DefaultHeadTypeDefs;
            }
        }

        public BodyTypeDef BodyType
        {
            get => this.bodyType;
            set
            {
                if (this.bodyType == value)
                    return;
                this.bodyType = value;

                this.OutfitStand.StoreSettings.filter.SetAllow(SpecialThingFilterDef.Named("AllowAdultOnlyApparel"), !this.IsJuvenileBodyType);
                this.OutfitStand.StoreSettings.filter.SetAllow(SpecialThingFilterDef.Named("AllowChildOnlyApparel"), this.IsJuvenileBodyType);

                this.RecacheGraphics();
            }
        }

        public bool IsJuvenileBodyType => (this.bodyType == BodyTypeDefOf.Baby || this.bodyType == BodyTypeDefOf.Child);

        public HeadTypeDef HeadType
        {
            get => this.headType;
            set
            {
                this.headType = value;
                this.RecacheGraphics();
            }
        }

        private void RecacheGraphics()
        {
            LongEventHandler.ExecuteWhenFinished(this.RecacheGraphicsStatic);
        }

        private bool blockRecache = false;

        private void RecacheGraphicsStatic()
        {
            if (this.HeadType == null || this.BodyType == null || this.blockRecache)
                return;

            ThingWithComps heldWeapon = this.OutfitStand.HeldWeapon;
            if (heldWeapon != null)
                if (!(this.OutfitStand as IHaulDestination).Accepts(heldWeapon))
                    this.OutfitStand.TryDrop(heldWeapon, this.parent.Position, ThingPlaceMode.Near, 1, out _);

            if(this.OutfitStand.HeldItems.Any())
                foreach (Thing item in this.OutfitStand.HeldItems.ToList())
                    if (item is Apparel && !(this.OutfitStand as IHaulDestination).Accepts(item))
                        this.OutfitStand.TryDrop(item, this.parent.Position, ThingPlaceMode.Near, 1, out _);


            int savedIndex = this.parent.HashOffset();
            int shared     = 0;

            AlienPartGenerator.ExtendedGraphicTop.drawOverrideDummy = new DummyExtendedGraphicsPawnWrapper { race = this.Race, bodyType = this.BodyType, headType = this.HeadType};

            ThingDef_AlienRace alienRace = (this.Race as ThingDef_AlienRace);
            string             bodyPath          = alienRace?.alienRace.graphicPaths.body.GetPath(null, ref shared, savedIndex);

            this.bodyGraphic = CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), bodyPath, ShaderDatabase.Cutout,
                                                                             CachedData.outfitStandDrawSizeBody() * (alienRace?.alienRace.generalSettings.alienPartGenerator.customDrawSize ?? Vector2.one), 
                                                                             Color.white, Color.white, null, 0, null, string.Empty));

            string headPath = alienRace?.alienRace.graphicPaths.head.GetPath(null, ref shared, savedIndex);

            this.headGraphic = CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), headPath, ShaderDatabase.Cutout,
                                                                                 CachedData.outfitStandDrawSizeHead() * (alienRace?.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize ?? Vector2.one), 
                                                                                 Color.white, Color.white, null, 0, null, string.Empty));

            CachedData.outfitStandRecacheGraphics(this.OutfitStand);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad)
                return;
            if(this.race == null)
                this.Race = this.parent.Faction != null ? this.parent.Faction.def.basicMemberKind.race ?? ThingDefOf.Human : ThingDefOf.Human;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra()) 
                yield return gizmo;

            if (this.parent.Faction != null && this.parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                             {
                                 defaultLabel = this.Race.LabelCap,
                                 defaultDesc  = "HAR.OutfitStandRaceCommandDesc".Translate(),
                                 icon         = this.Race.uiIcon,
                                 action       = () =>
                                                {
                                                    Find.WindowStack.Add(new FloatMenu(HarmonyPatches.colonistRaces.Select(td => 
                                                                                            new FloatMenuOption(td.LabelCap, () => this.Race = td)).ToList()));
                                                }
                             };

                yield return new Command_Action
                             {
                                 defaultLabel = this.BodyType.defName,
                                 defaultDesc  = "HAR.OutfitStandBodyCommandDesc".Translate(),
                                 icon         = this.BodyType.Icon,
                                 action = () =>
                                          {
                                              Find.WindowStack.Add(new FloatMenu(this.BodyTypesAvailable.Select(bd => 
                                                                                            new FloatMenuOption(bd.defName, () => this.BodyType = bd, bd.Icon, this.parent.Stuff?.stuffProps.color ?? Color.white)).ToList()));
                                          }
                             };

                yield return new Command_Action
                             {
                                 defaultLabel = this.HeadType.defName,
                                 defaultDesc  = "HAR.OutfitStandHeadCommandDesc".Translate(),
                                 icon         = this.HeadType.Icon,
                                 action = () =>
                                          {
                                              Find.WindowStack.Add(new FloatMenu(this.HeadTypesAvailable.Select(hdt =>
                                                                                                                    new FloatMenuOption(hdt.defName, () => this.HeadType = hdt, hdt.Icon, this.parent.Stuff?.stuffProps.color ?? Color.white)).ToList()));
                                          }
                             };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref this.race,     nameof(this.Race));
            Scribe_Defs.Look(ref this.bodyType, nameof(this.BodyType));
            Scribe_Defs.Look(ref this.headType, nameof(this.HeadType));

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.Race     ??= ThingDefOf.Human;
                this.BodyType ??= this.BodyTypesAvailable.RandomElement();
                this.HeadType ??= this.HeadTypesAvailable.RandomElement();

                this.RecacheGraphics();
            }
        }
    }
}