namespace AlienRace.ExtendedGraphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using JetBrains.Annotations;
using System.Xml;
using HarmonyLib;

[StaticConstructorOnStartup]
public abstract class Condition
{
    public static Dictionary<string, string> XmlNameParseKeys;

    static Condition()
    {
        XmlNameParseKeys = [];
        foreach (Type type in typeof(Condition).AllSubclassesNonAbstract()) 
            XmlNameParseKeys.Add(Traverse.Create(type).Field(nameof(XmlNameParseKey)).GetValue<string>(), type.FullName);
    }


    public const string XmlNameParseKey = "";

    public virtual bool Static => false;

    public abstract bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data);


    [UsedImplicitly]
    public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //Log.Message("Condition: " + xmlRoot.OuterXml + "\n" + xmlRoot.ChildNodes.Count);
        if(xmlRoot.ChildNodes.Count == 1)
        {
            FieldInfo field = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).First();
            Utilities.SetFieldFromXmlNodeRaw(Traverse.Create(this).Field(field.Name), xmlRoot, this, field.Name, field.FieldType);
        }
        else
        {
            Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, []);
        }
    }

    public static XmlNode CustomListLoader(XmlNode xmlNode)
    {
        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            if (Condition.XmlNameParseKeys.TryGetValue(node.Name, out string classTag))
            {
                XmlAttribute attribute = xmlNode.OwnerDocument!.CreateAttribute("Class");
                attribute.Value = classTag;
                node.Attributes!.SetNamedItem(attribute);
            }
        }

        return xmlNode;
    }
}

public class ConditionRotStage : Condition
{
    public new const string XmlNameParseKey = "RotStage";

    public List<RotStage> allowedStages = [RotStage.Fresh];

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) => 
        this.allowedStages.Contains(pawn.GetRotStage() ?? RotStage.Fresh);

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        this.allowedStages = xmlRoot.FirstChild.Value.Split(',').Select(ParseHelper.FromString<RotStage>).ToList();
    }
}

public class ConditionBodyPart : Condition
{
    public override bool Static => true;

    public new const string XmlNameParseKey = "BodyPart";

    public BodyPartDef bodyPart;
    public string      bodyPartLabel;
    public bool        drawWithoutPart = false;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.HasNamedBodyPart(data.bodyPart = this.bodyPart, data.bodyPartLabel = this.bodyPartLabel)// || pawn.LinkToCorePart(this.drawWithoutPart, this.alignWithHead, this.bodyPart, this.bodyPartLabel))
    //|| this.extendedGraphics.OfType<AlienPartGenerator.ExtendedHediffGraphic>().Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart)
    ;

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot) => 
        Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, []);
}

public class ConditionDrafted : Condition
{
    public new const string XmlNameParseKey = "Drafted";

    private bool drafted = true;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        this.drafted == pawn.Drafted;
}

public class ConditionJob : Condition
{
    public new const string XmlNameParseKey = "Job";

    public JobDef job;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.CurJob?.def == this.job;

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        string value = xmlRoot.FirstChild.Value;
        if (value.Trim() == "None")
            this.job = null;
        else
            base.LoadDataFromXmlCustom(xmlRoot);
    }
}

public class ConditionMoving : Condition
{
    public new const string XmlNameParseKey = "Moving";

    public bool moving;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) => 
        this.moving == pawn.Moving;
}

public class ConditionApparel : Condition
{
    public new const string XmlNameParseKey = "Apparel";

    public List<BodyPartGroupDef> hiddenUnderApparelFor = [];
    public List<string>           hiddenUnderApparelTag = [];

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data)
    {
        return pawn.HasApparelGraphics()                                                                          ||
               (!AlienRenderTreePatches.IsPortrait(pawn.WrappedPawn) && !pawn.VisibleInBed())                     ||
               (AlienRenderTreePatches.IsPortrait(pawn.WrappedPawn)  && Prefs.HatsOnlyOnMap)                     ||
               (this.hiddenUnderApparelTag.NullOrEmpty()             && this.hiddenUnderApparelFor.NullOrEmpty()) ||
               !pawn.GetWornApparelProps().Any(ap =>
                                              ap.bodyPartGroups.Any(bpgd => this.hiddenUnderApparelFor.Contains(bpgd)) ||
                                              ap.tags.Any(s => this.hiddenUnderApparelTag.Contains(s)));
    }

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot) =>
        Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, []);
}

public class ConditionApparelDef : Condition
{
    public new const string XmlNameParseKey = "ApparelDef";

    public ThingDef apparel;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.GetWornApparel.Any(ap => ap.def == this.apparel);
}

public class ConditionPosture : Condition
{
    public new const string XmlNameParseKey = "Posture";

