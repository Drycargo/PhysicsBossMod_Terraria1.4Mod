using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace PhysicsBoss.NPC
{
    public interface EnemyInterface1
    {
        public static Player seekTarget(Vector2 myCenter, float minDist) {
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
    }
}
