using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace PhysicsBoss.Skies
{
    public class OpenTheGate : CustomSky
    {
        public const float PERIOD = 12 * 60;
        private float progress = 0f;
        private bool active;
        private int bossIndex = -1;

        private Texture2D tex = ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNMotionSymmetric").Value;
        private Texture2D colorMap = ModContent.Request<Texture2D>("PhysicsBoss/Asset/ColorMap").Value;
        private Texture2D bg = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Galaxy").Value;
        private float timer = 0;
        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
            bossIndex = (int)args[0];
            progress = 0;
        }

        public override void Deactivate(params object[] args)
        {
            progress = 0f;
            bossIndex = -1;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0
                && IsActive())
            {
                int rectStart = (int)((1f - progress) / 2f * Main.screenWidth);
                Rectangle rec = new Rectangle(rectStart, 0, (int)(Main.screenWidth * progress), Main.screenHeight);

                GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
                RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width, Main.screenTarget.Height);

                Main.spriteBatch.End();
                graphicsDevice.SetRenderTarget(screenTemp);
                graphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);


                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.Additive);

                PhysicsBoss.maskEffect.Parameters["texSize"].SetValue(Main.ScreenSize.ToVector2());
                PhysicsBoss.maskEffect.Parameters["polarizeShrink"].SetValue(2f);
                PhysicsBoss.maskEffect.Parameters["dispTimer"].SetValue(-timer * 0.025f);
                PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicPolarize"].Apply();

                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                Main.spriteBatch.Draw(tex, new Rectangle(0,0,Main.screenWidth, Main.screenHeight), Color.White);

                Main.spriteBatch.End();
                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.Additive);

                PhysicsBoss.worldEffect.Parameters["lum"].SetValue(0.15f);
                PhysicsBoss.worldEffect.Parameters["grayProgress"].SetValue(progress * 10f);
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["GrayScaleWithLum"].Apply();
                Main.spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);

                /*
                PhysicsBoss.maskEffect.Parameters["timer"].SetValue((float)Main.time * 0.001f);
                PhysicsBoss.maskEffect.Parameters["range"].SetValue(0.15f);
                PhysicsBoss.maskEffect.Parameters["transparency"].SetValue(progress * 5f);
                PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorRange"].Apply();
                Main.spriteBatch.Draw(bg, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                */
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.Additive);
                Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, 
                    new Rectangle(rec.Left - 3, 0, rec.Width + 6, rec.Height), Color.Purple);


                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.NonPremultiplied);
                PhysicsBoss.maskEffect.Parameters["phaseTimer1"].SetValue(timer * 0.025f);
                PhysicsBoss.maskEffect.Parameters["phaseTimer2"].SetValue(timer * 0.025f);
                PhysicsBoss.maskEffect.Parameters["texColorMap"].SetValue(colorMap);
                PhysicsBoss.maskEffect.Parameters["brightThreshold"].SetValue(0.8f);
                PhysicsBoss.maskEffect.Parameters["darkThreshold"].SetValue(0.3f);
                PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorMap"].Apply();
                Main.spriteBatch.Draw(Main.screenTargetSwap, rec, rec, Color.White);
                

                Main.spriteBatch.End();

                Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
                

                if (progress < 0) {
                    Vector2 pos = Main.rand.NextVector2FromRectangle(rec);
                    //Vector2 pos = new Vector2(Main.rand.NextBool() ? rec.Left : rec.Right, Main.rand.NextFloat() * Main.screenHeight);
                    Vector2 vel = (pos - Main.ScreenSize.ToVector2()/2).SafeNormalize(Vector2.UnitX) * 16f;
                    ParticleOrchestraSettings settings = new ParticleOrchestraSettings
                    {
                        PositionInWorld = pos + Main.screenPosition,
                        MovementVector = vel
                    };

                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.RainbowRodHit, settings);
                }
            }
        }

        public override bool IsActive()
        {
            return (active && !bossInactive()) || progress > 0;
        }

        public override void Reset()
        {
            bossIndex = -1;
            active = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (active)
            {
                if (bossInactive())
                {
                    active = false;
                    bossIndex = -1;
                }
            }

            if (active && progress < 1f)
                progress += 1f / PERIOD;
            else if (!active && progress > 0)
                progress -= 0.05f;

            timer++;
            timer %= 1000000;
        }

        private bool bossInactive()
        {
            return bossIndex >= 0 && (!Main.npc[bossIndex].active || Main.npc[bossIndex].life <= 0
                                || Main.npc[bossIndex].type != ModContent.NPCType<ChaosTheory>());
        }

    }
}
