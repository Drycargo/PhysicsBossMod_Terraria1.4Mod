using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles.TrailingStarMotion;
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
    public class TrailingStarHalvorsenRaise : TrailingStarHalvorsen
    {
        public const int LASER_PERIOD = (int)(2 * 60);
        public override float DECCELERATE => 0.7f;

        private Projectile laser;
        private int clockwise;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Halvorsen");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-Halvorsen吸引子");
            base.SetStaticDefaults();
        }

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            laser = null;
            clockwise = 1;
            Projectile.timeLeft = 10 * (LASER_PERIOD + 60);
        }

        protected override void aimLaser()
        {
            if (Projectile.timeLeft <= LASER_PERIOD)
            {
                drawRayLine = false;
                if (Projectile.timeLeft == LASER_PERIOD)
                    laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(),
                        Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlockFractalLaser>(),
                        70, 0);
                if (laser != null)
                {
                    float progress = ((float)LASER_PERIOD - (float)Projectile.timeLeft) / (float)LASER_PERIOD;
                    laser.timeLeft = Projectile.timeLeft;
                    laser.rotation = -MathHelper.PiOver2 + 
                        clockwise * 5 * MathHelper.PiOver4 
                        *(float)Math.Pow(0.5f*(1f - Math.Cos(progress* MathHelper.Pi)), 3);
                }
            }
            else {
                drawRayLine = true;
            }
        }

        public override void PostDraw(Color lightColor)
        {
            if (drawRayLine)
            {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    Projectile.Center - Vector2.UnitY,
                    drawColor * 0.8f * Math.Min(Math.Min(1, (Projectile.timeLeft - LASER_PERIOD)/15f), 
                    Timer/ 30f), 10);
            }
            specialDraw(lightColor);
        }

        public void changeClockWise() {
            clockwise *= -1;
        }
    }
}
