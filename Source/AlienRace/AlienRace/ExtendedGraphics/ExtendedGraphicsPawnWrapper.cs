namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

/**
 * Encapsulates pawn access for the purpose of BodyAddon access.
 */
public class ExtendedGraphicsPawnWrapper
{
    public Pawn WrappedPawn { get; private set; }

    public ExtendedGraphicsPawnWrapper(Pawn pawn) => this.WrappedPawn = pawn;

    public ExtendedGraphicsPawnWrapper()
    {
    }

    public virtual PawnDrawParms DrawParms => 
        CachedData.oldDrawParms(this.WrappedPawn.Drawer.renderer.renderTree);

    public virtual bool HasApparelGraphics() => 
        this.GetWornApparel.Any(ap => ap.def.apparel.HasDefinedGraphicProperties);

    //backstory isApplicable
    public virtual bool HasBackStory(BackstoryDef backstory) =>
        this.GetBackstories().Contains(backstory); //matches pawn backstory with input

    private bool IsHediffOfDefAndPart(Hediff hediff, HediffDef hediffDef, BodyPartDef part, string partLabel) => //checks if specific hediff is on given part or no part
        hediff.def == hediffDef && (hediff.Part == null || this.IsBodyPart(hediff.Part, part, partLabel));

    public virtual IEnumerable<float> SeverityOfHediffsOnPart(HediffDef hediffDef, BodyPartDef part, string partLabel) =>
        this.GetHediffList()
         .Where(h => this.IsHediffOfDefAndPart(h, hediffDef, part, partLabel))
         .Select(h => h.Severity);

    //hediff isApplicable
    public virtual Hediff HasHediffOfDefAndPart(HediffDef hediffDef, BodyPartDef part, string partLabel) => 
        this.GetHediffList().FirstOrDefault(h => this.IsHediffOfDefAndPart(h, hediffDef, part, partLabel)); //compares pawn hediffs to specified hediff def and part

    //age isApplicable
    public virtual bool CurrentLifeStageDefMatches(LifeStageDef lifeStageDef) =>
        this.WrappedPawn.ageTracker?.CurLifeStage?.Equals(lifeStageDef) ?? false;

    //damage isApplicable
    public virtual bool IsPartBelowHealthThreshold(BodyPartDef part, string partLabel, float healthThreshold) =>
        // look for part where a given hediff has a part matching defined part
        this.GetHediffList()
         .Select(hediff => hediff.Part)
         .Where(hediffPart => hediffPart != null)
         .Where(hediffPart => this.IsBodyPart(hediffPart, part, partLabel))
             //check if part health is less than health texture limit, needs to config ascending
         .Any(p => healthThreshold >= this.GetHediffSet().GetPartHealth(p));

    //trait isApplicable
    public virtual bool HasTraitWithIdentifier(string traitId)
    {
        string capitalisedTrait = traitId.CapitalizeFirst();
        return this.GetTraitList().Select(t => t.CurrentData).Any(t => 
                    capitalisedTrait == t.LabelCap || capitalisedTrait == t.GetLabelCapFor(this.WrappedPawn) || capitalisedTrait == t.untranslatedLabel.CapitalizeFirst());
    }

    public virtual IEnumerable<Apparel> GetWornApparel =>
        this.WrappedPawn.apparel?.WornApparel ?? Enumerable.Empty<Apparel>();

    public virtual IEnumerable<ApparelProperties> GetWornApparelProps() =>
        this.WrappedPawn.apparel?.WornApparel?.Select(ap => ap.def.apparel) ?? [];

    public virtual bool VisibleInBed(bool noBed = true) => 
        this.WrappedPawn.CurrentBed()?.def?.building?.bed_showSleeperBody ?? noBed;

    public virtual bool HasNamedBodyPart(BodyPartDef part, string partLabel) =>
        (part == null && partLabel.NullOrEmpty()) || this.GetBodyPart(part, partLabel) != null;

    public virtual BodyPartRecord GetBodyPart(BodyPartDef part, string partLabel) =>
        this.GetHediffSet().GetNotMissingParts()?.FirstOrDefault(bpr => IsBodyPart(bpr, part, partLabel));

    public virtual bool IsBodyPart(BodyPartRecord bpr, BodyPartDef part, string partLabel) =>
        (partLabel.NullOrEmpty() || bpr.untranslatedCustomLabel == partLabel) && (part == null || bpr.def == part);

    public virtual bool LinkToCorePart(bool drawWithoutPart, bool alignWithHead, BodyPartDef part, string partLabel) => 
        drawWithoutPart && 
        !this.NamedBodyPartExists(part, partLabel) && 
        (!alignWithHead || this.GetHediffSet().HasHead);

    public virtual bool NamedBodyPartExists(BodyPartDef part, string partLabel) =>
        (part == null && partLabel.NullOrEmpty()) || this.GetAnyBodyPart(part, partLabel) != null;

    public virtual BodyPartRecord GetAnyBodyPart(BodyPartDef part, string partLabel) =>
        this.WrappedPawn.RaceProps.body.AllParts.Find(bpr => IsBodyPart(bpr, part, partLabel));

    public virtual float GetNeed(NeedDef needDef, bool percentage)
    {
        Need need = this.WrappedPawn.needs?.TryGetNeed(needDef);
        return need == null ? 0 : percentage ? need.CurLevelPercentage : need.CurLevel;
    }

    public virtual Gender GetGender() => this.WrappedPawn.gender;

