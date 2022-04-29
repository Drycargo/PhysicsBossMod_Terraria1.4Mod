using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    public class DimNode:NodeMinion
    {
        public static readonly int SINGLE_PENDULUM_DIST = 750;
        public static readonly double SINGLE_PENDULUM_PERIOD = 21/4;
        public enum phase {
            SIGNLE_PENDULUM = 0,
            SIGNLE_PENDULUM_TWO = 1,
            ORBIT = 2,
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
        }

        public override void AI()
        {
            if (owner != null && target != null) {
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
                            orbit(owner.Timer/ORBIT_PERIOD * MathHelper.TwoPi);
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
            Texture2D bt = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Beam").Value;
            
            if (drawConnection)
                drawConnectionLine(spriteBatch, owner.NPC.Center, Color.Blue*1.5f, 10f);

            drawShadow(spriteBatch, Color.Blue*3.5f);
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
                    Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), NPC.Center, Vector2.UnitY * 7.5f,
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
    }
}
