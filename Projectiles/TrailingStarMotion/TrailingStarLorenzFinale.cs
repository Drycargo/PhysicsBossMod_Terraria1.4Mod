using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarLorenzFinale: TrailingStarLorenz
    {
        public const int TRANSIT = 120;
        public override float STEP => 1;
        public override float SPEED_LIMIT => MathHelper.Lerp(60, 3, Math.Min(1,Timer/TRANSIT));

        public override Matrix Transform =>
            Matrix.CreateRotationX(MathHelper.PiOver2)
            * Matrix.CreateTranslation(0, 25, 0)
            * Matrix.CreateScale(3f)
            * Matrix.CreateRotationY((float)(0.005f * Timer));


        private bool activated;

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            activated = false;
            drawColor = Color.White * 0.75f;
        }

        public void activate() {
            activated = true;
            drawColor = Color.White;

            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0,0,DustID.RainbowRod);
                d.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            }
        }

        protected override Color colorFun(float progress)
        {
            if (activated)
                return base.colorFun(progress);

            return drawColor * (1 - progress);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if(activated)
                return base.Colliding(projHitbox, targetHitbox);

            return false;
        }
    }
}
