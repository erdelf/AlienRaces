namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using System.Xml.Serialization;
using Verse;
using JetBrains.Annotations;
using System.Xml;
using static AlienRace.AlienPartGenerator.BodyAddon;

public abstract class Condition
{
    public virtual bool Static => false;

    public abstract bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel);


    [UsedImplicitly]
    public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {

        foreach (XmlNode childNode in xmlRoot.ChildNodes)
        {
            /*
            if (XML_CLASS_DICTIONARY.TryGetValue(childNode.Name, out string classTag))
            {
                XmlAttribute attribute = xmlRoot.OwnerDocument!.CreateAttribute("Class");
                attribute.Value = classTag;
                childNode.Attributes!.SetNamedItem(attribute);
            }*/
        }

        Utilities.SetInstanceVariablesFromChildNodesOf(xmlRoot, this, []);
    }
}

public class ConditionRotStage : Condition
{
    public List<RotStage> allowedStages = [RotStage.Fresh];

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        this.allowedStages.Contains(pawn.GetRotStage() ?? RotStage.Fresh);
}

public class ConditionBodyPart : Condition
{

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
    private bool drafted = true;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        this.drafted == pawn.Drafted;
}

public class ConditionJob : Condition
{
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
    private bool drawnOnGround = true;
    private bool drawnInBed = true;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) &&
        (pawn.VisibleInBed()                       || this.drawnInBed);
}

public class ConditionDamage : Condition
{
    public float damage;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        pawn.IsPartBelowHealthThreshold(part, partLabel, this.damage);
}

public class ConditionAge : Condition
{
    public LifeStageDef age;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.CurrentLifeStageDefMatches(this.age);
}

public class ConditionHediff : Condition
{
    public  HediffDef                     hediff;
    private List<ConditionHediffSeverity> severities;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasHediffOfDefAndPart(this.hediff, part, partLabel);
}

public class ConditionHediffSeverity : ConditionHediff
{
    public float severity;
}

public class ConditionBackstory : Condition
{
    public override bool         Static => true;
    public          BackstoryDef backstory;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasBackStory(this.backstory);
}

public class ConditionGender : Condition
{
    public override bool   Static => true;
    public          Gender gender;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        pawn.GetGender() == this.gender;
}

public class ConditionTrait : Condition
{
    public override bool   Static => true;
    public          string trait;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasTraitWithIdentifier(this.trait);
}

public class ConditionBodyType : Condition
{
    public override bool        Static => true;
    public          BodyTypeDef bodyType;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasBodyType(this.bodyType);
}

public class ConditionHeadType : Condition
{
    public override bool        Static => true;
    public          HeadTypeDef headType;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.HasHeadTypeNamed(this.headType);
}

public class ConditionGene : Condition
{
    public override bool    Static => true;
    public          GeneDef gene;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) => 
        !ModsConfig.BiotechActive || pawn.HasGene(this.gene);
}

public class ConditionRace : Condition
{
    public override bool     Static => true;

    public List<ThingDef> raceRequirement;
    public List<ThingDef> raceBlacklist;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        (this.raceRequirement.NullOrEmpty() || this.raceRequirement.Any(pawn.IsRace)) && (this.raceBlacklist.NullOrEmpty() || !this.raceBlacklist.Any(pawn.IsRace));
}

public class ConditionMutant : Condition
{
    public override bool      Static => true;
    public          MutantDef mutant;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.IsMutant(this.mutant);
}

public class ConditionCreepJoinerFormKind : Condition
{
    public override bool                   Static => true;
    public          CreepJoinerFormKindDef form;

    public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref BodyPartDef part, ref string partLabel) =>
        pawn.IsCreepJoiner(this.form);
}