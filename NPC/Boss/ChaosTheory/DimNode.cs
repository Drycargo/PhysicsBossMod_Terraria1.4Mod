using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    public class DimNode:NodeMinion
    {
        private Texture2D tex;
        public static readonly int SINGLE_PENDULUM_DIST = 600;
        public enum phase {
            SIGNLE_PENDULUM = 0,
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
                    case (int)phase.SIGNLE_PENDULUM: {
                            singlePendulum();
                            break;
                        }

                    default: break;
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)NPC.frameCounter * NPC.height;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D bt = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Beam").Value;
            //spriteBatch.Begin()
            spriteBatch.Draw(bt, (NPC.Center + owner.NPC.Center)/2 - Main.screenPosition, null, Color.Blue*1.5f,
                (NPC.Center - owner.NPC.Center).ToRotation() + MathHelper.PiOver2, bt.Size()/2f,
                new Vector2(10/(float)bt.Width, (float)SINGLE_PENDULUM_DIST/(float)bt.Height), SpriteEffects.None,0);

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



        private void singlePendulum()
        {
            if (!drawConnection)
                drawConnection = true;
            float angle = (float)(MathHelper.PiOver4 * Math.Cos(Timer*MathHelper.TwoPi/(5*60)) + MathHelper.PiOver2);
            NPC.Center = owner.NPC.Center + angle.ToRotationVector2() * SINGLE_PENDULUM_DIST;
            Timer ++;
        }
    }
}
