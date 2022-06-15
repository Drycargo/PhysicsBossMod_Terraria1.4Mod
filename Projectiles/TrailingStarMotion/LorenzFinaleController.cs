using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class LorenzFinaleController : TrailingStarController
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 10 * 60;
        }

        public void activateAll() {
            foreach (TrailingStarChaotic star in stars) {
                ((TrailingStarLorenzFinale) star).activate();
            }
        }

        public void summonStar() {
            TrailingStarLorenzFinale tsc = (TrailingStarLorenzFinale)Projectile.NewProjectileDirect(
                Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<TrailingStarLorenzFinale>(), 25, 0).ModProjectile;
            tsc.setOwner(this);
            stars.Enqueue(tsc);
        }

        public int getStarCount() {
            return stars.Count;
        }
    }
}
