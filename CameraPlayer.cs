using Microsoft.Xna.Framework;
using PhysicsBoss.NPC.Boss.ChaosTheory;
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

        private static bool activated = false;
        public override void ModifyScreenPosition()
        {
            /*
            // check for ChaosTheory in ThreeBodyPreparation Phase
            int requiredType = ModContent.NPCType<ChaosTheory>();
            foreach (Terraria.NPC npc in Main.npc) {
                if (npc.type == requiredType && npc.active) {
                    Main.screenPosition = npc.position;
                    break;
                }
            }*/

            if (activated)
            {

                Main.screenPosition += displacement;
                Main.screenPosition.X = Math.Max(Main.screenPosition.X, 0);
                Main.screenPosition.Y = Math.Max(Main.screenPosition.Y, 0);
            }

            if (displacement.Length() >= 1)
                displacement *= 0.9f;
            else
                displacement *= 0;

            base.ModifyScreenPosition();
        }

        public static void setDisplacement(Vector2 disp) {
            displacement = disp;
        }

        public static void activate() {
            activated = true;
        }

        public static void deActivate()
        {
            activated = false;
        }
    }
}
