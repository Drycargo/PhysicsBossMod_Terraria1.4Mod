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
        private Texture2D tex;
        public static readonly int SINGLE_PENDULUM_DIST = 600;
        public static readonly double SINGLE_PENDULUM_PERIOD = 21/4;
        public enum phase {
            SIGNLE_PENDULUM = 0,
            SIGNLE_PENDULUM_TWO = 1,
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
            //spriteBatch.Begin()
            
            if (drawConnection)
                drawConnectionLine(spriteBatch, owner.NPC.Center, Color.Blue*2.5f, 10f);

            int l = NPC.oldPos.Length;
            for (int i = l - 1; i >= 0; i--) {
                if (NPC.oldPos[i] != Vector2.Zero) {
                    Color c = Color.White * (1f - (float)i / l)*0.5f;
                    c.B +=50;
                    spriteBatch.Draw(tex, NPC.oldPos[i] - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height), c);
                }
            }
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
            if (shootStars && (int)(Timer % (SINGLE_PENDULUM_PERIOD / 13 * 60)) == 0) {
                Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), NPC.Center, Vector2.UnitY*5,
                    ModContent.ProjectileType<TrailingStar>(), 50, 0);
            }
        }
    }
}
