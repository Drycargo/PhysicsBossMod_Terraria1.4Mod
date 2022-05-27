using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
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

namespace PhysicsBoss.Projectiles.ConwayGame
{
    public class ConwayGlider: ModProjectile
    {
        public const float INIT_SPEED = 7.5f;
        
        public static readonly int TRAILING_CONST = 15;
        private VertexStrip tail = new VertexStrip();
        private Vector2 initialDirection = Vector2.Zero;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Conway Glider");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "康威滑翔机");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(1.75 * 60);
            Projectile.damage = 50;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

            Projectile.frame = 4;
            Projectile.frameCounter = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height / (Projectile.frame);
            Projectile.rotation = -MathHelper.PiOver4;

            Timer = 0;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 25; i++) {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0,0,DustID.Flare);
                d.velocity *= 5;
            }
        }
        public override void AI()
        {
            if (initialDirection == Vector2.Zero) {
                initialDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            }

            if (Projectile.velocity.Length() <= 1.2 * INIT_SPEED) {
                Projectile.velocity -= initialDirection * 0.25f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4;
            Timer++;
            if ((int)Timer % 15 == 0)
            {
                Projectile.frameCounter++;
                Projectile.frameCounter %= Projectile.frame;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            specialDraw();
            return false;
        }

        public void specialDraw()
        {
            // pre
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/LuminanceGradientRepeat").Value;

            PhysicsBoss.shineEffect.Parameters["timer"].SetValue((float)Main.time * 0.01f);
            PhysicsBoss.shineEffect.Parameters["tex0"].SetValue(
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/ColorGradient").Value);
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["DynamicColorTail"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, progress => Color.White * (1 - progress),
                progress => Projectile.width / 2,
                Projectile.Size / 2 - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // post
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            PhysicsBoss.shineEffect.Parameters["timer"].SetValue((float)Main.time * 0.01f);
            PhysicsBoss.shineEffect.Parameters["tex0"].SetValue(
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/ColorGradient").Value);
            PhysicsBoss.shineEffect.Parameters["texSize"].SetValue(tex.Size());
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["DynamicContour"].Apply();

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                new Rectangle(0, Projectile.frameCounter * (tex.Height / Projectile.frame), tex.Width, (tex.Height / Projectile.frame)),
                Color.White, Projectile.rotation, Projectile.Size / 2, 1f, SpriteEffects.None, 1);

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
