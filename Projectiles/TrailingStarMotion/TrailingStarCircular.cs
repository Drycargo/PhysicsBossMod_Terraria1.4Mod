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
    public class TrailingStarCircular : TrailingStarPlain
    {
        private float radius;
        private int clockwise;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Circular");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星圆弧");
            base.SetStaticDefaults();
        }

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            radius = 0;
            clockwise = 1;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            Projectile.velocity += Projectile.velocity.LengthSquared() / radius 
                *Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(
                    (float)clockwise * MathHelper.PiOver2);
            base.AI();
        }

        public void changeClockWise() {
            clockwise *= -1;
        }

        public void setRadius(float r) {
            radius = r;
        }

        protected override Color colorFun(float progress)
        {
            Color c1 = Color.Green * (1f - progress);
            Color c2 = Color.Cyan * progress;
            return  new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
        }
    }
}
