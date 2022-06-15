using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.LogisticMap
{
    public class FractalMap: LogisticMap
    {
        public override float LASER_WIDTH => 75f;
        public override float WIDTH => 1250f;
        public override float HEIGHT => 700f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fractal Logistic Map");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "分型图单峰映射");
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            contentTex = ModContent.Request<Texture2D>(Texture).Value;
        }

        public override void AI()
        {
            base.AI();
            if (currPhase != phase.INITIALZIED && currPhase != phase.UNINITIALIZED)
            {
                if (Timer % 1 == 0)
                {
                    for (int i = 0; i < STEP - 1; i++)
                    {
                        Vector2 dir = (Main.rand.NextFloat() - 0.5f) * (Projectile.oldRot[i + 1] + MathHelper.PiOver2).ToRotationVector2();

                        Dust d = Dust.NewDustDirect(Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], Main.rand.NextFloat())
                            + LASER_WIDTH * dir, 0, 0, ModContent.DustType<TwistedDust>());

                        d.velocity += dir * 10f;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            switch (currPhase)
            {
                case phase.UNINITIALIZED:
                    {
                        break;
                    }
                case phase.INITIALZIED:
                    {
                        drawInitialized(Color.Purple * 0.5f);
                        break;
                    }
                default:
                    {
                        drawEdges();
                        /*
                        Main.spriteBatch.End();

                        GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
                        RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width, Main.screenTarget.Height);

                        graphicsDevice.SetRenderTarget(screenTemp);
                        graphicsDevice.Clear(Color.Transparent);
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                        Main.spriteBatch.End();

                        graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                        graphicsDevice.Clear(Color.Transparent);
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        TwistedDust.drawAll(Main.spriteBatch);
                        Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;

                        tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                            progress => Color.White,
                            progress => LASER_WIDTH * 0.75f * Math.Min(1, (float)Projectile.timeLeft / FADE_TRANSIT),
                            -Main.screenPosition, STEP);

                        tail.DrawTrail();
                        Main.spriteBatch.End();

                        graphicsDevice.SetRenderTarget(Main.screenTarget);
                        graphicsDevice.Clear(Color.Transparent);
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                        Main.spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
                        drawEdges();

                        PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.8f);
                        PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(contentTex);
                        PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue(Color.Purple.ToVector4());
                        PhysicsBoss.maskEffect.Parameters["contentTint"].SetValue(Color.White.ToVector4());
                        PhysicsBoss.maskEffect.CurrentTechnique.Passes["MaskTint"].Apply();
                        Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);

                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            RasterizerState.CullNone, null,
                            Main.GameViewMatrix.TransformationMatrix);
                        */
                        break;
                    }
            }

            return false;
        }

        protected override void summonDust()
        {
            summonDust(Color.Purple);
        }
    }
}
