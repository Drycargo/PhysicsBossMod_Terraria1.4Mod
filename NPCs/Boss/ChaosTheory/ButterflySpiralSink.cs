using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using PhysicsBoss.Projectiles;
using PhysicsBoss.Effects;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;

namespace PhysicsBoss.NPCs.Boss.ChaosTheory
{
    public class ButterflySpiralSink: ModNPC
    {
        //public readonly Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple};
        public const int TRAILING_CONST = 15;

        private ChaosTheory origin = null;

        private Color color;

        private Texture2D tex;
        private Texture2D trailTex;
        private VertexStrip tail = new VertexStrip();

        public float Timer
        {
            get { return NPC.ai[0]; }
            set { NPC.ai[0] = value; }
        }


        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Butterfly Spiral Sink");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "螺旋汇聚蝶");

            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = 30;
            tex = ModContent.Request<Texture2D>(Texture).Value;
            trailTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvance").Value;
            NPC.friendly = false;

            NPC.width = tex.Width;
            NPC.height = tex.Height / Main.npcFrameCount[NPC.type];
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCHit5;

            NPC.lifeMax = 150;
            NPC.defense = 20;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.value = 0;

            Timer = 0f;

            color = Main.hslToRgb(new Vector3(Main.rand.NextFloat(), 0.8f, 0.75f));
            NPC.frameCounter = 0;
            NPC.oldPos = new Vector2[TRAILING_CONST];
            NPC.oldRot = new float[TRAILING_CONST];

            for (int i = 0; i < TRAILING_CONST; i++) {
                NPC.oldRot[i] = 0f;
                NPC.oldPos[i] = NPC.position;
            }
        }

        public override void AI()
        {
            if (origin != null)
            {
                if (Vector2.Distance(origin.NPC.Center, NPC.Center) < 30) {
                    OnKill();
                    NPC.life = -1;
                    NPC.active = false;
                }

                float amplitude = origin.getTargetDist() / ChaosTheory.SPIRAL_SINK_RADIUS + 0.5f;
                float sign = (origin.getTargetAngle() > MathHelper.Pi ? -1 : 1);

                float x = NPC.Center.X - origin.NPC.Center.X;
                float y = NPC.Center.Y - origin.NPC.Center.Y;

                NPC.velocity = new Vector2(amplitude * sign * y,
                    -amplitude * sign * x - y)/18;
                if (NPC.velocity.Length() > 18f)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 18f;
            }
            else {
                if (Timer > 120) {
                    OnKill();
                    NPC.life = -1;
                    NPC.active = false;
                }
                if (NPC.velocity == Vector2.Zero) {
                    NPC.velocity = Main.rand.NextVector2Unit() * 5f;
                }
            }

            if (Main.rand.NextFloat() < 0.2f)
            {
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RainbowMk2);
                d.color = color;
                d.noGravity = true;
            }

            /*
                for (float num22 = 0f; num22 < 0.5f; num22 += 0.25f) {
					settings = new ParticleOrchestraSettings {
						PositionInWorld = base.Center,
						MovementVector = Vector2.UnitX.RotatedBy(num22 * ((float)Math.PI * 2f)) * 16f
					};

					ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.RainbowRodHit, settings, owner);
				}
             */


            if ((int)Timer % 5 == 0) {
                NPC.frameCounter++;
                NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            }

            if ((int)Timer % 2 == 0) {
                for (int i = TRAILING_CONST - 1; i > 0; i--)
                {
                    NPC.oldPos[i] = NPC.oldPos[i - 1];
                    NPC.oldRot[i] = NPC.oldRot[i - 1];
                }
            }

            NPC.rotation = NPC.velocity.ToRotation();
            NPC.oldPos[0] = NPC.position;
            NPC.oldRot[0] = NPC.rotation;

            Timer++;
            base.AI();
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)NPC.frameCounter * NPC.height;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = trailTex;

            tail.PrepareStrip(NPC.oldPos, NPC.oldRot, progress => color, progress => (1 - progress) * NPC.height * 0.4f, -Main.screenPosition);
            tail.DrawTrail();
            /*
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            */
            spriteBatch.Draw(tex, NPC.position - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height),
                color * 2f, 0, NPC.Size / 2, 1,
                (NPC.velocity.X > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public void setOwner(ChaosTheory chaosTheory) {
            origin = chaosTheory;
        }

        public override void OnKill()
        {
            for (int i = 0; i < 5; i++)
            {
                ParticleOrchestraSettings settings = new ParticleOrchestraSettings
                {
                    PositionInWorld = NPC.Center,
                    MovementVector = Main.rand.NextVector2Unit() * 16f
                };

                ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.RainbowRodHit, settings, NPC.whoAmI);
            }
        }
    }
}
