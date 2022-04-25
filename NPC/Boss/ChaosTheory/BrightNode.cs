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
        public enum phase
        {
            SIGNLE_PENDULUM_TWO = 0,
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
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNNormal").Value;
            luminanceTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/LuminanceGradient").Value;
            colorTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/RedOrangeGradient").Value;
        }

        public override void AI()
        {
            if (owner != null && target != null)
            {
                switch (currentPhase)
                {
                    case (int)phase.SIGNLE_PENDULUM_TWO:
                        {
                            singlePendulum();
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
            if (drawConnection && owner.dimNode != null)
                drawConnectionLine(spriteBatch, owner.dimNode.NPC.Center, Color.Red * 2.5f, 10f);

            spriteBatch.Draw(tex, NPC.position - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height), Color.White);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            #region drawtail
            /*
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            var proj = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));

            PhysicsBoss.trailingEffect.Parameters["uTransform"].SetValue(model * proj);
            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time);

            Main.graphics.GraphicsDevice.Textures[0] = backTex;
            Main.graphics.GraphicsDevice.Textures[1] = luminanceTex;
            Main.graphics.GraphicsDevice.Textures[2] = colorTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;

            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["Trail"].Apply();

            tail.PrepareStrip(NPC.oldPos, NPC.oldRot, progress => Color.White * 0.6f,
                progress => (progress < 0.1 ? MathHelper.Lerp(0.5f, 22.5f, progress * 10) : MathHelper.Lerp(25f, 0f, progress)),
                tex.Size() / 2, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);*/
            #endregion

            drawShadow(spriteBatch, Color.Red);
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

        private void singlePendulum()
        {
            if (!drawConnection)
                drawConnection = true;
            float angle = -(float)(Timer * MathHelper.TwoPi / (SINGLE_PENDULUM_PERIOD * 60));
            NPC.Center = owner.dimNode.NPC.Center + angle.ToRotationVector2() * SINGLE_PENDULUM_DIST;
            NPC.rotation = angle;

            for (int i = 0; i < 10; i++)
                Dust.NewDust(NPC.Center, 5,5,DustID.Flare);
            /*
            if ((int)(Timer % 90) == 0) {
                
                Projectile p = Projectile.NewProjectileDirect(NPC.GetSpawnSource_ForProjectile(), 
                    owner.dimNode.NPC.Center, Vector2.Zero, ModContent.ProjectileType<NewtonBeamLong>(), 50, 0);
                p.rotation = (NPC.Center - owner.dimNode.NPC.Center).ToRotation();
                NewtonBeamLong np = (NewtonBeamLong)p.ModProjectile;
                np.initialize(target);

            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFurySwing);

                for (int i = 0; i < 30; i++)
                {
                    Dust d = Dust.NewDustDirect(p.Center, 0, 0, DustID.FlameBurst);
                    d.velocity = Main.rand.NextVector2Unit() * (5 + 10 * Main.rand.NextFloat());
                    d.noGravity = true;
                }
            }*/

            Timer++;
        }
    }
}
