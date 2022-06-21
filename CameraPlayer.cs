using Microsoft.Xna.Framework;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace PhysicsBoss
{
    public class CameraPlayer : ModPlayer
    {
        private static Vector2 displacement = Vector2.Zero;
        private static Vector2 shake = Vector2.Zero;
        private static int bossIndex = -1;

        private static bool activated = false;
        public override void ModifyScreenPosition()
        {

            if (activated)
            {
                // check for ChaosTheory in ThreeBodyPreparation Phase
                if (bossIndex < 0 || !Main.npc[bossIndex].active || Main.npc[bossIndex].life < 0
                    || Main.npc[bossIndex].type != ModContent.NPCType<ChaosTheory>()) {
                    deActivate();
                    return;
                }

                /*
                int requiredType = ModContent.NPCType<ChaosTheory>();
                foreach (Terraria.NPC npc in Main.npc)
                {
                    if (npc.type == requiredType)
                    {
                        break;
                    }
                    activated = false;
                    return;
                }
                */

                Main.screenPosition = displacement;
                Main.screenPosition.X = Math.Max(Main.screenPosition.X, 0);
                Main.screenPosition.Y = Math.Max(Main.screenPosition.Y, 0);
            }

            Main.screenPosition += shake;
            shake *= 0.95f;
            if (shake.Length() < 0.5f)
                shake *= 0;

            base.ModifyScreenPosition();
        }

        public static void setDisplacement(Vector2 disp) {
            displacement = disp;
        }

        public static void setShake(Vector2 disp)
        {
            shake = disp;
        }

        public static void activate(int bossIndex) {
            activated = true;
            CameraPlayer.bossIndex = bossIndex;
        }

        public static void deActivate()
        {
            activated = false;
            bossIndex = -1;
        }
    }
}
