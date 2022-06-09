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
    public class TrailingStarChua: TrailingStarChaotic
    {
        protected bool stopAcc;
        protected bool stopDec;

        public static Color[] colors = { Color.LightSeaGreen, Color.Cyan, Color.Blue, Color.LightBlue};

        public float AccTimer
        {
            get { return Projectile.ai[1]; }
            set { Projectile.ai[1] = value; }
        }

        public override float STEP => 1;

        public override Matrix Transform =>
            Matrix.CreateScale(0.65f)
            * Matrix.CreateTranslation(-40f, 5f, 0)
            * Matrix.CreateRotationY(MathHelper.Pi/3);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Chua");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-蔡氏电路");
            base.SetStaticDefaults();
        }
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = (int)(1.5 * 60);
            stopAcc = false;
            stopDec = false;
            AccTimer = 0;
        }

        protected override void motionUpdate()
        {
            float a = 36, b = 3, c = 20, u = -15.15f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                a*(y - x),
                (1 - z) * x + c * y + u,
                x*y - b*z)/60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT) {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;
            
            base.motionUpdate();
        }

        protected override void releaseAction()
        {
            // initialize
            if (AccTimer == 0)
            {
                Projectile.velocity = 0.35f * SPEED_LIMIT *
                    (Projectile.position - Projectile.oldPos[0]).SafeNormalize(Main.rand.NextVector2Unit());
            }
            else if (!stopDec) {
                Projectile.velocity *= 0.935f;
                if (Projectile.velocity.Length() <= 10f)
                    stopDec = true;
            } else
                chase();

            Projectile.rotation = Projectile.velocity.ToRotation();

            AccTimer++;
        }

        private void chase()
        {
            if (stopAcc)
            {
                return;
            }

            if (target == null || !target.active)
            {
                float minDist = 2000f;
                foreach (var player in Main.player)
                {
                    if (player.active && Vector2.Distance(player.Center, Projectile.Center) < minDist)
                    {
                        minDist = Vector2.Distance(player.Center, Projectile.Center);
                        target = player;
                    }
                }
            }

            if (target == null || !target.active)
                return;

            Vector2 disp = target.Center - Projectile.Center;
            if (Projectile.velocity.Length() >= SPEED_LIMIT * 0.35 || disp.Length() < 250f)
                stopAcc = true;
            else if (target.active)
            {
                Projectile.velocity = 0.85f * Projectile.velocity +
                    3f * disp.SafeNormalize(Vector2.UnitX);
            }
        }

        public override void setColor(int colorIndex)
        {
            //drawColor = colors[colorIndex % colors.Length];

            float factor = Main.rand.NextFloat() * (255 + 155);

            drawColor = getColorOnFactor(factor);
        }

        protected override Color colorFun(float progress)
        {
            if (released)
                return Color.Lerp(drawColor, Color.White, progress) * 2f;

            float factor = (255 + 155) * progress;

            return getColorOnFactor(factor);
        }

        private static Color getColorOnFactor(float factor)
        {
            return new Color(factor < 255 ? 0 : (factor - 255), factor < 255 ? (255 - factor) : 0, 255) * 0.8f;
        }
    }
}
