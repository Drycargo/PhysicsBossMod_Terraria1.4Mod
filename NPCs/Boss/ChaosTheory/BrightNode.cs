using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles;
using PhysicsBoss.Projectiles.DoublePendulum;
using PhysicsBoss.Projectiles.TrailingStarMotion;
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

namespace PhysicsBoss.NPCs.Boss.ChaosTheory
{
    public class BrightNode:NodeMinion
    {
        public const int SINGLE_PENDULUM_DIST = 200;
        public const double SINGLE_PENDULUM_PERIOD = 0.8;
        public const float HALVORSEN_PERIOD = 5.35f * 60/2.5f;
        public const float HALVORSEN_FINALE_PERIOD = HALVORSEN_PERIOD/2;


        public static readonly int TRAILING_CONST = 15;
        private VertexStrip tail = new VertexStrip();
        private Texture2D backTex;
        private Texture2D luminanceTex;
        private Texture2D colorTex;

        private Projectile[] triLasers;

        private Projectile[] sinlasers;

        private bool drawTriLasers;
        private float triLaserAngle;
        public enum phase
        {
            SIGNLE_PENDULUM_TWO = 0,
            ORBIT = 1,
            CHUA_CIRCUIT = 2,
            CHUA_CIRCUIT_FINALE = 3,
            HALVORSEN = 4,
            HALVORSEN_FINALE = 5,
            DOUBLE_PENDULUM_PREPARATION = 6,
            DOUBLE_PENDULUM_ONE = 7,
            DOUBLE_PENDULUM_TWO = 8,
            THREEBODY_MOTION = 9,
            SPIRAL_SINK = 10,
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

            NPC.lifeMax = 11000;
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

