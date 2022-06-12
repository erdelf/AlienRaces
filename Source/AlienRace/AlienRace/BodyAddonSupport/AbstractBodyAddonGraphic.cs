namespace AlienRace.BodyAddonSupport;

using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AbstractBodyAddonGraphic : IBodyAddonGraphic
{
    public string path;
    public int    variantCount;
    public string GetPath() => this.path;

    public int GetVariantCount()       => this.variantCount;
    public int IncrementVariantCount() => this.variantCount++;

    public abstract IEnumerator<IBodyAddonGraphic> GetSubGraphics(
        BodyAddonPawnWrapper pawn, string part);

    public abstract bool IsApplicable(BodyAddonPawnWrapper pawn, string part);

    public abstract IEnumerable<IBodyAddonGraphic> GetSubGraphicsOfPriority(
        AlienPartGenerator.BodyAddonPrioritization priority);

    public abstract IEnumerator<IBodyAddonGraphic> GetSubGraphics();

    protected IEnumerable<AlienPartGenerator.BodyAddonPrioritization> GetPriorities() =>
        Enum.GetValues(typeof(AlienPartGenerator.BodyAddonPrioritization))
         .Cast<AlienPartGenerator.BodyAddonPrioritization>();
}