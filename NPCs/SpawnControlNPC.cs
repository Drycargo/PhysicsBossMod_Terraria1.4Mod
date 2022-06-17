using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.NPCs
{
    public class SpawnControlNPC : GlobalNPC
    {
        public static int bossInd = -1;
        public static readonly int[] ANNOYING_CREATURES_ARRAY = {
            NPCID.WyvernHead,
            NPCID.WyvernBody,
            NPCID.WyvernBody3,
            NPCID.WyvernLegs,
            NPCID.WyvernTail,
            NPCID.Harpy
        };
        
        public static readonly HashSet<int> ANNOYING_CREATURES = new HashSet<int>(ANNOYING_CREATURES_ARRAY);
        
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (bossActive()) {
                foreach (int type in ANNOYING_CREATURES_ARRAY) {
                    pool.Remove(type);
                }
                /*
                pool.Remove(NPCID.WyvernHead);
                pool.Remove(NPCID.WyvernBody);
                pool.Remove(NPCID.WyvernBody2);
                pool.Remove(NPCID.WyvernBody3);
                pool.Remove(NPCID.WyvernLegs);
                pool.Remove(NPCID.WyvernTail);
                pool.Remove(NPCID.Harpy);*/
            }
            /*
            if (bossActive())
            {
                pool[NPCID.WyvernHead] = -10;
                pool[NPCID.Harpy] = -10;
            } else {
                pool[NPCID.WyvernHead] = 0;
                pool[NPCID.Harpy] = 0;
            }
            */
            //base.EditSpawnPool(pool, spawnInfo);
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (bossActive()) {
                if (ANNOYING_CREATURES.Contains(npc.type))
                    npc.active = false;
            }
        }

        private static bool bossActive()
        {
            if (bossInd < 0 || bossInd >= Main.npc.Length)
                return false;

            if (!(Main.npc[bossInd].type == ModContent.NPCType<ChaosTheory>()
                            && Main.npc[bossInd].life > 0 && Main.npc[bossInd].active)) {
                bossInd = -1;
                return false;
            }

            return true;
        }
    }
}
