namespace AlienRace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BodyAddonSupport;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

public partial class AlienPartGenerator
{
    //Damage graphics
    public class BodyAddonDamageGraphic : AbstractBodyAddonGraphic
    {
        public float damage;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.damage = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);

        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.IsPartBelowHealthThreshold(part, this.damage);
    }

    //Age Graphics
    public class BodyAddonAgeGraphic : AbstractBodyAddonGraphic
    {
        public LifeStageDef age;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlAttribute mayRequire = xmlRoot.Attributes?[name: "MayRequire"];
            int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
            const string ageFieldName = nameof(this.age);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, ageFieldName,
                                                                xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index),
                                                                mayRequire?.Value.ToLower());

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot, new HashSet<string>{ageFieldName});

        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.CurrentLifeStageDefMatches(this.age);
    }

    //Hediff graphics
    public class BodyAddonHediffGraphic : AbstractBodyAddonGraphic
    {
        public HediffDef hediff;
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

                //return each of the generic subgraphics lazily applying any type specific requirements
                if (isValid) yield return current;
            }
        }

        public override IEnumerable<IBodyAddonGraphic> GetSubGraphicsOfPriority(BodyAddonPrioritization priority) =>
            priority switch
            {
                BodyAddonPrioritization.Severity => this.severity ?? Enumerable.Empty<IBodyAddonGraphic>(),
                _ => base.GetSubGraphicsOfPriority(priority)
            };

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasHediffOfDefAndPart(this.hediff, part);

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlAttribute mayRequire      = xmlRoot.Attributes?[name: "MayRequire"];
            int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
            const string hediffFieldName = nameof(this.hediff);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, hediffFieldName,
                                                                xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index),
                                                                mayRequire?.Value.ToLower());

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot, new HashSet<string> { hediffFieldName });

        }
    }

    //Severity Graphics
    public class BodyAddonHediffSeverityGraphic : AbstractBodyAddonGraphic
    {
        public float severity;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.severity = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);

        }

        // In isolation severity graphics must always be considered applicable because we don't have the hediff
        // As severityGraphics are only valid nested It is expected that the list will only contain applicable instances. 
        // the specific severity amount check is done above in the hediff level
        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) => true;
    }

    //Backstory Graphics
    public class BodyAddonBackstoryGraphic : AbstractBodyAddonGraphic
    {
        public string backstory;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.backstory = xmlRoot.Name;

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);

        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasBackStoryWithIdentifier(this.backstory);
    }

    //Gender Graphics
    public class BodyAddonGenderGraphic : AbstractBodyAddonGraphic
    {
        public Gender gender;
        
        private bool genderIsValid;

        public Gender? GetGender => this.genderIsValid ? this.gender : null;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string genderString = xmlRoot.Name;
            if (!(this.genderIsValid = Enum.TryParse(genderString, out this.gender))) Debug.LogWarning($"Unable to parse {genderString} as Gender");
            
            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            this.genderIsValid && pawn.GetGender() == this.gender;
    }

    //Trait Graphics
    public class BodyAddonTraitGraphic : AbstractBodyAddonGraphic
    {
        public string trait;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.trait = xmlRoot.Name.Replace('_', ' ');

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasTraitWithIdentifier(this.trait);
    }

    //Bodytype Graphics
    public class BodyAddonBodytypeGraphic : AbstractBodyAddonGraphic
    {
        public string bodytype;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.bodytype = xmlRoot.Name;

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasBodyTypeNamed(this.bodytype);
    }

    //Crowntype Graphics
    public class BodyAddonCrowntypeGraphic : AbstractBodyAddonGraphic
    {
        public string Crowntype;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.Crowntype = xmlRoot.Name;

            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) =>
            pawn.HasCrownTypeNamed(this.Crowntype);
    }
}