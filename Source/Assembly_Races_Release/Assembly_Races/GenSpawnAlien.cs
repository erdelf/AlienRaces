using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public static class GenSpawnAlien
    {

        public static Thing Spawn(Thing newThing, IntVec3 loc, Map map)
        {
            return GenSpawnAlien.SpawnModded(newThing, loc, map, Rot4.North);
        }

        public static Thing SpawnModded(Thing newThing, IntVec3 loc, Map map, Rot4 rot)
        {
            if (map == null)
            {
                Log.Error("Tried to spawn " + newThing + " in a null map.");
                return null;
            }
            if (!loc.InBounds(map))
            {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to spawn ",
                    newThing,
                    " out of bounds at ",
                    loc,
                    "."
                }));
                return null;
            }
            if (newThing.Spawned)
            {
                Log.Error("Tried to spawn " + newThing + " but it's already spawned.");
                return newThing;
            }
            GenSpawn.WipeExistingThings(loc, rot, newThing.def, map, DestroyMode.Vanish);
            if (newThing.def.randomizeRotationOnSpawn)
            {
                newThing.Rotation = Rot4.Random;
            }
            else
            {
                newThing.Rotation = rot;
            }
            newThing.Position = loc;
            ThingUtility.UpdateRegionListers(IntVec3.Invalid, loc, map, newThing);
            map.thingGrid.Register(newThing);
            newThing.SpawnSetup(map);
            if (newThing.Spawned)
            {
                if (newThing.stackCount == 0)
                {
                    Log.Error("Spawned thing with 0 stackCount: " + newThing);
                    newThing.Destroy(DestroyMode.Vanish);
                    return null;
                }
            }
            else
            {
                ThingUtility.UpdateRegionListers(loc, IntVec3.Invalid, map, newThing);
                map.thingGrid.Deregister(newThing, true);
            }
            if (newThing.def.GetType() != typeof(Thingdef_AlienRace))
            {
                return newThing;
            }
            else
            {
                AlienPawn alienpawn2 = newThing as AlienPawn;
                alienpawn2.SpawnSetupAlien();

            //    Log.Message(alienpawn2.kindDef.race.ToString());
                return alienpawn2;
            }
        }        
    }
}