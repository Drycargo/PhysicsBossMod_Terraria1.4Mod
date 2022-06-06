using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarThreeScroll: TrailingStarChaotic
    {
        public static Color[] colors = { Color.Aqua, Color.DeepSkyBlue, Color.Blue, Color.DarkBlue };
        public override float SHRINK_CONST
        {
            get { return 20f; }
        }
        public override float STEP => 5;
        public override Matrix Transform =>
            Matrix.CreateScale(0.055f)
            * Matrix.CreateRotationZ((float)Main.time * 0.025f)
            * Matrix.CreateRotationY(MathHelper.PiOver2)
            * Matrix.CreateTranslation(-6.5f,0,0);
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Three Scroll");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-三轴吸引子");
            base.SetStaticDefaults();
        }
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = 2 * 60;
        }

        protected override void motionUpdate()
        {
            float a = 32.48f, b = 45.84f, c = 1.18f
                , d = 0.13f, e = 0.57f, f = 14.7f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                a*(y - x) + d*x*z,
                b*x - x*z + f*y,
                c*z + x*y - e*x*x) / 60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            if (Main.rand.NextFloat() < 0.05)
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowRod).noGravity = true;

            base.motionUpdate();
        }

        public override void setColor(int colorIndex)
        {
            drawColor = colors[colorIndex % colors.Length];
        }

        protected override void releaseAction()
        {
            if (Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = 0.35f * SPEED_LIMIT *
                    (Projectile.position - Projectile.oldPos[0]).SafeNormalize(Main.rand.NextVector2Unit());
            }
            base.releaseAction();
        }
    }
}
