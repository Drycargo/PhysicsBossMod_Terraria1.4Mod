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

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    [AutoloadBossHead]
    public class ChaosTheory : TargetEnemy
    {
        public static readonly float MAX_DISTANCE = 2000f;
        public static readonly int PHASE_COUNT = Enum.GetNames(typeof(phase)).Length;
        public static readonly int HOVER_DIST = 280;
        public enum phase
        {
            INIT = 0,
            PendulumOne1 = 1,
            PendulumOnePhaseTwo2 = 2,
        }

        public static readonly float[] phaseTiming = new float[] {
            0,
            2,
            11.75f,
        };

        private Texture2D tex;
        private phase currentPhase;

        public float GeneralTimer {
            get { return NPC.ai[2]; }
            set { NPC.ai[2] = value; }
        }

        public float Timer {
            get { return NPC.ai[3]; }
            set { NPC.ai[3] = value; }
        }

        private bool summoned;

        public DimNode dimNode;
        public BrightNode brightNode;
        public override string BossHeadTexture => base.BossHeadTexture;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Chaos Theory");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "混沌理论");

            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            tex = ModContent.Request<Texture2D>(Texture).Value;
            target = null;
            dimNode = null;

            NPC.width = tex.Width;
            NPC.height = tex.Height / Main.npcFrameCount[NPC.type];

            NPC.damage = 50;
            NPC.friendly = false;

            NPC.lifeMax = 1100;
            NPC.defense = 100;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.value = Item.buyPrice(0, 0, 15, 0);

            NPC.buffImmune[BuffID.Burning] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.Frostburn2] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.OnFire3] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            Banner = NPC.type;
            BannerItem = ItemID.Gel; //stub


            NPC.noGravity = true;
            NPC.noTileCollide = true;
            AIType = -3;

            AnimationType = -3;

            NPC.boss = true;
            currentPhase = phase.INIT;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Sound/Music/CanonRockCut");
            }

            summoned = false;
        }

        public override void AI()
        {
            if (!summoned) {
                Main.NewText("Hello, Lorentz.", Color.LightGreen);
                NPC.alpha = 255;
                summoned=true;
            }
            // frame animation
            if ((GeneralTimer) % 10 == 0)
            {
                NPC.frameCounter++;
                NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            }

            // seek target
            if (target == null)
            {
                target = seekTarget(NPC.Center, MAX_DISTANCE);
            }

            if (target != null) {
                switch (currentPhase) {
                    case phase.INIT: 
                        {
                            init();
                            Timer++;
                            break;
                        }
                    case phase.PendulumOne1: 
                        {
                            pendulumeOne1();
                            break;
                        }
                    case phase.PendulumOnePhaseTwo2:
                        {
                            pendulumeOne2();
                            break;    
                        }
                    default: break;
                }
            }

            // update timer
            GeneralTimer++;

            // update phase
            if ((int)currentPhase + 1 < PHASE_COUNT && GeneralTimer >= phaseTiming[(int)currentPhase + 1] * 60) {
                currentPhase++;
                Timer = 0;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)NPC.frameCounter * NPC.height;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Color c = Color.White * ((255f - NPC.alpha)/255f);
            spriteBatch.Draw(tex, NPC.position - Main.screenPosition, new Rectangle(0,NPC.frame.Y, NPC.width, NPC.height) ,c);
            //base.PostDraw(spriteBatch, screenPos, Color.White);
        }

        public override void OnKill()
        {
            if (dimNode != null) {
                dimNode.NPC.active = false;
                dimNode.NPC.life = -1;
            }

            if (brightNode != null)
            {
                brightNode.NPC.active = false;
                brightNode.NPC.life = -1;
            }

            base.OnKill();
        }

        #region private methods
        private void init()
        {
            if (NPC.alpha >0 && Timer < 600) {
                int newA = (int)MathHelper.Max(NPC.alpha - (255f / 600f), 0);
                NPC.alpha = newA;
            }
            hover(target.Center - 250*Vector2.UnitY, 25, 0.3f, 600);
        }

        private void pendulumeOne1()
        {
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
            if (dimNode == null) {
                createDimNode();
                dimNode.setPhase((int)DimNode.phase.SIGNLE_PENDULUM);
            }
        }
        private void pendulumeOne2()
        {
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
            dimNode.setPhase((int)DimNode.phase.SIGNLE_PENDULUM_TWO);
            if (brightNode == null)
            {
                createBrightNode();
                brightNode.setPhase((int)BrightNode.phase.SIGNLE_PENDULUM_TWO);
            }
        }

        private void createDimNode() {
            int id = Terraria.NPC.NewNPC(NPC.GetSpawnSourceForProjectileNPC(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DimNode>());
            dimNode = (DimNode)Main.npc[id].ModNPC;
            dimNode.setOwner(this);
            dimNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }


        private void createBrightNode()
        {
            int id = Terraria.NPC.NewNPC(NPC.GetSpawnSourceForProjectileNPC(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BrightNode>());
            brightNode = (BrightNode)Main.npc[id].ModNPC;
            brightNode.setOwner(this);
            brightNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }

        private void hover(Vector2 hoverCenter, float hoverRadius, float noise, float period)
        {
            float degree = (NPC.Center - hoverCenter).ToRotation() + MathHelper.TwoPi / period;

            NPC.Center = degree.ToRotationVector2() * hoverRadius + hoverCenter
                + noise * 2 * (Main.rand.NextFloat() - 0.5f) * Vector2.One;
        }
        #endregion
    }
}
