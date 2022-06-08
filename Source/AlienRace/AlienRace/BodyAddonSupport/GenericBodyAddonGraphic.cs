namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;
using System.Linq;

public abstract class GenericBodyAddonGraphic : AbstractBodyAddonGraphic
{
    public List<AlienPartGenerator.BodyAddonHediffGraphic>    hediffGraphics;
    public List<AlienPartGenerator.BodyAddonBackstoryGraphic> backstoryGraphics;
    public List<AlienPartGenerator.BodyAddonAgeGraphic>       ageGraphics;
    public List<AlienPartGenerator.BodyAddonDamageGraphic>    damageGraphics;

    public override IEnumerator<IBodyAddonGraphic> GetSubGraphics(
        BodyAddonPawnWrapper pawn, string part)
    {
        foreach (IBodyAddonGraphic graphic in this.backstoryGraphics ?? Enumerable.Empty<IBodyAddonGraphic>())  yield return graphic;
        foreach (IBodyAddonGraphic graphic in this.hediffGraphics ?? Enumerable.Empty<IBodyAddonGraphic>())  yield return graphic;
        foreach (IBodyAddonGraphic graphic in this.ageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>())  yield return graphic;
        foreach (IBodyAddonGraphic graphic in this.damageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>())  yield return graphic;
    }
}
