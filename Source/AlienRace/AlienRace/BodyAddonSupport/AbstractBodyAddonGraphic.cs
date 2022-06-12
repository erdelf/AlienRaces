namespace AlienRace.BodyAddonSupport;

using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AbstractBodyAddonGraphic : IBodyAddonGraphic
{
    public string path;
    public int  variantCount;
    
    // Not unused, users can define their own order in XML which takes priority.
    private List<AlienPartGenerator.BodyAddonPrioritization> prioritization;

    protected List<AlienPartGenerator.BodyAddonPrioritization> Prioritization =>
        this.prioritization ?? GetPrioritiesByDeclarationOrder().ToList();

    public string GetPath() => this.path;

    public int GetVariantCount() => this.variantCount;
    public int IncrementVariantCount() => this.variantCount++;

    public abstract IEnumerator<IBodyAddonGraphic> GetSubGraphics(
        BodyAddonPawnWrapper pawn, string part);

    public abstract bool IsApplicable(BodyAddonPawnWrapper pawn, string part);

    public abstract IEnumerable<IBodyAddonGraphic> GetSubGraphicsOfPriority(
        AlienPartGenerator.BodyAddonPrioritization priority);

    public abstract IEnumerator<IBodyAddonGraphic> GetSubGraphics();

    private static IEnumerable<AlienPartGenerator.BodyAddonPrioritization> GetPrioritiesByDeclarationOrder() => Enum
    .GetValues(typeof(AlienPartGenerator.BodyAddonPrioritization))
    .Cast<AlienPartGenerator.BodyAddonPrioritization>();
}
