namespace AlienRace.BodyAddonSupport;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HarmonyLib;
using Verse;

public abstract class AbstractBodyAddonGraphic : IBodyAddonGraphic
{
    public string path;
    public int variantCount;
    
    // Not unused, users can define their own order in XML which takes priority.
    #pragma warning disable CS0649
    private List<AlienPartGenerator.BodyAddonPrioritization>   prioritization;
    #pragma warning restore CS0649
    
    public  List<AlienPartGenerator.BodyAddonHediffGraphic>    hediffGraphics;
    public  List<AlienPartGenerator.BodyAddonBackstoryGraphic> backstoryGraphics;
    public  List<AlienPartGenerator.BodyAddonAgeGraphic>       ageGraphics;
    public  List<AlienPartGenerator.BodyAddonDamageGraphic>    damageGraphics;
    public List<AlienPartGenerator.BodyAddonGenderGraphic>     genderGraphics;
    public List<AlienPartGenerator.BodyAddonTraitGraphic>      traitGraphics;
    public List<AlienPartGenerator.BodyAddonBodytypeGraphic>   bodytypeGraphics;
    public List<AlienPartGenerator.BodyAddonCrowntypeGraphic>  crowntypeGraphics;


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
        AlienPartGenerator.BodyAddonPrioritization.Crowntype => this.crowntypeGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Bodytype => this.bodytypeGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Trait => this.traitGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Gender => this.genderGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Backstory => this.backstoryGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Hediff => this.hediffGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Age => this.ageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        AlienPartGenerator.BodyAddonPrioritization.Damage => this.damageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>(),
        _ => Enumerable.Empty<IBodyAddonGraphic>()
    };

    protected virtual void SetInstanceVariablesFromChildNodesOf(XmlNode xmlRootNode) =>
        this.SetInstanceVariablesFromChildNodesOf(xmlRootNode, new HashSet<string>());

    protected virtual void SetInstanceVariablesFromChildNodesOf(XmlNode xmlRootNode, HashSet<string> excludedFieldNames)
    {
        Traverse traverse = Traverse.Create(this);
        foreach (XmlNode xmlNode in xmlRootNode.ChildNodes)
            if (!excludedFieldNames.Contains(xmlNode.Name))
                this.SetFieldFromXmlNode(traverse.Field(xmlNode.Name), xmlNode);

        // If the path has not been set just use the value contained by the root node
        // This caters for nodes containing _only_ a path i.e. <someNode>a/path/here</someNode> 
        if (this.path.NullOrEmpty()) this.path = xmlRootNode.FirstChild.Value?.Trim();
    }
    
    protected virtual void SetFieldFromXmlNode(Traverse field, XmlNode xmlNode)
    {
        if (!field.FieldExists()) return;
        Type type = field.GetValueType();
        field.SetValue(field.GetValueType().IsGenericType
                           ? DirectXmlToObject.GetObjectFromXmlMethod(field.GetValueType())(xmlNode, false)
                           : ParseHelper.FromString(xmlNode.InnerXml.Trim(), type));
    }
}
