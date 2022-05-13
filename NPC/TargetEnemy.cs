using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PhysicsBoss.NPC
{
    public abstract class TargetEnemy: ModNPC {
        protected Player target;
        public static Player seekTarget(Vector2 myCenter, float minDist)
        {
            Player target = null;
            foreach (var player in Main.player)
            {
                if (player.active && Vector2.Distance(player.Center, myCenter) < minDist)
                {
                    minDist = Vector2.Distance(player.Center, myCenter);
                    target = player;
                }
            }
            return target;
        }

        public void setTarget(Player t) {
            target = t;
        }

        public Player getTarget()
        {
            return target;
        }

        protected void hover(Vector2 hoverCenter, float hoverRadius, float noise, float period)
        {
            hover(hoverCenter, hoverRadius, noise, period, 10f,-1f, 0.7f);
        }

        // If period is negative, rotate counter-clockwise
        protected void hover(Vector2 hoverCenter, float hoverRadius, float noise, float period, float safetyDistance, float traceDistance,float inertia)
        {
            float degree = (NPC.Center - hoverCenter).ToRotation() + MathHelper.TwoPi / period;

            Vector2 aim = degree.ToRotationVector2() * hoverRadius + hoverCenter
                + noise * 2 * (Main.rand.NextFloat() - 0.5f) * Vector2.One;

            if (aim.Distance(NPC.Center) > safetyDistance)
            {
                if (traceDistance >= 0 && aim.Distance(NPC.Center) > traceDistance)
                    NPC.Center = 0.8f * inertia * NPC.Center + (1f - inertia * 0.8f) * aim;
                else
                    NPC.Center = inertia * NPC.Center + (1f - inertia) * aim;
            } else
                NPC.Center = aim;

            NPC.rotation = (hoverCenter - NPC.Center).ToRotation();
        }

        protected void fireWork(Color c) {
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RainbowRod);
                d.velocity = Main.rand.NextVector2Unit() * 15f;
                d.color = c;
                d.noGravity = true;
            }
        }
    }
}