            triLasers = new Projectile[3];
            drawTriLasers = false;
            triLaserAngle = MathHelper.PiOver2;
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
                            if (drawTrail != trail.SHADOW)
                                drawTrail = trail.SHADOW;
                            if (NPC.dontTakeDamage)
                                NPC.dontTakeDamage = false;
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
                    case (int)phase.HALVORSEN: {
                            halvorsen();
                            break;
                        }
                    case (int)phase.HALVORSEN_FINALE: {
                            if (drawTrail != trail.TAIL)
                                drawTrail = trail.TAIL;
                            halvorsenFinale();
                            break;
                        }
                    case (int)phase.DOUBLE_PENDULUM_PREPARATION: {
                            if (drawTrail != trail.SHADOW) {
                                drawTrail = trail.SHADOW;
                            }

                            if (Timer < 30)
                                orbit((owner.GeneralTimer / ORBIT_PERIOD + 0.5f) * MathHelper.TwoPi);
                            else
                            {
                                hover(owner.NPC.Center - ChaosTheory.DOUBLE_PENDULUM_TOTAL_LENGTH * Vector2.UnitY, 20, 0.3f, 60); 
                            }

                            Timer++;
                            break;
                        }
                    case (int)phase.DOUBLE_PENDULUM_ONE: {
                            if (!drawConnection)
                                drawConnection = true;
                            if (drawTrail != trail.TAIL)
                                drawTrail = trail.TAIL;
                            doublePendulumOne();
                            break;
                        }
                    case (int)phase.DOUBLE_PENDULUM_TWO: {
                            if (!drawConnection)
                                drawConnection = true;
                            if (drawTrail != trail.DEFAULT)
                                drawTrail = trail.DEFAULT;
                            if (!drawTriLasers)
                                drawTriLasers = true;
                            doublePendulumTwo();
                            break;
                        }
                    case (int)phase.THREEBODY_MOTION:
                        {
                            if (drawTriLasers)
                                drawTriLasers = false;
                            if (drawTrail != trail.SHADOW)
                                drawTrail = trail.SHADOW;
                            if (drawConnection)
                                drawConnection = false;
                            orbit((owner.GeneralTimer / ORBIT_PERIOD + 0.5f) * MathHelper.TwoPi);
                            break;
                        }
                    case (int)phase.SPIRAL_SINK:
                        {
                            if (!NPC.dontTakeDamage)
                                NPC.dontTakeDamage = true;
                            orbit((owner.GeneralTimer / (ORBIT_PERIOD * 0.5f) + 0.5f) * MathHelper.TwoPi, ChaosTheory.SPIRAL_SINK_RADIUS);
                            break;
                        }
                    default: break;
                }
            }
            base.AI();
        }

        private void doublePendulumTwo()
        {
            foreach (Projectile l in triLasers) {
                if (l != null) {
                    l.Center = NPC.Center;
                }
            }
        }

        public void summonTriLasers() {
            for (int i = 0; i < 3; i++) {
                triLasers[i] = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlockFractalLaser>(),100,0);
                triLasers[i].rotation = triLaserAngle + MathHelper.TwoPi / 3 * (float)i;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)NPC.frameCounter * NPC.height;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawConnectionLine(spriteBatch, Color.Red * 1.5f, 10f);

            spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height), Color.White,
                NPC.rotation, tex.Size()/2, 1f, SpriteEffects.None, 0);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (drawTrail == trail.SHADOW)
                drawShadow(spriteBatch, Color.Red);
            else if (drawTrail == trail.TAIL)
                drawTail(spriteBatch, Color.OrangeRed * 3.5f);

            if (drawTriLasers) {
                for (int i = 0; i < 3; i++) {
                    GlobalEffectController.drawRayLine(spriteBatch, NPC.Center,
                        NPC.Center + (triLaserAngle + (float)i * MathHelper.TwoPi/3).ToRotationVector2(),
                        Color.Red*0.6f, 20f);
                }
            }

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
                BlendState.AlphaBlend,
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

 
        private void chuaCircuit()
        {
            /*
            Vector2 dist = //(target.Center - owner.NPC.Center).SafeNormalize(Vector2.UnitX);
                (MathHelper.Pi * 5f / 6 + (float)Math.Sin(Timer / 60f) / 36f).ToRotationVector2();
            */
            Vector2 dist = (-MathHelper.PiOver2 + (float)Math.Sin(Timer / 60f) / 36f).ToRotationVector2();
            float angle = (float)(MathHelper.Pi / (4.5f + 1.5f * Math.Min(1, Timer / 60))
                + Math.Sin(Timer / 60f) / 24f);

            Vector2 dir1 = dist.RotatedBy(angle);

            Vector2 dir2 = dist.RotatedBy(-angle);

            if ((int)Timer == 0)
            {
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

        private void chuaCircuitFinale()
        {
            for (int i = 0; i < 2; i++)
            {
                if (sinlasers[i] != null && sinlasers[i].timeLeft > (int)SinLaser.TRANSIT)
                    sinlasers[i].timeLeft = (int)SinLaser.TRANSIT;
            }
        }
        
        private void halvorsen()
        {
            if (trailingStarController == null)
            {
                Projectile p = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<TrailingStarController>(), 0, 0);
                trailingStarController = (TrailingStarController)p.ModProjectile;

                for (int i = 0; i < 3; i ++)
                    trailingStarController.summonStarBundle<TrailingStarHalvorsen>();
                Timer = 0;
            }

            /*
            hover(target.Center +
                500f * ((float)(MathHelper.Pi / 3 * Math.Cos(Timer / (ChaosTheory.CHAOTIC_DURATION / MathHelper.TwoPi))
                - MathHelper.Pi/ 7f)).ToRotationVector2(),
                30, 0.3f, 1200);
            */

            // movement
            int factor = (int)Timer % (int)ChaosTheory.CHAOTIC_DURATION;
            int posIndex = (int)Timer / (int)ChaosTheory.CHAOTIC_DURATION;
            if (factor == 0) {
                fireWork();
                NPC.Center = new Vector2(target.Center.X + (posIndex % 2 == 0 ? -1 : 1) * 450f, target.Center.Y - 600f);
                fireWork();
            }

            float x = 2 * (float)factor / (float)((int)ChaosTheory.CHAOTIC_DURATION) - 1;

            NPC.Center = new Vector2(target.Center.X + (posIndex % 2 == 0 ? -1 : 1) * 450f, MathHelper.Lerp(- 600, 600, (x * x * x + 1f)/2f) +  target.Center.Y);

            trailingStarController.Projectile.Center = NPC.Center;


            // fire lasers
            if (factor == 0 || factor == 10)
                trailingStarController.releaseStarBundle(target);
            else if (factor == (int)(ChaosTheory.CHAOTIC_DURATION / 2)
                || factor == (int)(ChaosTheory.CHAOTIC_DURATION / 2) + 10)
                trailingStarController.summonStarBundle<TrailingStarHalvorsen>();


            trailingStarController.Projectile.timeLeft++;

            Timer++;
        }

        private void halvorsenFinale()
        {
            NPC.velocity *= 0;
            hyperbolicMotion();
            if ((int)Timer == 0) {
                trailingStarController.Projectile.Kill();
                Projectile p = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<TrailingStarController>(), 0, 0);
                trailingStarController = (TrailingStarController)p.ModProjectile;
                trailingStarController.summonStarBundle<TrailingStarHalvorsenRaise>();
            }

            if (trailingStarController!=null)
                trailingStarController.Projectile.Center = NPC.Center;

            if ((int)Timer == (int)(HALVORSEN_PERIOD) + 29) {
                foreach (Projectile p in Main.projectile)
                {
                    if (p.type == ModContent.ProjectileType<TrailingStarHalvorsenRaise>())
                    {
                        if (p.Center.X < target.Center.X)
                        {
                            ((TrailingStarHalvorsenRaise)p.ModProjectile).changeClockWise();
                        }
                    }
                }
                Timer = 0;
                setPhase((int)phase.DOUBLE_PENDULUM_PREPARATION);
            } else if ((int)Timer == (int)(HALVORSEN_PERIOD)) {
                trailingStarController.Projectile.Kill();
                trailingStarController = null;
                foreach (Projectile p in Main.projectile){
                    if (p.type == ModContent.ProjectileType<TrailingStarHalvorsenRaise>()) {
                        p.timeLeft = 30 + TrailingStarHalvorsenRaise.LASER_PERIOD;
                    }
                }
            } else if ((int)Timer < (int)(HALVORSEN_PERIOD) &&
                (int)Timer % (int)(0.3 * HALVORSEN_PERIOD) == 0)
            {
                trailingStarController.summonStarBundle<TrailingStarHalvorsenRaise>();
                trailingStarController.releaseStarBundle(); // CHANGED
            }

            Timer++;
        }

        private void hyperbolicMotion()
        {
            float progress;
            if (Timer % (2 * HALVORSEN_FINALE_PERIOD) > HALVORSEN_FINALE_PERIOD)
                progress = 2 * (HALVORSEN_FINALE_PERIOD - (Timer % HALVORSEN_FINALE_PERIOD)) / HALVORSEN_FINALE_PERIOD - 1;
            else
                progress = 2 * (Timer % HALVORSEN_FINALE_PERIOD) / HALVORSEN_FINALE_PERIOD - 1;

            float altitude = 500 * progress;
            float radius = (float)(progress * progress) * 400 + 200;
            float innerAngle = (float)(MathHelper.TwoPi * ((Timer / (0.2 * HALVORSEN_FINALE_PERIOD)) % 1f));

            Vector3 realPos = new Vector3(
                radius * (float)Math.Cos(innerAngle),
                altitude,
                radius * (float)Math.Sin(innerAngle));

            float adjustment =  (float)Math.Atan(realPos.Z) / (MathHelper.PiOver2) / 400;

            NPC.Center = new Vector2(
                target.Center.X + realPos.X * (1f + adjustment),
                target.Center.Y + realPos.Y * (1f + adjustment));
        }

        private void doublePendulumOne()
        {
            Timer++;
        }

        public void summonLaserPairNormal()
        {
            summonLaserPair(0);
        }

        public void summonLaserPairTangent()
        {
            summonLaserPair(MathHelper.PiOver2);
        }

        private void summonLaserPair(float dev)
        {
            if (owner == null || owner.dimNode == null)
                return;
            Vector2 dir = (NPC.Center - owner.dimNode.NPC.Center).RotatedBy(
                                dev).SafeNormalize(Vector2.Zero);
            LaserSword a = (LaserSword)(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + dir * 10f, dir,
                ModContent.ProjectileType<LaserSword>(), 25, 0).ModProjectile);

            float progress = (Timer % ChaosTheory.DOUBLE_PENDULUM_PERIOD)/ChaosTheory.DOUBLE_PENDULUM_PERIOD;
            Color c = Color.Lerp(Color.Red, Color.Gold, progress);

            a.setColor(c);
            LaserSword b = (LaserSword)(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center - dir * 10f, -dir,
                ModContent.ProjectileType<LaserSword>(), 25, 0).ModProjectile);
            b.setColor(c);
        }

        public void fireWork()
        {
            fireWork(Color.Pink);
        }

        public void setTriLaserAngle(float angle) {
            triLaserAngle = angle;
        }
    }
}
