using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.LogisticMap
{
    public class BlockMap: LogisticMap
    {
        public override float LASER_WIDTH => 50f;
        public override float WIDTH => 1250f;
        public override float HEIGHT => 700f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Block Logistic Map");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "块状单峰映射");
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
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < STEP - 1; i++)
                    {
                        Dust.NewDust(Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], Main.rand.NextFloat())
                            + (Main.rand.NextFloat() - 0.5f) * LASER_WIDTH * (Projectile.oldRot[i]
                            + MathHelper.Pi).ToRotationVector2(), 0, 0, ModContent.DustType<BlockDust>());
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
                        drawInitialized(Color.LightBlue * 0.5f);
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
                        BlockDust.drawAll(Main.spriteBatch);
                        Main.spriteBatch.End();

                        graphicsDevice.SetRenderTarget(Main.screenTarget);
                        graphicsDevice.Clear(Color.Transparent);
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate,BlendState.NonPremultiplied);

                        Main.spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
                        drawEdges();

                        PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.99f);
                        PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(contentTex);
                        PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue(Color.LightBlue.ToVector4());
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
            summonDust(Color.LightBlue);
        }
    }
}
