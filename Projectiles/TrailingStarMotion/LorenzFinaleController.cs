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
            Projectile.timeLeft = 60 * 60;
        }
    }
}
