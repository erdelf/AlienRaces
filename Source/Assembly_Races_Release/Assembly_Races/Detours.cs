using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace AlienRace
{
    public static class Detours
    {
        /*
        public static IEnumerable<FloatMenuOption> _GetFloatMenuOptions(this Building_CommsConsole _this, Pawn myPawn)
        {
            if (!myPawn.CanReserve(_this, 1))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item
                };
            }
            if (!myPawn.CanReach(_this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item2
                };
            }
            if (_this.Spawned && _this.Map.mapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare))
            {
                FloatMenuOption item3 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item3
                };
            }
            if (!_this.GetComp<CompPowerTrader>().PowerOn)
            {
                FloatMenuOption item4 = new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item4
                };
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                FloatMenuOption item5 = new FloatMenuOption("CannotUseReason".Translate(new object[]
                {
                    "IncapableOfCapacity".Translate(new object[]
                    {
                        PawnCapacityDefOf.Talking.label
                    })
                }), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item5
                };
            }
            if (!_this.CanUseCommsNow)
            {
                Log.Error(myPawn + " could not use comm console for unknown reason.");
                FloatMenuOption item6 = new FloatMenuOption("Cannot use now", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                    {
                        item6
                    };
            }
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            IEnumerable<ICommunicable> enumerable = myPawn.Map.passingShipManager.passingShips.Cast<ICommunicable>().Concat(Find.FactionManager.AllFactionsInViewOrder.Cast<ICommunicable>());
            foreach (ICommunicable commTarget in enumerable)
            {
                ICommunicable localCommTarget = commTarget;
                string text = "CallOnRadio".Translate(new object[]
                {
                    localCommTarget.GetCallLabel()
                });
                Faction faction = localCommTarget as Faction;
                if (faction != null)
                {
                    if (faction.IsPlayer)
                    {
                        continue;
                    }
                    if (!Building_CommsConsole.LeaderIsAvailableToTalk(faction))
                    {
                        list.Add(new FloatMenuOption(text + " (" + "LeaderUnavailable".Translate(new object[]
                        {
                            faction.leader?.LabelShort ?? ""
                        }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        continue;
                    }
                }
                Log.Message("5");
                Action action = delegate
                {
                    ICommunicable localCommTargetCopy = localCommTarget;
                    if (commTarget is TradeShip && !Building_OrbitalTradeBeacon.AllPowered(_this.Map).Any<Building_OrbitalTradeBeacon>())
                    {
                        Messages.Message("MessageNeedBeaconToTradeWithShip".Translate(), _this, MessageSound.RejectInput);
                        return;
                    }
                    Job job = new Job(JobDefOf.UseCommsConsole, _this);
                    job.commTarget = localCommTargetCopy;
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                };
                list.Add(new FloatMenuOption(text, action, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null));
            }
            return list;
        }*/




        private static List<string> detoured = new List<string>();
        private static List<string> destinations = new List<string>();

        /**
            _this is a basic first implementation of the IL method 'hooks' (detours) made possible by RawCode's work;
            https://ludeon.com/forums/index.php?topic=17143.0

            Performs detours, spits out basic logs and warns if a method is detoured multiple times.
        **/
        public static unsafe bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
        {
            // error out on null arguments
            if (source == null)
            {
                Log.Message("Source MethodInfo is null");
                return false;
            }

            if (destination == null)
            {
                Log.Message("Destination MethodInfo is null");
                return false;
            }

            // keep track of detours and spit out some messaging
            string sourceString = source.DeclaringType.FullName + "." + source.Name + " @ 0x" + source.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString());
            string destinationString = destination.DeclaringType.FullName + "." + destination.Name + " @ 0x" + destination.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString());


            if (detoured.Contains(sourceString))
            {
                Log.Warning("Source method ('" + sourceString + "') is previously detoured to '" + destinations[detoured.IndexOf(sourceString)] + "'");
            }
            UnityEngine.Debug.Log( "AlienRace: Detouring '" + sourceString + "' to '" + destinationString + "'");
            //Log.Message("Detouring '" + sourceString + "' to '" + destinationString + "'");


            detoured.Add(sourceString);
            destinations.Add(destinationString);

            if (IntPtr.Size == sizeof(Int64))
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                long Source_Base = source.MethodHandle.GetFunctionPointer().ToInt64();
                long Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                byte* Pointer_Raw_Source = (byte*)Source_Base;

                // Pointer to insert jump address into native code
                long* Pointer_Raw_Address = (long*)(Pointer_Raw_Source + 0x02);

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *(Pointer_Raw_Source + 0x00) = 0x48;
                *(Pointer_Raw_Source + 0x01) = 0xB8;
                *Pointer_Raw_Address = Destination_Base; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *(Pointer_Raw_Source + 0x0A) = 0xFF;
                *(Pointer_Raw_Source + 0x0B) = 0xE0;

            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                int Source_Base = source.MethodHandle.GetFunctionPointer().ToInt32();
                int Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                byte* Pointer_Raw_Source = (byte*)Source_Base;

                // Pointer to insert jump address into native code
                int* Pointer_Raw_Address = (int*)(Pointer_Raw_Source + 1);

                // Jump offset (less instruction size)
                int offset = (Destination_Base - Source_Base) - 5;

                // Insert 32-bit relative jump into native code
                *Pointer_Raw_Source = 0xE9;
                *Pointer_Raw_Address = offset;
            }

            // done!
            return true;
        }

    }

}
