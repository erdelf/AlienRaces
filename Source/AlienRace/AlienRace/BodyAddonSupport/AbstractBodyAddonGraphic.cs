namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;

public abstract class AbstractBodyAddonGraphic: IBodyAddonGraphic
{
    public string path;
    public int    variantCount;
    public string GetPath() => this.path;

    public int GetVariantCount()       => this.variantCount;
    public int IncrementVariantCount() => this.variantCount++;

    public abstract IEnumerator<IBodyAddonGraphic> GetSubGraphics(
        BodyAddonPawnWrapper pawn, string part);
            
    public abstract bool IsApplicable(BodyAddonPawnWrapper pawn, string part);
}