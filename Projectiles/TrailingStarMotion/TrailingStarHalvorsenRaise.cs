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
        private float angle;
        private float initialAngle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Halvorsen Raise");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-Halvorsen吸引子-上升激光");
            base.SetStaticDefaults();
        }

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            laser = null;
            clockwise = 1;
            Projectile.timeLeft = 3 * (LASER_PERIOD + 60);
            initialAngle = -MathHelper.PiOver2;
            angle = initialAngle;
            drawRayLine = true;
        }

        protected override void aimLaser()
        {
            if (Projectile.timeLeft <= LASER_PERIOD)
            {
                if (Projectile.timeLeft == (int)(LASER_PERIOD * 0.5))
                {
                    drawRayLine = false;
                    laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(),
                        Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlockFractalLaser>(),
                        70, 0);
                    ((BlockFractalLaser)laser.ModProjectile).setColor(drawColor);
                }
                
                float progress = ((float)LASER_PERIOD - (float)Projectile.timeLeft) / (float)LASER_PERIOD;
                angle = initialAngle +
                    clockwise * (MathHelper.TwoPi * 5f/6f)
                    * progress * progress * progress;

                if (laser != null)
                {
                    laser.timeLeft = Projectile.timeLeft;
                    laser.rotation = angle;
                }
            }
        }

        public override void PostDraw(Color lightColor)
        {
            if (drawRayLine)
            {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    Projectile.Center + angle.ToRotationVector2(),
                    drawColor * 0.8f * Math.Min(1,Timer/ 30f), 10);
            }
            specialDraw(lightColor);
        }

        public void changeClockWise() {
            clockwise *= -1;
        }


        public override void releaseProj(Player target)
        {
            if (target != null)
            {
                initialAngle = (target.Center - Projectile.Center).ToRotation() + MathHelper.Pi * 15 / 180;
                angle = initialAngle;
            }
            base.releaseProj(target);
        }
    }
}
