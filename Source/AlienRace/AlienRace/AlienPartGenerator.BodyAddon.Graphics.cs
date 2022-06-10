namespace AlienRace;

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
    public class BodyAddonDamageGraphic : AbstractBodyAddonGraphic
    {
        public float damage;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.damage = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());
            this.path   = xmlRoot.InnerXml.Trim();
        }

        public override IEnumerator<IBodyAddonGraphic> GetSubGraphics(
            BodyAddonPawnWrapper pawn, string part) => Enumerable.Empty<IBodyAddonGraphic>().GetEnumerator();//there are no subgraphics for damage
        public override IEnumerator<IBodyAddonGraphic> GeneratePaths() => Enumerable.Empty<IBodyAddonGraphic>().GetEnumerator();
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
            foreach (BodyAddonHediffSeverityGraphic graphic in
                     this.severity?.Where(s => maxSeverityOfHediff >= s.severity) ??
                     Enumerable.Empty<BodyAddonHediffSeverityGraphic>()) yield return graphic;//run severity cycle first

            IEnumerator<IBodyAddonGraphic> genericSubGraphics = base.GetSubGraphics(pawn, part);//run rest of graphic cycles
            while (genericSubGraphics.MoveNext())
            {
                yield return genericSubGraphics.Current;//return the final current of genericsubgraphics, needed as genericsubgraphics just gets the subgraphic, not return it to the previous iteration
            }
        }
        
        public override IEnumerator<IBodyAddonGraphic> GeneratePaths()
        {
            foreach (IBodyAddonGraphic graphic in this.severity ?? Enumerable.Empty<IBodyAddonGraphic>()) yield return graphic;
            foreach (IBodyAddonGraphic graphic in this.hediffGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) yield return graphic;//cycle through each hediff graphic defined
            foreach (IBodyAddonGraphic graphic in this.backstoryGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) yield return graphic;//cycle through each backstory graphic defined
            foreach (IBodyAddonGraphic graphic in this.ageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) yield return graphic;//cycle through each lifestage graphic defined
            foreach (IBodyAddonGraphic graphic in this.damageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) yield return graphic;//cycle through each damage graphic defined
        }

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
            this.path     = xmlRoot.InnerXml.Trim();
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

            this.path = xmlRoot.FirstChild.Value;
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasBackStoryWithIdentifier(this.backstory);
    }
}