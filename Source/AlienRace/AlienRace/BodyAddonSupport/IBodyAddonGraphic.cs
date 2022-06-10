namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;

public interface IBodyAddonGraphic
{
    public string GetPath();
    public int    GetVariantCount();
    public int    IncrementVariantCount();

    public IEnumerator<IBodyAddonGraphic> GetSubGraphics(BodyAddonPawnWrapper pawn, string part);//generic get subgraphic
    public bool                           IsApplicable(BodyAddonPawnWrapper   pawn, string part);//generic is applicable check
    public IEnumerator<IBodyAddonGraphic> GeneratePaths();
}
