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
    private Pawn WrappedPawn { get; set; }

    public ExtendedGraphicsPawnWrapper(Pawn pawn) => this.WrappedPawn = pawn;

    public ExtendedGraphicsPawnWrapper()
    {
    }

    //backstory isApplicable
    public virtual bool HasBackStory(BackstoryDef backstory) =>
        this.GetBackstories().Contains(backstory); //matches pawn backstory with input

    private bool IsHediffOfDefAndPart(Hediff hediff, HediffDef hediffDef, BodyPartDef part, string partLabel) => //checks if specific hediff is on given part or no part
        hediff.def == hediffDef && (hediff.Part == null || this.IsBodyPart(hediff.Part, part, partLabel));

    public virtual IEnumerable<float> SeverityOfHediffsOnPart(HediffDef hediffDef, BodyPartDef part, string partLabel) =>
        this.GetHediffList()
         .Where(h => IsHediffOfDefAndPart(h, hediffDef, part, partLabel))
         .Select(h => h.Severity);

    //hediff isApplicable
    public virtual bool HasHediffOfDefAndPart(HediffDef hediffDef, BodyPartDef part, string partLabel) => 
        this.GetHediffList().Any(h => this.IsHediffOfDefAndPart(h, hediffDef, part, partLabel)); //compares pawn hediffs to specified hediff def and part

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
                    capitalisedTrait == t.LabelCap || capitalisedTrait == t.GetLabelCapFor(this.WrappedPawn) || capitalisedTrait == t.untranslatedLabel);
    }

    public virtual bool HasApparelGraphics() =>
        !this.WrappedPawn.Drawer?.renderer?.graphics?.apparelGraphics?.NullOrEmpty() ?? false;

    public virtual IEnumerable<ApparelProperties> GetWornApparel() =>
        this.WrappedPawn.apparel?.WornApparel?.Select(ap => ap.def.apparel) ?? Enumerable.Empty<ApparelProperties>();

    public virtual bool VisibleInBed() => this.WrappedPawn.CurrentBed()?.def?.building?.bed_showSleeperBody ?? true;

    public virtual bool HasBackstory(BackstoryDef backstory) =>
        this.WrappedPawn.story?.AllBackstories?.Contains(backstory) ?? false;

    public virtual bool HasNamedBodyPart(BodyPartDef part, string partLabel) =>
        (part == null && partLabel.NullOrEmpty()) || this.GetBodyPart(part, partLabel) != null;

    public virtual BodyPartRecord GetBodyPart(BodyPartDef part, string partLabel) =>
        this.GetHediffSet().GetNotMissingParts()?.FirstOrDefault(bpr => IsBodyPart(bpr, part, partLabel));

    public virtual bool IsBodyPart(BodyPartRecord bpr, BodyPartDef part, string partLabel) =>
        (partLabel.NullOrEmpty() || bpr.untranslatedCustomLabel == partLabel) && (part == null || bpr.def == part);

    public virtual Gender GetGender() => this.WrappedPawn.gender;

    public virtual PawnPosture GetPosture() => this.WrappedPawn.GetPosture();

    public virtual bool HasBodyType(BodyTypeDef bodyType) =>
        this.WrappedPawn.story.bodyType == bodyType;

    public virtual bool HasHeadTypeNamed(HeadTypeDef headType) =>
        this.WrappedPawn.story.headType == headType;

    public virtual RotStage? GetRotStage() => this.WrappedPawn.Corpse?.GetRotStage();

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
}