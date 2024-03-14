namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
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

    public abstract bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel);

    public virtual IEnumerable<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel) =>
        this.GetSubGraphics();

    public virtual IEnumerable<IExtendedGraphic> GetSubGraphics() => 
        this.extendedGraphics;


    private static readonly Dictionary<string, string> XML_CLASS_DICTIONARY = new()
                                                                            {
                                                                                {"hediffGraphics", $"{nameof(ConditionHediff)}"},
                                                                                {"backstoryGraphics", $"{nameof(ConditionBackstory)}"},
                                                                                {"ageGraphics", $"{nameof(ConditionAge)}"},
                                                                                {"damageGraphics", $"{nameof(ConditionDamage)}"},
                                                                                {"genderGraphics", $"{nameof(ConditionGender)}"},
                                                                                {"traitGraphics", $"{nameof(ConditionTrait)}"},
                                                                                {"bodytypeGraphics", $"{nameof(ConditionBodyType)}"},
                                                                                {"headtypeGraphics", $"{nameof(ConditionHeadType)}"},
                                                                                {"geneGraphics", $"{nameof(ConditionGene)}"},
                                                                                {"raceGraphics", $"{nameof(ConditionRace)}"}
                                                                            };

    [UsedImplicitly]
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        foreach (XmlNode childNode in xmlRoot.ChildNodes)
        {
            //Log.Message("checking: " + childNode.Name);
            if (XML_CLASS_DICTIONARY.TryGetValue(childNode.Name, out string classTag))
            {
                //Log.Message("Original: " + childNode.OuterXml);
                foreach (XmlNode graphicNode in childNode.ChildNodes)
                {
                    XmlAttribute attribute = xmlRoot.OwnerDocument!.CreateAttribute("Class");
                    attribute.Value = typeof(AlienPartGenerator.ExtendedConditionGraphic).FullName;
                    graphicNode.Attributes!.SetNamedItem(attribute);
                    XmlAttribute attribute2 = xmlRoot.OwnerDocument!.CreateAttribute("For");
                    attribute2.Value = graphicNode.Name;
                    graphicNode.Attributes!.SetNamedItem(attribute2);

                    CachedData.xmlElementName(graphicNode as XmlElement) = CachedData.xmlDocumentAddName(childNode.OwnerDocument, string.Empty, classTag, string.Empty, null);
                }

                CachedData.xmlElementName(childNode as XmlElement) = CachedData.xmlDocumentAddName(childNode.OwnerDocument, string.Empty, nameof(this.extendedGraphics), string.Empty, null);

                //Log.Message("Adjusting: " + childNode.OuterXml);
            }
            else if(childNode.Name.Equals(nameof(this.extendedGraphics)))
            {
                foreach (XmlNode graphicNode in childNode.ChildNodes)
                    if (graphicNode.Attributes!["Class"] == null)
                    {
                        XmlAttribute attribute = xmlRoot.OwnerDocument!.CreateAttribute("Class");
                        attribute.Value = typeof(AlienPartGenerator.ExtendedConditionGraphic).FullName;
                        graphicNode.Attributes!.SetNamedItem(attribute);
                    }
            }
        }

        if(this is AlienPartGenerator.BodyAddon)
            Log.Message(xmlRoot.OuterXml);
        this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
    }

    protected virtual void SetInstanceVariablesFromChildNodesOf(XmlNode xmlRootNode)
    {
        Utilities.SetInstanceVariablesFromChildNodesOf(xmlRootNode, this, []);

        // If the path has not been set just use the value contained by the root node
        // This caters for nodes containing _only_ a path i.e. <someNode>a/path/here</someNode> 
        if (this.path.NullOrEmpty())
            this.path = xmlRootNode.FirstChild.Value?.Trim();
    }
}
