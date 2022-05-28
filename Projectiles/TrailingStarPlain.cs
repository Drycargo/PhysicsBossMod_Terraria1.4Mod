using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles
{
    public class TrailingStarPlain : ModProjectile
    {
        protected VertexStrip tail = new VertexStrip();
        public const int TRAILING_CONST = 25;
        protected Color drawColor = Color.Green;

        protected Texture2D tex;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Plain");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星白板");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            setBasicDefaults();

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        protected virtual void setBasicDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(3.75 * 60);
            Projectile.damage = 50;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        protected virtual float widthFun(float progress){
            return (1f-progress) * tex.Width* 0.2f;
        }

        protected virtual Color colorFun(float progress) {
            return drawColor * 0.8f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = 
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvanceTexture").Value;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                colorFun, widthFun,
                Projectile.Size / 2 - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            return false;
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
                null, drawColor, Projectile.rotation, Projectile.Size /2, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                null, Color.White, Projectile.rotation, Projectile.Size / 2, 0.6f, SpriteEffects.None, 0);

            Lighting.AddLight(Projectile.Center, drawColor.ToVector3());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public virtual void setColor(int colorIndex)
        {
        }
        public void setColor(Color c) { 
            drawColor = c;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < TRAILING_CONST - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    break;
                
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.oldPos[i] + Projectile.Size / 2, Projectile.oldPos[i + 1] + Projectile.Size / 2, widthFun((float)i / TRAILING_CONST) / 2, ref point))
                    return true;
            }

            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 30; i++) {
                Vector2 dir = 15 * Main.rand.NextVector2Unit();
                Dust d = Dust.NewDustDirect(Projectile.Center, 0,0,DustID.RainbowTorch, dir.X,dir.Y,0,drawColor * 0.5f, 2f);
                d.noGravity = true;
                d.noLight = false;
            }

            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);
            base.Kill(timeLeft);
        }
    }
}
