namespace AlienRace.ExtendedGraphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HarmonyLib;
using Verse;

public abstract class AbstractExtendedGraphic : IExtendedGraphic
{
    public string path;
    public int variantCount = 0;
    
    // Not unused, users can define their own order in XML which takes priority.
    
    #pragma warning disable CS0649
    private List<AlienPartGenerator.ExtendedGraphicsPrioritization>   prioritization;
    #pragma warning restore CS0649

    public  List<AlienPartGenerator.ExtendedHediffGraphic>    hediffGraphics;
    public  List<AlienPartGenerator.ExtendedBackstoryGraphic> backstoryGraphics;
    public  List<AlienPartGenerator.ExtendedAgeGraphic>       ageGraphics;
    public  List<AlienPartGenerator.ExtendedDamageGraphic>    damageGraphics;
    public List<AlienPartGenerator.ExtendedGenderGraphic>     genderGraphics;
    public List<AlienPartGenerator.ExtendedTraitGraphic>      traitGraphics;
    public List<AlienPartGenerator.ExtendedBodytypeGraphic>   bodytypeGraphics;
    public List<AlienPartGenerator.ExtendedHeadtypeGraphic>   headtypeGraphics;


    protected List<AlienPartGenerator.ExtendedGraphicsPrioritization> Prioritization =>
        this.prioritization ?? GetPrioritiesByDeclarationOrder().ToList();

    public string GetPath() => this.path;

    public int GetVariantCount() => this.variantCount;
    public int IncrementVariantCount() => this.variantCount++;

    public abstract bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel);

    private static IEnumerable<AlienPartGenerator.ExtendedGraphicsPrioritization> GetPrioritiesByDeclarationOrder() => 
        Enum.GetValues(typeof(AlienPartGenerator.ExtendedGraphicsPrioritization)).Cast<AlienPartGenerator.ExtendedGraphicsPrioritization>();

    public virtual IEnumerator<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel) =>
        this.GetSubGraphics();

    /**
     * Get an enumerator of all the sub-graphics, unrestricted in any way.
     * Used to aid initialization in the AlienPartGenerator
     */
    public virtual IEnumerator<IExtendedGraphic> GetSubGraphics() => 
        this.Prioritization.SelectMany(this.GetSubGraphicsOfPriority).GetEnumerator();

    public virtual IEnumerable<IExtendedGraphic> GetSubGraphicsOfPriority(AlienPartGenerator.ExtendedGraphicsPrioritization priority) => priority switch
    {
        AlienPartGenerator.ExtendedGraphicsPrioritization.Headtype => this.headtypeGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Bodytype => this.bodytypeGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Trait => this.traitGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Gender => this.genderGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Backstory => this.backstoryGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Hediff => this.hediffGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Age => this.ageGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Damage => this.damageGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        _ => Enumerable.Empty<IExtendedGraphic>()
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
        if (this.path.NullOrEmpty()) 
            this.path = xmlRootNode.FirstChild.Value?.Trim();
    }
    
    protected virtual void SetFieldFromXmlNode(Traverse field, XmlNode xmlNode)
    {
        if (!field.FieldExists()) 
            return;
        field.SetValue(field.GetValueType().IsGenericType ? 
                           DirectXmlToObject.GetObjectFromXmlMethod(field.GetValueType())(xmlNode, false) : 
                           ParseHelper.FromString(xmlNode.InnerXml.Trim(), field.GetValueType()));
    }
}
