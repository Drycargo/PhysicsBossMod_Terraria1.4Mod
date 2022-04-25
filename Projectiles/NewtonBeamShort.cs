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

namespace PhysicsBoss.Projectiles
{
    public class NewtonBeamShort:ModProjectile
    {
        private bool released;
        public static readonly int TRAILING_CONST = 10;

        private Texture2D tex;
        private Vector2 targetVel;

        private VertexStrip tail = new VertexStrip();
        private Texture2D backTex;
        private Texture2D luminanceTex;
        private Texture2D colorTex;
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Newton Beam Short");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "短柄");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(5 * 60);
            Projectile.damage = 50;
            Timer = 0;
            targetVel = Vector2.Zero;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            backTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNNormal").Value;
            luminanceTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/LuminanceGradientTopWhite").Value;
            colorTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/RedOrangeGradient").Value;
        }

        public override void AI()
        {
            Projectile.rotation += 0.3f;
            if (released)
            {
                if (Projectile.velocity.Length() < 45)
                    Projectile.velocity += 0.3f * targetVel;
            }
            else {
                for (int i = 0; i < 3; i++)
                { 
                    Dust d = Dust.NewDustDirect(Projectile.Center, 10, 10, DustID.PinkCrystalShard);
                    d.noGravity = true;
                }
            }
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            //Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, lightColor);
            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            if (released)
            {
                #region drawtail
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.Additive,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                var proj = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
                var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));

                PhysicsBoss.trailingEffect.Parameters["uTransform"].SetValue(model * proj);
                PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time);

                Main.graphics.GraphicsDevice.Textures[0] = backTex;
                Main.graphics.GraphicsDevice.Textures[1] = luminanceTex;
                Main.graphics.GraphicsDevice.Textures[2] = colorTex;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;

                PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Trail"].Apply();

                tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, progress => Color.White * 0.6f,
                    progress => MathHelper.Lerp(50f, 0f, progress),
                    tex.Size() / 2, TRAILING_CONST);
                tail.DrawTrail();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.NonPremultiplied,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
                #endregion
            }

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, 
                Color.White, Projectile.rotation, tex.Size()/2, 1f, SpriteEffects.None, 0);
        }

        public void release() { 
            released = true;
        }

        public void setTarget(Player t) {
            targetVel = (t.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
        }
    }
}
