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
    }
}
