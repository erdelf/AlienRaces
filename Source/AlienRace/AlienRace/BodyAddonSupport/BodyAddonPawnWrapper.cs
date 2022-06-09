namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

public class BodyAddonPawnWrapper
{
    private Pawn WrappedPawn { get; set; }

    public BodyAddonPawnWrapper(Pawn pawn) => this.WrappedPawn = pawn;

    public BodyAddonPawnWrapper() {}

    public virtual bool HasBackStoryWithIdentifier(string backstoryId) =>
        this.WrappedPawn.story.AllBackstories.Any(bs => bs.identifier == backstoryId);

    private bool IsHediffOfDefAndPart(Hediff hediff, HediffDef hediffDef, string part) =>
        hediff.def == hediffDef &&
        (hediff.Part == null                         ||
         part.NullOrEmpty()                          ||
         hediff.Part.untranslatedCustomLabel == part ||
         hediff.Part.def.defName             == part);

    public virtual IEnumerable<float> SeverityOfHediffsOnPart(HediffDef hediffDef, string part) =>
        this.WrappedPawn.health.hediffSet.hediffs
         .Where(h => IsHediffOfDefAndPart(h, hediffDef, part))
         .Select(h => h.Severity);

    public virtual bool HasHediffOfDefAndPart(HediffDef hediffDef, string part) => this.WrappedPawn.health.hediffSet
    .hediffs
    .Any(h => IsHediffOfDefAndPart(h, hediffDef, part));

    public virtual LifeStageDef CurrentLifeStageDef => this.WrappedPawn.ageTracker.CurLifeStage;

    public virtual bool HasHediffOnPartBelowHealthThreshold(string part, float healthThreshold)
    {
        // look for part where a given hediff has a part matching defined part
        return this.WrappedPawn.health.hediffSet.hediffs
                .Where(predicate: h => h.Part.untranslatedCustomLabel == part || h.Part.def.defName == part)
                //check if part health is less than health texture limit, needs to config ascending
                .Any(h => healthThreshold >= this.WrappedPawn.health.hediffSet.GetPartHealth(h.Part));
    }
    
    public virtual bool HasApparel() => !WrappedPawn.Drawer.renderer.graphics.apparelGraphics.NullOrEmpty();

    public virtual IEnumerable<ApparelProperties> GetWornApparel() => WrappedPawn.apparel.WornApparel.Select(ap => ap.def.apparel);
    public virtual bool VisibleInBed() => WrappedPawn.CurrentBed()?.def.building.bed_showSleeperBody ?? true;
    public virtual bool HasBackstory(string backstoryId) =>
        WrappedPawn.story.AllBackstories.Any(b => b.identifier == backstoryId);

    public virtual bool HasNamedBodyPart(string part) =>
        WrappedPawn.health.hediffSet.GetNotMissingParts()
         .Any(bpr => bpr.untranslatedCustomLabel == part || bpr.def.defName == part);

    public virtual Gender GetGender() => WrappedPawn.gender;
    public virtual PawnPosture GetPosture() => WrappedPawn.GetPosture();
    public virtual string GetBodyTypeName() => WrappedPawn.story.bodyType.ToString();
    public virtual RotStage? GetRotStage() => WrappedPawn.Corpse?.GetRotStage();

}
