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
            NPCID.Harpy,
            NPCID.PossessedArmor,
            NPCID.Werewolf,
            NPCID.Wraith,
            NPCID.BlueSlime,
            NPCID.GreenSlime,
            NPCID.PurpleSlime,
            NPCID.YellowSlime,
            NPCID.DemonEye,
            NPCID.DemonEye2,
            NPCID.CataractEye,
            NPCID.CataractEye2,
            NPCID.DialatedEye,
            NPCID.DialatedEye2,
            NPCID.GreenEye,
            NPCID.GreenEye2,
            NPCID.PurpleEye,
            NPCID.PurpleEye2,
            NPCID.SleepyEye,
            NPCID.SleepyEye2,
            NPCID.WanderingEye,
            NPCID.BaldZombie,//
            NPCID.BigBaldZombie,
            NPCID.SmallBaldZombie,
            NPCID.Zombie,//
            NPCID.BigZombie,
            NPCID.SmallZombie,
            NPCID.SlimedZombie,//
            NPCID.BigSlimedZombie,
            NPCID.SmallSlimedZombie,
            NPCID.SwampZombie,//
            NPCID.BigSwampZombie,
            NPCID.SmallSwampZombie,
            NPCID.TwiggyZombie,//
            NPCID.BigTwiggyZombie,
            NPCID.SmallTwiggyZombie,
            NPCID.PincushionZombie,//
            NPCID.SmallPincushionZombie,
            NPCID.BigPincushionZombie,
            NPCID.FemaleZombie,//
            NPCID.SmallFemaleZombie,
            NPCID.BigFemaleZombie,
            NPCID.ArmedZombie,//
            NPCID.ArmedTorchZombie,
            NPCID.ArmedZombiePincussion,
            NPCID.ArmedZombieSlimed,
            NPCID.ArmedZombieSwamp,
            NPCID.ArmedZombieTwiggy,
        };
        
        public static readonly HashSet<int> ANNOYING_CREATURES = new HashSet<int>(ANNOYING_CREATURES_ARRAY);
        
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (bossActive())
            {
                foreach (int type in ANNOYING_CREATURES_ARRAY)
                {
                    pool.Remove(type);
                }
            }
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
