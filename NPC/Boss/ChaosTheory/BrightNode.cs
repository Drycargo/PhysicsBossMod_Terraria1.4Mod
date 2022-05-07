using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles;
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

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    public class BrightNode:NodeMinion
    {
        public static readonly int SINGLE_PENDULUM_DIST = 200;
        public static readonly double SINGLE_PENDULUM_PERIOD = 0.8;

        public static readonly int TRAILING_CONST = 15;
        private VertexStrip tail = new VertexStrip();
        private Texture2D backTex;
        private Texture2D luminanceTex;
        private Texture2D colorTex;

        private Projectile[] sinlasers;
        public enum phase
        {
            SIGNLE_PENDULUM_TWO = 0,
            ORBIT = 1,
            CHUA_CIRCUIT = 2,
            CHUA_CIRCUIT_FINALE = 3,
        }
        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Bright Node");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "明节点");

            Main.npcFrameCount[NPC.type] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = 0;
            tex = ModContent.Request<Texture2D>(Texture).Value;
            NPC.friendly = false;

            NPC.width = tex.Width;
            NPC.height = tex.Height / Main.npcFrameCount[NPC.type];
            NPC.rotation = 0;

            NPC.lifeMax = 1100;
            NPC.defense = 100;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 0, 15, 0);

            Timer = 0f;
            currentPhase = 0;

            NPCID.Sets.TrailingMode[NPC.type] = 0;
            NPCID.Sets.TrailCacheLength[NPC.type] = TRAILING_CONST;

            backTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNMotion").Value;
            luminanceTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/LuminanceGradient").Value;
            colorTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/RedOrangeGradient").Value;

            sinlasers = new Projectile[2];

        }

        public override void AI()
        {
            if (owner != null && target != null && target.active)
            {
                switch (currentPhase)
                {
                    case (int)phase.SIGNLE_PENDULUM_TWO:
                        {
                            singlePendulum();
                            break;
                        }
                    case (int)phase.ORBIT:
                        {
                            if (Timer != 0)
                                Timer = 0;
                            orbit((owner.GeneralTimer / ORBIT_PERIOD + 0.5f) * MathHelper.TwoPi);
                            break;
                        }
                    case (int)phase.CHUA_CIRCUIT: {
                            orbit((owner.GeneralTimer / ORBIT_PERIOD + 0.5f) * MathHelper.TwoPi);
                            chuaCircuit();
                            break;
                        }
                    case (int)phase.CHUA_CIRCUIT_FINALE: {
                            orbit((owner.GeneralTimer / ORBIT_PERIOD + 0.5f) * MathHelper.TwoPi);
                            chuaCircuitFinale();
                            break;
                        }
                    default: break;
                }
            }
            base.AI();
        }

        private void chuaCircuitFinale()
        {
            for (int i = 0; i < 2; i++) {
                if (sinlasers[i] != null && sinlasers[i].timeLeft > (int)SinLaser.TRANSIT)
                    sinlasers[i].timeLeft = (int)SinLaser.TRANSIT;
            }
        }

        private void chuaCircuit()
        {
            Vector2 dist = //(target.Center - owner.NPC.Center).SafeNormalize(Vector2.UnitX);
                (MathHelper.Pi*5.5f/6 + (float)Math.Sin(Timer / 60f) /36f).ToRotationVector2();
            float angle = (float)(MathHelper.Pi / 4.5f + Math.Sin(Timer / 60f) / 24f);

            Vector2 dir1 = dist.RotatedBy(angle);

            Vector2 dir2 = dist.RotatedBy(-angle);

            if ((int)Timer == 0) {
                sinlasers[0] = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<SinLaser>(), 100, 0);
                sinlasers[1] = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<SinLaser>(), 100, 0);
                ((SinLaser)sinlasers[1].ModProjectile).reverseAmp();
            }

            sinlasers[0].rotation = dir1.ToRotation();
            sinlasers[1].rotation = dir2.ToRotation();

            sinlasers[0].Center = NPC.Center;
            sinlasers[1].Center = NPC.Center;

            Timer++;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)NPC.frameCounter * NPC.height;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (drawConnection && owner.dimNode != null)
                drawConnectionLine(spriteBatch, owner.dimNode.NPC.Center, Color.Red * 1.5f, 10f);

            spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height), Color.White,
                NPC.rotation, tex.Size()/2, 1f, SpriteEffects.None, 0);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawShadow(spriteBatch, Color.Red);
            if (currentPhase == (int)phase.SIGNLE_PENDULUM_TWO) {
                drawDisplacement();
            }
            return false;
        }

        protected override void summonEvent()
        {
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
            for (int i = 0; i < 60; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.FlameBurst);
                d.velocity = Main.rand.NextVector2Unit() * 15;
                d.noGravity = true;
            }
        }

        public void summonBareLightning(float vel) {
            if (target != null)
            {
                for (int i = 0; i < 120; i++)
                {
                    Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RedTorch);
                    d.velocity = Main.rand.NextVector2Unit() * 10;
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, 
                    (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX)*vel,
                    ModContent.ProjectileType<LightningBolt>(), 50, 0);
            }
        }

        public void summonBareLightning(Vector2 velocity)
        {
            if (target != null)
            {
                for (int i = 0; i < 120; i++)
                {
                    Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RedTorch);
                    d.velocity = Main.rand.NextVector2Unit() * 10;
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                    velocity, ModContent.ProjectileType<LightningBolt>(), 50, 0);
            }
        }

        private void singlePendulum()
        {
            if (!drawConnection)
                drawConnection = true;
            float angle = -(float)(Timer * MathHelper.TwoPi / (SINGLE_PENDULUM_PERIOD * 60));
            angle %= MathHelper.TwoPi;
            NPC.Center = owner.dimNode.NPC.Center + angle.ToRotationVector2() * SINGLE_PENDULUM_DIST;
            NPC.rotation = angle;

            Timer++;
        }

        private void drawDisplacement()
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenWidth, Main.screenHeight);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(screenTemp);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            #region drawtail

            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = backTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            Vector2[] pos = new Vector2[TRAILING_CONST];

            for (int i = TRAILING_CONST - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    NPC.oldRot[0] = NPC.rotation + MathHelper.PiOver2;
                }
                else
                {
                    NPC.oldRot[i] = NPC.oldRot[i - 1];
                }
                //pos[i] = NPC.oldPos[i] - Main.screenPosition;
                pos[i] = owner.dimNode.NPC.position + (NPC.oldRot[i] - MathHelper.PiOver2).ToRotationVector2() * SINGLE_PENDULUM_DIST / 2 - Main.screenPosition;
            }


            tail.PrepareStrip(pos, NPC.oldRot, progress => Color.White * (1 - progress),
                progress => (1-progress)*SINGLE_PENDULUM_DIST/2,
                tex.Size() / 2, TRAILING_CONST);
            tail.DrawTrail();

            spriteBatch.End();

            #endregion

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Displacement"].Apply();
            PhysicsBoss.trailingEffect.Parameters["tex0"].SetValue(Main.screenTargetSwap);
            PhysicsBoss.trailingEffect.Parameters["intensity"].SetValue(0.05f);
            spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnKill()
        {
            for (int i = 0; i < 2; i++)
            {
                if (sinlasers[i] != null)
                {
                    sinlasers[i].Kill();
                    sinlasers[i] = null;
                }
            }
            base.OnKill();
        }
    }
}
