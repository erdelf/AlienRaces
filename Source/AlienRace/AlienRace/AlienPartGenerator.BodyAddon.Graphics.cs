namespace AlienRace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BodyAddonSupport;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

public partial class AlienPartGenerator
{
    public class BodyAddonDamageGraphic : GenericBodyAddonGraphic
    {
        public float damage;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.damage = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());

            this.path = xmlRoot.FirstChild.Value?.Trim();

            Traverse traverse = Traverse.Create(this);
            foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
            {
                Traverse field = traverse.Field(xmlRootChildNode.Name);
                if (field.FieldExists())
                    field.SetValue(field.GetValueType().IsGenericType
                                       ? DirectXmlToObject
                                       .GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, arg2: false)
                                       : xmlRootChildNode.InnerXml.Trim());
            }
        }
        
        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasHediffOnPartBelowHealthThreshold(part, this.damage);
    }

    public class BodyAddonAgeGraphic : GenericBodyAddonGraphic
    {
        public LifeStageDef age;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlAttribute mayRequire = xmlRoot.Attributes?[name: "MayRequire"];
            int          index      = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.age),
                                                                xmlRoot.Name.Substring(index,
                                                                    xmlRoot.Name.Length - index),
                                                                mayRequire?.Value.ToLower());

            this.path = xmlRoot.FirstChild.Value?.Trim();

            Traverse traverse = Traverse.Create(this);
            foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
            {
                Traverse field = traverse.Field(xmlRootChildNode.Name);
                if (field.FieldExists())
                    field.SetValue(field.GetValueType().IsGenericType
                                       ? DirectXmlToObject
                                       .GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, arg2: false)
                                       : xmlRootChildNode.InnerXml.Trim());
            }
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.CurrentLifeStageDef == this.age;
    }

    public class BodyAddonHediffGraphic : GenericBodyAddonGraphic
    {
        public HediffDef                            hediff;
        public List<BodyAddonHediffSeverityGraphic> severity;

        public override IEnumerator<IBodyAddonGraphic> GetSubGraphics(BodyAddonPawnWrapper pawn, string part)
        {
            float maxSeverityOfHediff = pawn.SeverityOfHediffsOnPart(this.hediff, part).Max();
            IEnumerator<IBodyAddonGraphic> genericSubGraphics = base.GetSubGraphics(pawn, part); //run rest of graphic cycles
            while (genericSubGraphics.MoveNext())
            {
                IBodyAddonGraphic current = genericSubGraphics.Current;
                
                // check if graphic is valid to return applying type specific requirements
                bool isValid = current switch
                {
                    BodyAddonHediffSeverityGraphic severityGraphic => maxSeverityOfHediff >= severityGraphic.severity,
                    _ => true
                };
                
                //return each of the generic subgraphics lazily applying any type specific requirments
                if (isValid) yield return current;
            }
        }
        
        public override IEnumerable<IBodyAddonGraphic> GetSubGraphicsOfPriority(BodyAddonPrioritization priority) => priority switch
        {
            BodyAddonPrioritization.Severity => this.severity ?? Enumerable.Empty<IBodyAddonGraphic>(),
            _ => base.GetSubGraphicsOfPriority(priority)
        };

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasHediffOfDefAndPart(this.hediff, part);

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlAttribute mayRequire = xmlRoot.Attributes?[name: "MayRequire"];
            int          index      = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.hediff),
                                                                xmlRoot.Name.Substring(index,
                                                                    xmlRoot.Name.Length - index),
                                                                mayRequire?.Value.ToLower());

            this.path = xmlRoot.FirstChild.Value?.Trim();

            Traverse traverse = Traverse.Create(this);
            foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
            {
                Traverse field = traverse.Field(xmlRootChildNode.Name);
                if (field.FieldExists())
                    field.SetValue(field.GetValueType().IsGenericType
                                       ? DirectXmlToObject
                                       .GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, arg2: false)
                                       : xmlRootChildNode.InnerXml.Trim());
            }
        }
    }

    public class BodyAddonHediffSeverityGraphic : GenericBodyAddonGraphic
    {
        public float severity;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.severity = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());

            this.path = xmlRoot.FirstChild.Value?.Trim();

            Traverse traverse = Traverse.Create(this);
            foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
            {
                Traverse field = traverse.Field(xmlRootChildNode.Name);
                if (field.FieldExists())
                    field.SetValue(field.GetValueType().IsGenericType
                                       ? DirectXmlToObject
                                       .GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, arg2: false)
                                       : xmlRootChildNode.InnerXml.Trim());
            }
        }

        // In isolation severity graphics must always be considered applicable because we don't have the hediff
        // As severityGraphics are only valid nested It is expected that the list will only contain applicable instances. 
        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) => true;
    }

    public class BodyAddonBackstoryGraphic : GenericBodyAddonGraphic
    {
        public string backstory;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.backstory = xmlRoot.Name;

            this.path = xmlRoot.FirstChild.Value?.Trim();

            Traverse traverse = Traverse.Create(this);
            foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
            {
                Traverse field = traverse.Field(xmlRootChildNode.Name);
                if (field.FieldExists())
                    field.SetValue(field.GetValueType().IsGenericType
                                       ? DirectXmlToObject
                                       .GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, arg2: false)
                                       : xmlRootChildNode.InnerXml.Trim());
            }
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasBackStoryWithIdentifier(this.backstory);
    }
}