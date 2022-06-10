using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarSprott: TrailingStarChaotic
    {
        public override float SHRINK_CONST
        {
            get { return 20f; }
        }

        public override float STEP => 3;

        public override Matrix Transform =>
            Matrix.CreateTranslation(-0.7f, 0, 0)
            * Matrix.CreateScale(10f)
            * Matrix.CreateRotationX(MathHelper.PiOver2);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Sprott");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-Sprott吸引子");
            base.SetStaticDefaults();
        }
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = 2 * 60;
            drawColor = Color.Lerp(Color.White, Color.LightBlue, 0.5f);
        }

        protected override void motionUpdate()
        {
            float a = 2.07f, b = 1.79f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                y + a*x*y + x*z,
                1 - b*x*x + y*z,
                x - x*x - y*y) / 60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            if (Main.rand.NextFloat() < 0.25)
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ice);

            base.motionUpdate();
        }
    }
}
