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
    public class TrailingStarAizawa: TrailingStarChaotic
    {
        public static Color[] colors = { Color.Purple, Color.Violet, Color.BlueViolet, Color.MediumPurple};

        public override Matrix Transform =>
            Matrix.CreateRotationX((float)Main.time/240f * MathHelper.TwoPi)
            *Matrix.CreateRotationY((float)Main.time /240f * MathHelper.TwoPi);
        public override float SHRINK_CONST
        {
            get { return 60f; }
        }
        public override float FOCAL
        {
            get { return 5f; }
        }
        public override float STEP => 5;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Aizawa");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-Aizawa吸引子");
            base.SetStaticDefaults();
        }
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = 2 * 60;

        }

        protected override void motionUpdate()
        {
            float a = 0.95f, b = 0.7f, c = 0.6f
                , d = 3.5f, e = 0.25f, f = 0.1f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                (z - b) * x - d*y,
                d*x + (z - b)*y,
                c + a*z - z*z*z/3 - (x*x + y*y)*(1 + e*z) + f * z * x*x*x) / 60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            if(Main.rand.NextFloat() < 0.05)
                Dust.NewDustDirect(Projectile.Center, 0,0,DustID.RainbowRod).noGravity = true;

            base.motionUpdate();
        }

        protected override void releaseAction()
        {
            Projectile.Kill();
            //base.releaseAction();
        }

        public override void setColor(int colorIndex)
        {
            drawColor = colors[colorIndex % colors.Length];
        }

        protected override float widthFun(float progress)
        {
            return (1f - progress) * tex.Width * 0.35f;
        }
    }
}