    public virtual PawnPosture GetPosture() => this.WrappedPawn.GetPosture();

    public virtual bool HasBodyType(BodyTypeDef bodyType) =>
        this.WrappedPawn.story.bodyType == bodyType;

    public virtual bool HasHeadTypeNamed(HeadTypeDef headType) =>
        this.WrappedPawn.story.headType == headType;

    public virtual RotStage? GetRotStage() => this.WrappedPawn.Corpse?.GetRotStage();

    public virtual RotDrawMode GetRotDrawMode() => this.WrappedPawn.Drawer.renderer.CurRotDrawMode;

    public virtual List<Hediff> GetHediffList() => this.GetHediffSet().hediffs;

    public virtual List<Trait> GetTraitList() => this.GetTraits().allTraits;

    /**
     * Helper to get a HediffSet or initialise one if absent
     */
    public virtual HediffSet GetHediffSet() => this.WrappedPawn.health?.hediffSet ?? new HediffSet(this.WrappedPawn);

    public virtual IEnumerable<BackstoryDef> GetBackstories() =>
        this.WrappedPawn.story?.AllBackstories ?? Enumerable.Empty<BackstoryDef>();

    public virtual TraitSet GetTraits() => this.WrappedPawn.story?.traits ?? new TraitSet(this.WrappedPawn);

    public virtual bool Drafted => this.WrappedPawn.Drafted;

    public virtual Job CurJob => this.WrappedPawn.CurJob;

    public virtual bool Moving => this.WrappedPawn.pather.MovingNow;

    public virtual bool HasGene(GeneDef gene) => this.WrappedPawn.genes.GetGene(gene)?.Active ?? false;

    public virtual bool IsRace(ThingDef race) => this.WrappedPawn.def == race;

    public virtual bool IsMutant(MutantDef def) => this.WrappedPawn.IsMutant && (def == null || this.WrappedPawn.mutant.Def == def);

    public virtual bool IsCreepJoiner(CreepJoinerFormKindDef def) => this.WrappedPawn.IsCreepJoiner && (def == null || this.WrappedPawn.creepjoiner.form == def);
}

public class DummyExtendedGraphicsPawnWrapper : ExtendedGraphicsPawnWrapper
{
    // Backing fields for all overridden members
    public  PawnDrawParms                         drawParms;
    public  IEnumerable<Apparel>                  wornApparel;
    public  IEnumerable<ApparelProperties>        wornApparelProps;
    public  bool                                  visibleInBed;
    public  Gender                                gender;
    public  PawnPosture                           posture;
    public  RotStage?                             rotStage;
    public  RotDrawMode                           rotDrawMode;
    public  List<Hediff>                          hediffList;
    public  List<Trait>                           traitList;
    public  HediffSet                             hediffSet;
    public  IEnumerable<BackstoryDef>             backstories;
    public  TraitSet                              traits;
    public  bool                                  drafted;
    public  Job                                   curJob;
    public  bool                                  moving;
    public  LifeStageDef                          currentLifeStage;
    public  BodyTypeDef                           bodyType;
    public  HeadTypeDef                           headType;
    public  List<GeneDef>                         genes;
    public  ThingDef                              race;
    public  MutantDef                             mutant;
    public  CreepJoinerFormKindDef                creepJoiner;
    private BodyDef                               body;

    public DummyExtendedGraphicsPawnWrapper() : base()
    {
    }

    public override PawnDrawParms DrawParms => this.drawParms;

    public override bool CurrentLifeStageDefMatches(LifeStageDef lifeStageDef) =>
        lifeStageDef == this.currentLifeStage;

    public override IEnumerable<Apparel> GetWornApparel => this.wornApparel;

    public override IEnumerable<ApparelProperties> GetWornApparelProps()
    {
        IEnumerable<ApparelProperties> apparelProps = base.GetWornApparelProps().ToList();
        return apparelProps.Any() ? apparelProps : this.wornApparelProps;
    }

    public override bool VisibleInBed(bool noBed = true) => this.visibleInBed;

    public override BodyPartRecord GetAnyBodyPart(BodyPartDef part, string partLabel) =>
        body.AllParts.Find(bpr => IsBodyPart(bpr, part, partLabel));

    public override float GetNeed(NeedDef needDef, bool percentage) => 0;

    public override Gender GetGender() => this.gender;

    public override PawnPosture GetPosture() => this.posture;

    public override bool HasBodyType(BodyTypeDef bodyType) => bodyType == this.bodyType;

    public override bool HasHeadTypeNamed(HeadTypeDef headType) => headType == this.headType;

    public override RotStage? GetRotStage() => this.rotStage;

    public override RotDrawMode GetRotDrawMode() => this.rotDrawMode;

    public override List<Hediff> GetHediffList() => this.hediffList;

    public override List<Trait> GetTraitList() => this.traitList;

    public override HediffSet GetHediffSet() => this.hediffSet;

    public override IEnumerable<BackstoryDef> GetBackstories() => this.backstories;

    public override TraitSet GetTraits() => this.traits;

    public override bool Drafted => this.drafted;

    public override Job CurJob => this.curJob;

    public override bool Moving => this.moving;

    public override bool HasGene(GeneDef gene) => this.genes.Contains(gene);

    public override bool IsRace(ThingDef race) => race == this.race;

    public override bool IsMutant(MutantDef def) => def == this.mutant;

    public override bool IsCreepJoiner(CreepJoinerFormKindDef def) => def == this.creepJoiner;
}