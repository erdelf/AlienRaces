namespace AlienRace.BodyAddonSupport;

using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AbstractBodyAddonGraphic : IBodyAddonGraphic
{
    public string path;
    public int  variantCount;
    
    // Not unused, users can define their own order in XML which takes priority.
    private List<AlienPartGenerator.BodyAddonPrioritization>   prioritization;
    public  List<AlienPartGenerator.BodyAddonHediffGraphic>    hediffGraphics;
    public  List<AlienPartGenerator.BodyAddonBackstoryGraphic> backstoryGraphics;
    public  List<AlienPartGenerator.BodyAddonAgeGraphic>       ageGraphics;
    public  List<AlienPartGenerator.BodyAddonDamageGraphic>    damageGraphics;

    protected List<AlienPartGenerator.BodyAddonPrioritization> Prioritization =>
        this.prioritization ?? GetPrioritiesByDeclarationOrder().ToList();

    public string GetPath() => this.path;

    public int GetVariantCount() => this.variantCount;
    public int IncrementVariantCount() => this.variantCount++;

    public abstract bool IsApplicable(BodyAddonPawnWrapper pawn, string part);

    private static IEnumerable<AlienPartGenerator.BodyAddonPrioritization> GetPrioritiesByDeclarationOrder() => Enum
                                                                                                            .GetValues(typeof(AlienPartGenerator.BodyAddonPrioritization))
                                                                                                            .Cast<AlienPartGenerator.BodyAddonPrioritization>();

    public virtual IEnumerator<IBodyAddonGraphic> GetSubGraphics(
        BodyAddonPawnWrapper pawn, string part) =>
        this.GetSubGraphics();

    /**
     * Get an enumerator of all the sub-graphics, unrestricted in any way.
     * Used to aid initialisation in the AlienPartGenerator
     */
    public virtual IEnumerator<IBodyAddonGraphic> GetSubGraphics()
    {
        foreach (AlienPartGenerator.BodyAddonPrioritization priority in this.Prioritization)
        {
            foreach (IBodyAddonGraphic graphic in this.GetSubGraphicsOfPriority(priority)) yield return graphic;
        }
    }

    public virtual IEnumerable<IBodyAddonGraphic> GetSubGraphicsOfPriority(AlienPartGenerator.BodyAddonPrioritization priority) => priority switch
    {
        AlienPartGenerator.BodyAddonPrioritization.Backstory => this.backstoryGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Hediff => this.hediffGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Age => this.ageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Damage => this.damageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        _ => Enumerable.Empty<IBodyAddonGraphic>()
    };
}
