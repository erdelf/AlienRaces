﻿namespace AlienRace.ExtendedGraphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HarmonyLib;
using Verse;

public abstract class AbstractExtendedGraphic : IExtendedGraphic
{
    public string       path;
    public List<string> paths         = new();
    public List<string> pathsFallback = new();

    public bool usingFallback = false;

    public int       variantCount  = 0;
    public List<int> variantCounts = new();

    // Not unused, users can define their own order in XML which takes priority.
    
    #pragma warning disable CS0649
    private List<AlienPartGenerator.ExtendedGraphicsPrioritization>   prioritization;
    #pragma warning restore CS0649

    public List<AlienPartGenerator.ExtendedHediffGraphic>    hediffGraphics    = new();
    public List<AlienPartGenerator.ExtendedBackstoryGraphic> backstoryGraphics = new();
    public List<AlienPartGenerator.ExtendedAgeGraphic>       ageGraphics       = new();
    public List<AlienPartGenerator.ExtendedDamageGraphic>    damageGraphics    = new();
    public List<AlienPartGenerator.ExtendedGenderGraphic>    genderGraphics    = new();
    public List<AlienPartGenerator.ExtendedTraitGraphic>     traitGraphics     = new();
    public List<AlienPartGenerator.ExtendedBodytypeGraphic>  bodytypeGraphics  = new();
    public List<AlienPartGenerator.ExtendedHeadtypeGraphic>  headtypeGraphics  = new();
    public List<AlienPartGenerator.ExtendedGeneGraphic>      geneGraphics      = new();
    public List<AlienPartGenerator.ExtendedRaceGraphic>      raceGraphics      = new();


    protected List<AlienPartGenerator.ExtendedGraphicsPrioritization> Prioritization =>
        this.prioritization ?? GetPrioritiesByDeclarationOrder().ToList();

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
        AlienPartGenerator.ExtendedGraphicsPrioritization.Headtype => this.headtypeGraphics   ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Bodytype => this.bodytypeGraphics   ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Trait => this.traitGraphics         ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Gender => this.genderGraphics       ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Backstory => this.backstoryGraphics ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Hediff => this.hediffGraphics       ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Age => this.ageGraphics             ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Damage => this.damageGraphics       ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Gene => this.geneGraphics           ?? Enumerable.Empty<IExtendedGraphic>(),
        AlienPartGenerator.ExtendedGraphicsPrioritization.Race => this.raceGraphics           ?? Enumerable.Empty<IExtendedGraphic>(),
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
