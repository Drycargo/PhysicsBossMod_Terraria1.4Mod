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
    public class TrailingStarLorenz:TrailingStarChaotic
    {
        public const int FRAME_COUNT = 3;

        private float zoom;
        public override float STEP => 2;
        public override Matrix Transform =>
            Matrix.CreateRotationX(MathHelper.PiOver2)
            * Matrix.CreateTranslation(0, 25, 0)
            * Matrix.CreateScale(zoom);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Lorenz");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-洛伦茨吸引子");

            base.SetStaticDefaults();
        }
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();

            Projectile.width = tex.Width;
            Projectile.height = tex.Height / FRAME_COUNT;

            Projectile.timeLeft = (int)(1.5 * 60);
            drawColor = Color.White;
            Projectile.frameCounter = 0;

            zoom = 0.001f;
        }

        public override void AI()
        {
            base.AI();
            if ((int)Timer % 5 == 0) {
                Projectile.frameCounter++;
                Projectile.frameCounter %= FRAME_COUNT;
            }

            if (Main.rand.NextFloat() < 0.25f) {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowMk2);
                d.color = drawColor;
                d.noGravity = true;
            }

            if (zoom < 1.5)
                zoom += 1.5f / 180f;
        }

        protected override void motionUpdate()
        {
            float p = 28, s = 10, b = 8f / 3f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
               s * (y - x),
               x * (p - z) - y,
               x * y - b * z) / 60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            base.motionUpdate();
        }

        protected override Color colorFun(float progress)
        {
            return Main.hslToRgb(progress, 0.8f, 0.75f, (byte)(255 * (1 - progress)));
        }

        protected override float widthFun(float progress)
        {
            return (1f - progress) * tex.Width * 0.35f;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                new Rectangle(0, Projectile.frameCounter * Projectile.height, Projectile.width, Projectile.height),
                drawColor, 0, Projectile.Size / 2, 1f, (Projectile.velocity.X < 0 ? SpriteEffects.None: SpriteEffects.FlipVertically), 0);

            Lighting.AddLight(Projectile.Center, drawColor.ToVector3());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
