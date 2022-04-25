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
    public class NewtonBeamLong: ModProjectile
    {
        private bool released;
        public static readonly int TRAILING_CONST = 20;

        private Texture2D tex;

        private VertexStrip tail = new VertexStrip();
        private Texture2D backTex;
        private Texture2D luminanceTex;
        private Texture2D colorTex;
        private Player target;

        private NewtonBeamShort appendage;
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Newton Beam Long");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "长柄");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(3.5 * 60);
            Projectile.damage = 75;
            Timer = 0;

            target = null;
            appendage = null;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            backTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNMotion").Value;
            luminanceTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/LuminanceGradientTopWhiteHalf").Value;
            colorTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/RedOrangeGradient").Value;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            if (target != null) {
                Projectile.velocity = 2.5f * (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            }

            if (appendage != null) {
                appendage.Projectile.Center = Projectile.Center + 
                    Projectile.height/2 * Vector2.UnitY.RotatedBy(Projectile.rotation);
            }

            if ((int)(Timer % 60)==0)
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing);

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
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenWidth, Main.screenHeight);

            #region drawDis
            /*
            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Displacement"].Apply();
            PhysicsBoss.trailingEffect.Parameters["tex0"].SetValue(Main.screenTargetSwap);
            PhysicsBoss.trailingEffect.Parameters["intensity"].SetValue(0.02f);

            spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
            //spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            
            spriteBatch.End();
            */
            #endregion

            #region drawtail
            
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            var proj = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));

            
            PhysicsBoss.trailingEffect.Parameters["uTransform"].SetValue(model * proj);
            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time * 0.1f);

            Main.graphics.GraphicsDevice.Textures[0] = backTex;
            Main.graphics.GraphicsDevice.Textures[1] = luminanceTex;
            Main.graphics.GraphicsDevice.Textures[2] = colorTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;

            //PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DefaultTrail"].Apply();
            Vector2[] pos = new Vector2[TRAILING_CONST];
            for (int i = 0; i < TRAILING_CONST; i++)
                pos[i] = Projectile.position - Main.screenPosition;
            
            tail.PrepareStrip(pos, Projectile.oldRot, progress => Color.White * (1 - progress) * 0.5f,
                progress => Projectile.height * 0.5f,
                tex.Size() / 2, TRAILING_CONST);
            tail.DrawTrail();

            spriteBatch.End();
            
            #endregion


            graphicsDevice.SetRenderTarget(screenTemp);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Displacement"].Apply();
            PhysicsBoss.trailingEffect.Parameters["tex0"].SetValue(Main.screenTargetSwap);
            PhysicsBoss.trailingEffect.Parameters["intensity"].SetValue(0.02f);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Displacement"].Apply();
            //PhysicsBoss.trailingEffect.Parameters["tex0"].SetValue(Main.screenTargetSwap);
            spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
            //spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.End();



            /*
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Default"].Apply();
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.End();*/

            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                Color.White, Projectile.rotation, tex.Size() / 2, 1f, SpriteEffects.None, 0);
        }

        public override void Kill(int timeLeft)
        {
            if (appendage != null)
            {
                appendage.release();
                appendage.setTarget(target);
                appendage.Projectile.penetrate = 1;

                for (int i = 0; i < 30; i++)
                {
                    Dust d = Dust.NewDustDirect(appendage.Projectile.Center, 0, 0, DustID.PinkFairy);
                    d.velocity = Main.rand.NextVector2Unit() * 15;
                    d.noGravity = true;
                }
            }
            base.Kill(timeLeft);
        }

        public void initialize(Player t)
        {
            target = t;
            Projectile p = 
                Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(),
                Projectile.Center, Vector2.Zero, ModContent.ProjectileType<NewtonBeamShort>(), 50, 0);
            appendage = (NewtonBeamShort)p.ModProjectile;
        }
    }
}
