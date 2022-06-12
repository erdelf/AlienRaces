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
        BodyAddonPawnWrapper pawn, string part) =>
        this.GetSubGraphics();

    /**
     * Get an enumerator of all the sub-graphics, unrestricted in any way.
     * Used to aid initialisation in the AlienPartGenerator
     */
    public override IEnumerator<IBodyAddonGraphic> GetSubGraphics()
    {
        foreach (AlienPartGenerator.BodyAddonPrioritization priority in this.Prioritization)
        {
            foreach (IBodyAddonGraphic graphic in this.GetSubGraphicsOfPriority(priority)) yield return graphic;
        }
    }

    public override IEnumerable<IBodyAddonGraphic> GetSubGraphicsOfPriority(AlienPartGenerator.BodyAddonPrioritization priority) => priority switch
    {
        AlienPartGenerator.BodyAddonPrioritization.Backstory => this.backstoryGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Hediff => this.hediffGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Age => this.ageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Damage => this.damageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        _ => Enumerable.Empty<IBodyAddonGraphic>()
    };
}
