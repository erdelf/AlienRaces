namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using System.Xml;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

public abstract class AbstractExtendedGraphic : IExtendedGraphic
{
    public string       path;
    public List<string> paths         = [];
    public List<string> pathsFallback = [];

    public bool usingFallback = false;

    public int       variantCount  = 0;
    public List<int> variantCounts = [];
    
    [LoadAlias("hediffGraphics")]
    [LoadAlias("backstoryGraphics")]
    [LoadAlias("ageGraphics")]
    [LoadAlias("damageGraphics")]
    [LoadAlias("genderGraphics")]
    [LoadAlias("traitGraphics")]
    [LoadAlias("bodytypeGraphics")]
    [LoadAlias("headtypeGraphics")]
    [LoadAlias("geneGraphics")]
    [LoadAlias("raceGraphics")]
    public List<AbstractExtendedGraphic> extendedGraphics = [];

    public void Init()
    {
        if (this.paths.NullOrEmpty() && !this.path.NullOrEmpty())
            this.paths.Add(this.path);
        if (!this.paths.NullOrEmpty() && this.path.NullOrEmpty())
            this.path = this.paths[0];

        for (int i = 0; i < this.paths.Count; i++)
            this.variantCounts.Add(0);
    }

    public string GetPath()          => this.GetPathCount() > 0 ? this.GetPath(0) : this.path;
    public string GetPath(int index) => !this.usingFallback ? this.paths[index] : this.pathsFallback[index];

    public int GetPathCount() => !this.usingFallback ? this.paths.Count : this.pathsFallback.Count;

    public bool UseFallback()
    {
        this.usingFallback = true;

        this.variantCounts.Clear();
        for (int i = 0; i < this.pathsFallback.Count; i++)
            this.variantCounts.Add(0);

        return this.pathsFallback.Any();
    }

    public string GetPathFromVariant(ref int variantIndex, out bool zero)
    {
        zero = true;
        for (int index = 0; index < this.variantCounts.Count; index++)
        {
            int count = this.variantCounts[index];
            if (variantIndex < count)
            {
                zero = variantIndex == 0;
                return this.GetPath(index);
            }

            variantIndex -= count;
        }
        return this.path;
    }

    public int GetVariantCount()       => this.variantCount;
    public int GetVariantCount(int index)       => this.variantCounts[index];
    public int IncrementVariantCount() => this.IncrementVariantCount(0);
    public int IncrementVariantCount(int index)
    {
        this.variantCount++;
        return this.variantCounts[index]++;
    }

    public abstract bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel);

    public virtual IEnumerable<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel) =>
        this.GetSubGraphics();

    public virtual IEnumerable<IExtendedGraphic> GetSubGraphics() => 
        this.extendedGraphics;


    private static readonly Dictionary<string, string> XML_CLASS_DICTIONARY = new()
                                                                            {
                                                                                {"hediffGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedHediffGraphic)}"},
                                                                                {"backstoryGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedBackstoryGraphic)}"},
                                                                                {"ageGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedAgeGraphic)}"},
                                                                                {"damageGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedDamageGraphic)}"},
                                                                                {"genderGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedGenderGraphic)}"},
                                                                                {"traitGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedTraitGraphic)}"},
                                                                                {"bodytypeGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedBodytypeGraphic)}"},
                                                                                {"headtypeGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedHeadtypeGraphic)}"},
                                                                                {"geneGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedGeneGraphic)}"},
                                                                                {"raceGraphics", $"{nameof(AlienPartGenerator)}.{nameof(AlienPartGenerator.ExtendedRaceGraphic)}"}
                                                                            };

    [UsedImplicitly]
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {

        foreach (XmlNode childNode in xmlRoot.ChildNodes)
        {
            if(XML_CLASS_DICTIONARY.TryGetValue(childNode.Name, out string classTag))
            {
                XmlAttribute attribute = xmlRoot.OwnerDocument!.CreateAttribute("Class");
                attribute.Value = classTag;
                childNode.Attributes!.SetNamedItem(attribute);
            }
        }
        
        this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
    }

    protected virtual void SetInstanceVariablesFromChildNodesOf(XmlNode xmlRootNode) =>
        this.SetInstanceVariablesFromChildNodesOf(xmlRootNode, []);

    protected virtual void SetInstanceVariablesFromChildNodesOf(XmlNode xmlRootNode, HashSet<string> excludedFieldNames)
    {
        Traverse traverse = Traverse.Create(this);
        foreach (XmlNode xmlNode in xmlRootNode.ChildNodes)
            if (!excludedFieldNames.Contains(xmlNode.Name))
                this.SetFieldFromXmlNode(traverse, xmlNode);

        // If the path has not been set just use the value contained by the root node
        // This caters for nodes containing _only_ a path i.e. <someNode>a/path/here</someNode> 
        if (this.path.NullOrEmpty()) 
            this.path = xmlRootNode.FirstChild.Value?.Trim();
    }
    
    protected virtual void SetFieldFromXmlNode(Traverse field, XmlNode xmlNode)
    {
        Utilities.SetFieldFromXmlNode(field, xmlNode, this, xmlNode.Name);
    }
}
