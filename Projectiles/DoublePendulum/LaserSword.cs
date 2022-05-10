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

namespace PhysicsBoss.Projectiles.DoublePendulum
{
    public class LaserSword:ModProjectile
    {
        private VertexStrip tail = new VertexStrip();
        public const int TRAILING_CONST = 15;
        public const int TRANSIT = 15;

        private Texture2D backTex;
        
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;

        private Vector2 dir;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Laser Sword");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "激光刃");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(3.75 * 60);
            Projectile.damage = 25;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            backTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Asset/White").Value;
        }

        public override void AI()
        {
            if (Timer == 0)
            {
                dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.rotation = dir.ToRotation();
                Projectile.velocity *= 0;
            }
            else if (Timer > TRANSIT) {
                Projectile.velocity += 0.5f * dir;
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowRod).noGravity = true;
                }
            }

            Timer++;

        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Timer < TRANSIT) {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    Projectile.Center + Projectile.rotation.ToRotationVector2(),
                    Color.Blue * 0.8f * Math.Min(1, (Timer * 2 / (float)TRANSIT)), 8);
            }
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = backTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.Blue * (float)Math.Pow(1f - progress, 0.5),
                progress => Projectile.height * 0.35f,
                Projectile.Size / 2 - Projectile.width * 0.5f * Projectile.rotation.ToRotationVector2()
                - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White,
                Projectile.rotation, Projectile.Size / 2, 1f, SpriteEffects.None, 0);

            //Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3());
        }
    }
}