    private bool drawnStanding = false;
    private bool drawnLaying   = false;
    private bool drawnInBed    = false;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        (pawn.GetPosture() == PawnPosture.Standing && this.drawnStanding) ||
        ((pawn.GetPosture() != PawnPosture.Standing && this.drawnLaying) &&
         (!pawn.GetPosture().InBed() || this.drawnInBed));

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot) =>
        Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, []);
}

public class ConditionDamage : Condition
{
    public new const string XmlNameParseKey = "Damage";

    public float damage;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) => 
        pawn.IsPartBelowHealthThreshold(data.bodyPart, data.bodyPartLabel, this.damage);
}

public class ConditionAge : Condition
{
    public new const string XmlNameParseKey = "Age";

    public LifeStageDef age;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.CurrentLifeStageDefMatches(this.age);
}

public class ConditionHediff : Condition
{
    public new const string XmlNameParseKey = "Hediff";

    public HediffDef hediff;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data)
    {
        bool      satisfied      = pawn.HasHediffOfDefAndPart(this.hediff, data.bodyPart, data.bodyPartLabel) != null;
        if (satisfied)
            data.hediff = this.hediff;
        return satisfied;
    }
}

public class ConditionHediffSeverity : Condition
{
    public new const string XmlNameParseKey = "Severity";

    public float severity;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data)
    {
        return pawn.SeverityOfHediffsOnPart(data.hediff, data.bodyPart, data.bodyPartLabel).Any(sev => sev > this.severity);
    }
}

public class ConditionBackstory : Condition
{
    public new const string XmlNameParseKey = "Backstory";

    public override bool         Static => true;
    public          BackstoryDef backstory;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.HasBackStory(this.backstory);
}

public class ConditionGender : Condition
{
    public new const string XmlNameParseKey = "Gender";

    public override bool   Static => true;
    public          Gender gender;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) => 
        pawn.GetGender() == this.gender;
}

public class ConditionTrait : Condition
{
    public new const string XmlNameParseKey = "Trait";

    public override  bool   Static => true;
    public           string trait;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.HasTraitWithIdentifier(this.trait);
}

public class ConditionBodyType : Condition
{
    public new const string XmlNameParseKey = "BodyType";

    public override bool        Static => true;
    public          BodyTypeDef bodyType;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.HasBodyType(this.bodyType);
}

public class ConditionHeadType : Condition
{
    public new const string XmlNameParseKey = "HeadType";

    public override bool        Static => true;
    public          HeadTypeDef headType;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.HasHeadTypeNamed(this.headType);
}

public class ConditionGene : Condition
{
    public new const string XmlNameParseKey = "Gene";

    public override bool    Static => true;
    public          GeneDef gene;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) => 
        !ModsConfig.BiotechActive || pawn.HasGene(this.gene);
}

public class ConditionRace : Condition
{
    public new const string XmlNameParseKey = "Race";

    public override bool     Static => true;

    public List<ThingDef> raceRequirement;
    public List<ThingDef> raceBlacklist;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        (this.raceRequirement.NullOrEmpty() || this.raceRequirement.Any(pawn.IsRace)) && (this.raceBlacklist.NullOrEmpty() || !this.raceBlacklist.Any(pawn.IsRace));
}

public class ConditionMutant : Condition
{
    public new const string XmlNameParseKey = "Mutant";

    public override bool      Static => true;
    public          MutantDef mutant;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.IsMutant(this.mutant);
}

public class ConditionCreepJoinerFormKind : Condition
{
    public new const string XmlNameParseKey = "CreepForm";

    public override bool                   Static => true;
    public          CreepJoinerFormKindDef form;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
        pawn.IsCreepJoiner(this.form);
}

public abstract class ConditionLogic : Condition
{
    public List<Condition> conditions = [];

    public override bool Static => this.conditions.TrueForAll(cd => cd.Static);

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        this.conditions = DirectXmlToObject.ObjectFromXml<List<Condition>>(xmlRoot, true);
    }
}

public class ConditionLogicOr : ConditionLogic
{
    public new const string XmlNameParseKey = "Or";

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data)
    {
        ResolveData tmpData = data;
        bool        logic   = this.conditions.Any(cd => cd.Satisfied(pawn, ref tmpData));
        data = tmpData;

        Log.Message($"{this.conditions.Count}: {logic}");

        return logic;
    }
}

public class ConditionLogicAnd : ConditionLogic
{
    public new const string XmlNameParseKey = "And";

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data)
    {
        ResolveData tmpData = data;
        bool        logic   = this.conditions.TrueForAll(cd => cd.Satisfied(pawn, ref tmpData));
        data = tmpData;
        return logic;
    }
}