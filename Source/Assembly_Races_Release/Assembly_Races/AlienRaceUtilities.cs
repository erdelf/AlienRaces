using System;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    public static class AlienRaceUtilities
    {
        public static List<PawnKindDef> AllAlienColonistReferences = new List<PawnKindDef>();

        public static List<AlienKey> AllAlienColorKeys = new List<AlienKey>();

        public static bool ColorsInitiated = false;
        public static void InitializeAlienColors()
        {
            if (!AlienRaceUtilities.ColorsInitiated)
            {
                try
                {
              //      Log.Message("Initalizing Alien Colors");
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.defName.Contains("Alien_"))
                        {
                            Thingdef_AlienRace tdef = def as Thingdef_AlienRace;
              //              Log.Message("Initializing Colorstuff");

                            ColorGenerator_Options colorgenerator1;
                            List<Color> list1 = new List<Color>();
                            ColorGenerator_Options colorgenerator2;
                            List<Color> list2 = new List<Color>();
                            if ((colorgenerator1 = tdef.alienhaircolorgen as ColorGenerator_Options) != null)
                            {
                                if (colorgenerator1.options != null)
                                {
                                    foreach (ColorOption colOption in colorgenerator1.options)
                                    {
                                        //  Log.Message("Adding HairColors");
                                        list1.Add(colOption.only);
                                    }
                                }
                            }
                            if ((colorgenerator2 = tdef.alienskincolorgen as ColorGenerator_Options) != null)
                            {
                                if (colorgenerator2.options != null)
                                {
                                    foreach (ColorOption colOption in colorgenerator2.options)
                                    {
                                        // Log.Message("Adding SkinColors");
                                        list2.Add(colOption.only);
                                    }
                                }
                            }

                            AlienRaceUtilities.AllAlienColorKeys.Add(new AlienKey(tdef, list1, list2));
                        }
                    }
                    AlienRaceUtilities.ColorsInitiated = true;
                }
                catch
                { }
            }
        }

        public static void InitializeAlienColonistOptions()
        {
            foreach (PawnKindDef_StartingColonist pdef in StartingColonistAliens)
            {
                if (pdef.IsPossibleStartingColonistOf.Any(x => x.factionDef == Faction.OfPlayer.def))
                {
                    if (!AllAlienColonistReferences.Contains(pdef))
                    {
                        AllAlienColonistReferences.Add(pdef);
                    }
                }
            }
        }


        public static AlienPawn GenerateNewStartingAlienColonist(PawnKindDef kinddef)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(kinddef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, false, true, true, null, null, null, null, null, null);
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
                AlienPawn alienpawn2 = pawn as AlienPawn;
                alienpawn2.SpawnSetupAlien();
            return alienpawn2;

        }

        public static Pawn NewGeneratedStartingPawnModded()
        {
            PawnKindDef pdef = Faction.OfPlayer.def.basicMemberKind;
            List<PawnKindDef_StartingColonist> list = AlienRaceUtilities.StartingColonistAliens;
            if (AlienRaceUtilities.StartingColonistAliens.Count>0)
            {
                for(int i=0; i< list.Count; i++)
                {
                    for(int j=0; j < list[i].IsPossibleStartingColonistOf.Count; j++)
                    {
                        if (Faction.OfPlayer.def == list[i].IsPossibleStartingColonistOf[j].factionDef)
                        {
                            //         Log.Message("Cycling Pwnkinds :" + list[i].IsPossibleStartingColonistOf[j].factionDef.ToString());
                            float num = 1 / (list[i].IsPossibleStartingColonistOf[j].ProportionOfBasicMember + 1);
                            //         Log.Message(list[i].IsPossibleStartingColonistOf[j].ProportionToBasicMember.ToString());
                            //         Log.Message("Proportion " + num.ToString());                           
                            if (Rand.Range(0f, 1f) < num)
                            {
                  //              Log.Message("GotAlienstartingcolonist");
                                pdef = list[i];                                
                                break;                                
                            }
                        }
                    }                    
                }                
            }
            //Log.Message("1");
            PawnGenerationRequest request = new PawnGenerationRequest(pdef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, false, true, true, null, null, null, null, null, null);
            Pawn pawn = null;
            try
            {
                //Log.Message("2");
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            //Log.Message("3");
            //Log.Message((pawn != null).ToString());
            //Log.Message((pawn.relations != null).ToString());
            pawn.relations.everSeenByPlayer = true;
            //Log.Message("4");
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            //Log.Message("5");
            if (!pawn.def.defName.Contains("Alien_"))
            {
                return pawn;
            }
            else
            {
                AlienPawn alienpawn2 = pawn as AlienPawn;
                alienpawn2.SpawnSetupAlien();
             //   Log.Message("Generated Starting Pawn from race: " + alienpawn2.kindDef.race.ToString());
                return alienpawn2;
            }
        }

        public static Pawn NewGeneratedStartingPawn()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, false, true, true, null, null, null, null, null, null);
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            return pawn;
        }

        public static List<PawnKindDef_StartingColonist> StartingColonistAliens
        {
            get
            {
                return DefDatabase<PawnKindDef_StartingColonist>.AllDefsListForReading;

            }
        }

        public static void DoRecruitAlien(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
        {
            string text = recruitee.LabelIndefinite();
            if (recruitee.guest != null)
            {
                recruitee.guest.SetGuestStatus(null, false);
            }
            bool flag = recruitee.Name != null;
            if (recruitee.Faction != recruiter.Faction)
            {

                if (recruitee.kindDef.race.ToString().Contains("Alien"))
                {
                    Log.Message("RecruitingAlienPawn");
                    var x = recruitee.kindDef;
                    AlienPawn temprecruitee = recruitee as AlienPawn;
                    temprecruitee.SetFaction(recruiter.Faction, recruiter);
                    temprecruitee.kindDef = x;
                    Log.Message("Pawn Converted to Kind:  " + recruitee.kindDef.race.ToString());
                }
                else
                {
                    recruitee.SetFaction(recruiter.Faction, recruiter);
                }
            }
            if (recruitee.RaceProps.Humanlike)
            {
                if (useAudiovisualEffects)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelMessageRecruitSuccess".Translate(), "MessageRecruitSuccess".Translate(new object[]
                    {
                recruiter,
                recruitee,
                recruitChance.ToStringPercent()
                    }), LetterType.Good, recruitee, null);
                }
                TaleRecorder.RecordTale(TaleDefOf.Recruited, new object[]
                {
            recruiter,
            recruitee
                });
                recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
                recruitee.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RecruitedMe, recruiter);
            }
            else
            {
                if (useAudiovisualEffects)
                {
                    if (!flag)
                    {
                        Messages.Message("MessageTameAndNameSuccess".Translate(new object[]
                        {
                    recruiter.LabelShort,
                    text,
                    recruitChance.ToStringPercent(),
                    recruitee.Name.ToStringFull
                        }).AdjustedFor(recruitee), recruitee, MessageSound.Benefit);
                    }
                    else
                    {
                        Messages.Message("MessageTameSuccess".Translate(new object[]
                        {
                    recruiter.LabelShort,
                    text,
                    recruitChance.ToStringPercent()
                        }), recruitee, MessageSound.Benefit);
                    }
                    MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruitee.Map, "TextMote_TameSuccess".Translate(new object[]
                    {
                recruitChance.ToStringPercent()
                    }), 8f);
                }
                recruiter.records.Increment(RecordDefOf.AnimalsTamed);
                RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.05f);
                float num = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
                if (Rand.Value < num)
                {
                    TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, new object[]
                    {
                recruiter,
                recruitee
                    });
                }
            }
            if (recruitee.caller != null)
            {
                recruitee.caller.DoCall();
            }
        }

    }
}
