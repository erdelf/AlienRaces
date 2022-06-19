namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

/**
 * Encapsulates pawn access for the purpose of BodyAddon access.
 */
public class BodyAddonPawnWrapper
{
    private Pawn WrappedPawn { get; set; }

    public BodyAddonPawnWrapper(Pawn pawn) => this.WrappedPawn = pawn;

    public BodyAddonPawnWrapper()
    {
    }

    public virtual bool HasBackStoryWithIdentifier(string backstoryId) =>
        this.GetBackstories()
         .Any(bs => bs.identifier == backstoryId); //matches pawn backstory with input

    private static bool
        IsHediffOfDefAndPart(Hediff hediff, HediffDef hediffDef,
                             string part) => //checks if specific hediff is on given part or no part
        hediff.def == hediffDef &&
        (hediff.Part == null                         ||
         part.NullOrEmpty()                          ||
         hediff.Part.untranslatedCustomLabel == part ||
         hediff.Part.def.defName             == part);

    public virtual IEnumerable<float> SeverityOfHediffsOnPart(HediffDef hediffDef, string part) =>
        this.GetHediffList()
         .Where(h => IsHediffOfDefAndPart(h, hediffDef, part))
         .Select(h => h.Severity);

    public virtual bool HasHediffOfDefAndPart(HediffDef hediffDef, string part) => this
    .GetHediffList()                                     //get list of pawn hediffs
    .Any(h => IsHediffOfDefAndPart(h, hediffDef, part)); //compares pawn hediffs to specified hediff def and part

    //checks if lifestagedefs match
    public virtual bool CurrentLifeStageDefMatches(LifeStageDef lifeStageDef) =>
        this.WrappedPawn.ageTracker?.CurLifeStage?.Equals(lifeStageDef) ?? false;

    public virtual bool IsPartBelowHealthThreshold(string part, float healthThreshold)
    {
        // look for part where a given hediff has a part matching defined part
        return this.GetHediffList()
                .Select(hediff => hediff.Part)
                .Where(hediffPart => hediffPart != null)
                .Where(hediffPart => hediffPart.untranslatedCustomLabel == part || hediffPart.def?.defName == part)
                    //check if part health is less than health texture limit, needs to config ascending
                .Any(p => healthThreshold >= this.GetHediffSet().GetPartHealth(p));
    }

    public virtual bool HasApparelGraphics() =>
        !this.WrappedPawn.Drawer?.renderer?.graphics?.apparelGraphics?.NullOrEmpty() ?? false;

    public virtual IEnumerable<ApparelProperties> GetWornApparel() =>
        this.WrappedPawn.apparel?.WornApparel?.Select(ap => ap.def.apparel) ?? Enumerable.Empty<ApparelProperties>();

    public virtual bool VisibleInBed() => this.WrappedPawn.CurrentBed()?.def?.building?.bed_showSleeperBody ?? true;

    public virtual bool HasBackstory(string backstoryId) =>
        this.WrappedPawn.story?.AllBackstories?.Any(b => b.identifier == backstoryId) ?? false;

    public virtual bool HasNamedBodyPart(string part) =>
        this.GetHediffSet().GetNotMissingParts()
         ?.Any(bpr => bpr.untranslatedCustomLabel == part || bpr.def.defName == part) ?? false;

    public virtual Gender GetGender() => this.WrappedPawn.gender;

    public virtual PawnPosture GetPosture() => this.WrappedPawn.GetPosture();

    public virtual bool HasBodyTypeNamed(string bodyType) =>
        this.WrappedPawn.story?.bodyType?.ToString().EqualsIgnoreCase(bodyType) ?? false;

    public virtual RotStage? GetRotStage() => this.WrappedPawn.Corpse?.GetRotStage();

    public virtual List<Hediff> GetHediffList() => this.GetHediffSet().hediffs;

    /**
     * Helper to get a HediffSet or initialise one if absent
     */
    public virtual HediffSet GetHediffSet() => this.WrappedPawn.health?.hediffSet ?? new HediffSet(this.WrappedPawn);

    public virtual IEnumerable<Backstory> GetBackstories() =>
        this.WrappedPawn.story?.AllBackstories ?? Enumerable.Empty<Backstory>();
}