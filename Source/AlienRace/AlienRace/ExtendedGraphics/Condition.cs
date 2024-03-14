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

    public abstract bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel);


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
            Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, []);
    }
}

public class ConditionRotStage : Condition
{
    public new const string XmlNameParseKey = "RotStage";

    public List<RotStage> allowedStages = [RotStage.Fresh];

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        this.allowedStages.Contains(pawn.GetRotStage() ?? RotStage.Fresh);

    public override void LoadDataFromXmlCustom(XmlNode xmlRoot) => 
        this.allowedStages = xmlRoot.Value.Split(',').Select(ParseHelper.FromString<RotStage>).ToList();
}

public class ConditionBodyPart : Condition
{
    public new const string XmlNameParseKey = "BodyPart";

    public BodyPartDef bodyPart;
    public string      bodyPartLabel;
    public bool        drawWithoutPart = false;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        (pawn.HasNamedBodyPart(this.bodyPart, this.bodyPartLabel))// || pawn.LinkToCorePart(this.drawWithoutPart, this.alignWithHead, this.bodyPart, this.bodyPartLabel))
    //|| this.extendedGraphics.OfType<AlienPartGenerator.ExtendedHediffGraphic>().Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart)
    ;
}

public class ConditionDrafted : Condition
{
    public new const string XmlNameParseKey = "Drafted";

    private bool drafted = true;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        this.drafted == pawn.Drafted;
}

public class ConditionJob : Condition
{
    public new const string XmlNameParseKey = "JobConfig";

    public BodyAddonJobConfig jobs = new();

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.CurJob == null ?
            this.jobs.drawNoJob :
            !this.jobs.JobMap.TryGetValue(pawn.CurJob.def, out BodyAddonJobConfig.BodyAddonJobConfigJob jobConfig) || jobConfig.IsApplicable(pawn);

    public class BodyAddonJobConfig
    {
        public bool drawNoJob = true;

        public List<BodyAddonJobConfigJob> jobs = new();

        private Dictionary<JobDef, BodyAddonJobConfigJob> jobMap;

        public Dictionary<JobDef, BodyAddonJobConfigJob> JobMap => this.jobMap ??= this.jobs.ToDictionary(bajcj => bajcj.job);

        public class BodyAddonJobConfigJob
        {
            public JobDef                        job;
            public Dictionary<PawnPosture, bool> drawPostures;
            public bool                          drawMoving   = true;
            public bool                          drawUnmoving = true;

            public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn) =>
                (!this.drawPostures.TryGetValue(pawn.GetPosture(), out bool postureDraw) || postureDraw) &&
                (this.drawMoving   && pawn.Moving ||
                 this.drawUnmoving && !pawn.Moving);
        }
    }
}

public class ConditionApparel : Condition
{
    public new const string XmlNameParseKey = "Apparel";

    public List<BodyPartGroupDef> hiddenUnderApparelFor = [];
    public List<string>           hiddenUnderApparelTag = [];

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        !pawn.HasApparelGraphics()                                                             ||
        (this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) ||
        !pawn.GetWornApparel().Any(ap =>
                                       ap.bodyPartGroups.Any(bpgd => this.hiddenUnderApparelFor.Contains(bpgd)) ||
                                       ap.tags.Any(s => this.hiddenUnderApparelTag.Contains(s)));
}

public class ConditionPosture : Condition
{
    public new const string XmlNameParseKey = "Posture";

    private bool drawnOnGround = true;
    private bool drawnInBed = true;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) &&
        (pawn.VisibleInBed()                       || this.drawnInBed);
}

public class ConditionDamage : Condition
{
    public new const string XmlNameParseKey = "Damage";

    public float damage;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        pawn.IsPartBelowHealthThreshold(part, partLabel, this.damage);
}

public class ConditionAge : Condition
{
    public new const string XmlNameParseKey = "Age";

    public LifeStageDef age;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.CurrentLifeStageDefMatches(this.age);
}

public class ConditionHediff : Condition
{
    public new const string XmlNameParseKey = "Hediff";

    public  HediffDef                     hediff;
    private List<ConditionHediffSeverity> severities;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasHediffOfDefAndPart(this.hediff, part, partLabel);
}

public class ConditionHediffSeverity : ConditionHediff
{
    public new const string XmlNameParseKey = "Severity";

    public float severity;
}

public class ConditionBackstory : Condition
{
    public new const string XmlNameParseKey = "Backstory";

    public override bool         Static => true;
    public          BackstoryDef backstory;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasBackStory(this.backstory);
}

public class ConditionGender : Condition
{
    public new const string XmlNameParseKey = "Gender";

    public override bool   Static => true;
    public          Gender gender;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        pawn.GetGender() == this.gender;
}

public class ConditionTrait : Condition
{
    public new const string XmlNameParseKey = "Trait";

    public override  bool   Static => true;
    public           string trait;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasTraitWithIdentifier(this.trait);
}

public class ConditionBodyType : Condition
{
    public new const string XmlNameParseKey = "BodyType";

    public override bool        Static => true;
    public          BodyTypeDef bodyType;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasBodyType(this.bodyType);
}

public class ConditionHeadType : Condition
{
    public new const string XmlNameParseKey = "HeadType";

    public override bool        Static => true;
    public          HeadTypeDef headType;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasHeadTypeNamed(this.headType);
}

public class ConditionGene : Condition
{
    public new const string XmlNameParseKey = "Gene";

    public override bool    Static => true;
    public          GeneDef gene;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        !ModsConfig.BiotechActive || pawn.HasGene(this.gene);
}

public class ConditionRace : Condition
{
    public new const string XmlNameParseKey = "Race";

    public override bool     Static => true;

    public List<ThingDef> raceRequirement;
    public List<ThingDef> raceBlacklist;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        (this.raceRequirement.NullOrEmpty() || this.raceRequirement.Any(pawn.IsRace)) && (this.raceBlacklist.NullOrEmpty() || !this.raceBlacklist.Any(pawn.IsRace));
}

public class ConditionMutant : Condition
{
    public new const string XmlNameParseKey = "Mutant";

    public override bool      Static => true;
    public          MutantDef mutant;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.IsMutant(this.mutant);
}

public class ConditionCreepJoinerFormKind : Condition
{
    public new const string XmlNameParseKey = "CreepForm";

    public override bool                   Static => true;
    public          CreepJoinerFormKindDef form;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.IsCreepJoiner(this.form);
}