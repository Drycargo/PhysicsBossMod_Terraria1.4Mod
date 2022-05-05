using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles;
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

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    public class DimNode:NodeMinion
    {
        public const int SINGLE_PENDULUM_DIST = 750;
        public const double SINGLE_PENDULUM_PERIOD = 21/4;

        public const float CHUA_ORBIT_PERIOD = 1f * 60 / 5;

        private TrailingStarController trailingStarController;
        public enum phase {
            SIGNLE_PENDULUM = 0,
            SIGNLE_PENDULUM_TWO = 1,
            ORBIT = 2,
            CHUA_CIRCUIT = 3,
            CHUA_CIRCUIT_FINALE = 4,
            TEMP = 5,
        }
        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Dim Node");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "暗节点");

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

            NPC.lifeMax = 1100;
            NPC.defense = 100;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 0, 15, 0);

            Timer = 0f;
            currentPhase = 0;

            NPCID.Sets.TrailingMode[NPC.type] = 0;
            NPCID.Sets.TrailCacheLength[NPC.type] = 8;

            trailingStarController = null;
        }

        public override void AI()
        {
            if (owner != null && target != null && target.active) {
                switch (currentPhase) {
                    case (int)phase.SIGNLE_PENDULUM: 
                        {
                            singlePendulum(true);
                            break;
                        }
                    case (int)phase.SIGNLE_PENDULUM_TWO:
                        {
                            singlePendulum(false);
                            break;
                        }
                    case (int)phase.ORBIT:
                        {
                            orbit(owner.GeneralTimer/ORBIT_PERIOD * MathHelper.TwoPi);
                            break;
                        }
                    case (int)phase.CHUA_CIRCUIT: 
                        {
                            chuaCircuit();
                            break;
                        }
                    case (int)phase.CHUA_CIRCUIT_FINALE: 
                        {
                            if (drawTrail != trail.TAIL)
                                drawTrail = trail.TAIL;
                            chuaCircuitFinale();
                            break;
                        }
                    case (int)phase.TEMP:
                        {
                            if (drawTrail != trail.SHADOW)
                                drawTrail = trail.SHADOW;
                            orbit(owner.GeneralTimer / ORBIT_PERIOD * MathHelper.TwoPi);
                            break;
                        }
                    default: break;
                }
            }
            base.AI();
        }



        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)NPC.frameCounter * NPC.height;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
            if (drawConnection)
                drawConnectionLine(spriteBatch, owner.NPC.Center, Color.Blue*1.5f, 10f);

            if (drawTrail == trail.SHADOW)
                drawShadow(spriteBatch, Color.Blue * 3.5f);
            else if (drawTrail == trail.TAIL)
                drawTail(spriteBatch, Color.LightBlue);
            spriteBatch.Draw(tex, NPC.position - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height), Color.White);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        protected override void summonEvent() {
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
            for (int i = 0; i < 60; i++) {
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Clentaminator_Cyan);
                d.velocity = Main.rand.NextVector2Unit()* 15;
                d.noGravity = true;
            }
        }

        private void singlePendulum(bool shootStars)
        {
            if (!drawConnection)
                drawConnection = true;
            float angle = (float)(MathHelper.PiOver4 * 
                Math.Cos(Timer*MathHelper.TwoPi/(SINGLE_PENDULUM_PERIOD*60)) + MathHelper.PiOver2);
            NPC.Center = owner.NPC.Center + angle.ToRotationVector2() * SINGLE_PENDULUM_DIST;
            Timer ++;

            // shoot rising stars
            if (shootStars)
            {
                if ((int)(Timer % (SINGLE_PENDULUM_PERIOD / 8 * 60)) == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY * 7.5f,
                        ModContent.ProjectileType<TrailingStar>(), 50, 0);
                }
            }
            else {
                int factor = (int)(Timer % (SINGLE_PENDULUM_PERIOD / 2 * 60));
                if (factor < 18 && factor % 6 == 0) {
                    owner.brightNode.summonBareLightning(12);
                }
            }
        }

        private void chuaCircuit()
        {
            if (trailingStarController == null) {
                Projectile p = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<TrailingStarController>(),0,0);
                trailingStarController = (TrailingStarController)p.ModProjectile;
                trailingStarController.summonStarBundle();
                Timer = 0;
            }

            hover(target.Center + 
                600f * ((float)( MathHelper.Pi/3.5 * Math.Sin(Timer/(180f/MathHelper.TwoPi)) 
                + MathHelper.Pi * (1f -  1 / 7f))).ToRotationVector2(),
                30, 0.3f, 1200);

            trailingStarController.Projectile.Center = NPC.Center;

            int factor = (int)Timer % (int)ChaosTheory.CHAOTIC_DURATION;

            if (factor == 0 || factor == 10 || factor == 20)
                trailingStarController.summonStarBundle();
            else if (factor == (int)(ChaosTheory.CHAOTIC_DURATION/2) 
                || factor == (int)(ChaosTheory.CHAOTIC_DURATION / 2) +10
                || factor == (int)(ChaosTheory.CHAOTIC_DURATION / 2) +20)
                trailingStarController.releaseStarBundle(target);

            trailingStarController.Projectile.timeLeft++;

            Timer++;
        }

        private void chuaCircuitFinale()
        {
            if (trailingStarController != null)
            {
                trailingStarController.Projectile.Center = NPC.Center;
                hover(target.Center, 400f + Timer * 5, 0f, CHUA_ORBIT_PERIOD, 50f, 1000f, 0.5f);
            }

            if (Timer >= CHUA_ORBIT_PERIOD * 5)
            {
                if (Timer >= CHUA_ORBIT_PERIOD * 6 && trailingStarController != null)
                {
                    trailingStarController.Projectile.Kill();
                    trailingStarController = null;
                    setPhase((int)phase.TEMP);
                }
                else {
                    if ((int)Timer % (int)(CHUA_ORBIT_PERIOD / 5) == 0)
                        trailingStarController.releaseStarBundle(target);
                }
                
            }
            else
            {
                if ((int)Timer % (int)CHUA_ORBIT_PERIOD == 0)
                {
                    trailingStarController.summonStarBundle();
                }
            }

            Timer++;
        }

        public override void OnKill()
        {
            if (trailingStarController != null) {
                trailingStarController.Projectile.Kill();
                trailingStarController = null;
            }
            base.OnKill();
        }
    }
}
