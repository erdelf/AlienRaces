namespace AlienRace;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ExtendedGraphics;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

public partial class AlienPartGenerator
{
    public class ExtendedConditionGraphic : AbstractExtendedGraphic
    {
        public List<Condition> conditions = [];

        [UsedImplicitly]
        public new void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (Condition.XmlNameParseKeys.TryGetValue(xmlRoot.LocalName, out string classTagOrigin))
            {
                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml($"<conditions><{classTagOrigin}>{xmlRoot.Attributes!["For"].Value}</{classTagOrigin}></conditions>");
                xmlRoot.AppendChild(xmlRoot.OwnerDocument!.ImportNode(xmlDoc, true));
            }

            foreach (XmlNode childNode in xmlRoot.ChildNodes)
            {
                if (childNode.Name.Equals(nameof(this.conditions)))
                {
                    foreach (XmlNode node in childNode.ChildNodes)
                    {
                        if (Condition.XmlNameParseKeys.TryGetValue(node.Name, out string classTag))
                        {
                            XmlAttribute attribute = xmlRoot.OwnerDocument!.CreateAttribute("Class");
                            attribute.Value = classTag;
                            node.Attributes!.SetNamedItem(attribute);
                        }
                    }
                }
            }
            Log.Message(xmlRoot.OuterXml);
            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel)
        {
            BodyPartDef bodyPart      = part;
            string      bodyPartLabel = partLabel;

            bool applicable = this.conditions.TrueForAll(c => c.Satisfied(pawn, ref bodyPart, ref bodyPartLabel));

            if (applicable)
            {
                part      = bodyPart;
                partLabel = bodyPartLabel;
            }

            return applicable;
        }
    }

    //Damage graphics
    public class ExtendedDamageGraphic : AbstractExtendedGraphic
    {
        public float damage;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.damage = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.IsPartBelowHealthThreshold(part, partLabel, this.damage);
    }

    //Age Graphics
    public class ExtendedAgeGraphic : AbstractExtendedGraphic
    {
        [XmlIgnore]
        public LifeStageDef age;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlAttribute mayRequire = xmlRoot.Attributes?["MayRequire"];
            int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
            const string ageFieldName = nameof(this.age);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, ageFieldName,
                                                                xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index),
                                                                mayRequire?.Value.ToLower());

            Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, [ageFieldName]);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.CurrentLifeStageDefMatches(this.age);
    }

    //Hediff graphics
    public class ExtendedHediffGraphic : AbstractExtendedGraphic
    {
        [XmlIgnore]
        public HediffDef hediff;
        public List<ExtendedHediffSeverityGraphic> severity = [];

        public override IEnumerable<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel)
        {
            float maxSeverityOfHediff = pawn.SeverityOfHediffsOnPart(this.hediff, part, partLabel).Max();
            IEnumerable<IExtendedGraphic> genericSubGraphics = base.GetSubGraphics(pawn, part, partLabel); //run rest of graphic cycles

            foreach (IExtendedGraphic current in genericSubGraphics)
            {
                bool isValid = current switch
                {
                    ExtendedHediffSeverityGraphic severityGraphic => maxSeverityOfHediff >= severityGraphic.severity,
                    _ => true
                };

                //return each of the generic subgraphics lazily applying any type specific requirements
                if (isValid) 
                    yield return current;
            }
        }

        public override IEnumerable<IExtendedGraphic> GetSubGraphics()
        {
            foreach (ExtendedHediffSeverityGraphic severityGraphic in this.severity) 
                yield return severityGraphic;

            IEnumerable<IExtendedGraphic> subGraphics = base.GetSubGraphics();
            foreach (IExtendedGraphic graphic in subGraphics) 
                yield return graphic;
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.HasHediffOfDefAndPart(this.hediff, part, partLabel);

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlAttribute mayRequire      = xmlRoot.Attributes?["MayRequire"];
            int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
            const string hediffFieldName = nameof(this.hediff);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, hediffFieldName,
                                                                xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index),
                                                                mayRequire?.Value.ToLower());

            Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, [hediffFieldName]);
        }
    }

    //Severity Graphics
    public class ExtendedHediffSeverityGraphic : AbstractExtendedGraphic
    {
        public float severity;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.severity = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim(), CultureInfo.InvariantCulture);
            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);

        }

        // In isolation severity graphics must always be considered applicable because we don't have the hediff
        // As severityGraphics are only valid nested It is expected that the list will only contain applicable instances. 
        // the specific severity amount check is done above in the hediff level
        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => true;
    }

    //Backstory Graphics
    public class ExtendedBackstoryGraphic : AbstractExtendedGraphic
    {
        [XmlIgnore]
        public BackstoryDef backstory;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.backstory), xmlRoot.Name);
            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);

        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.HasBackStory(this.backstory);
    }

    //Gender Graphics
    public class ExtendedGenderGraphic : AbstractExtendedGraphic
    {
        public Gender gender;
        
        private bool genderIsValid = true;

        public Gender? GetGender => this.genderIsValid ? this.gender : null;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string genderString = xmlRoot.Name;
            if (!(this.genderIsValid = Enum.TryParse(genderString.ToLowerInvariant().CapitalizeFirst(), out this.gender)))
                Debug.LogWarning($"Unable to parse {genderString} as Gender");

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            this.genderIsValid && pawn.GetGender() == this.gender;
    }

    //Trait Graphics
    public class ExtendedTraitGraphic : AbstractExtendedGraphic
    {
        public string trait;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.trait = xmlRoot.Name.Replace('_', ' ');

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.HasTraitWithIdentifier(this.trait);
    }

    //Bodytype Graphics
    public class ExtendedBodytypeGraphic : AbstractExtendedGraphic
    {
        public BodyTypeDef bodytype;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.bodytype), xmlRoot.Name);

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.HasBodyType(this.bodytype);
    }

    //Headtype Graphics
    public class ExtendedHeadtypeGraphic : AbstractExtendedGraphic
    {
        public HeadTypeDef headType;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.headType), xmlRoot.Name);

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.HasHeadTypeNamed(this.headType);
    }

    //Gene Graphics
    public class ExtendedGeneGraphic : AbstractExtendedGraphic
    {
        public GeneDef gene;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.gene), xmlRoot.Name);

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.HasGene(this.gene);
    }

    public class ExtendedRaceGraphic : AbstractExtendedGraphic
    {
        public ThingDef race;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.race), xmlRoot.Name);

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
            pawn.IsRace(this.race);
    }

    public class ExtendedMutantGraphic : AbstractExtendedGraphic
    {
        public MutantDef mutant;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.mutant), xmlRoot.Name);

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
            pawn.IsMutant(this.mutant);
    }

    public class ExtendedCreepJoinerFormGraphic : AbstractExtendedGraphic
    {
        public CreepJoinerFormKindDef form;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.form), xmlRoot.Name);

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
            pawn.IsCreepJoiner(this.form);
    }
}