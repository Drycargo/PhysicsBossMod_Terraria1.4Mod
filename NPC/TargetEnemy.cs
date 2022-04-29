using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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

        protected void hover(Vector2 hoverCenter, float hoverRadius, float noise, float period)
        {
            float degree = (NPC.Center - hoverCenter).ToRotation() + MathHelper.TwoPi / period;

            Vector2 aim = degree.ToRotationVector2() * hoverRadius + hoverCenter
                + noise * 2 * (Main.rand.NextFloat() - 0.5f) * Vector2.One;

            if (aim.Distance(NPC.Center) > 10f)
                NPC.Center = 0.8f * NPC.Center + 0.2f * aim;
            else
                NPC.Center = aim;
        }
    }
}
