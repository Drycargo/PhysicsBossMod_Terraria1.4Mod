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
using PhysicsBoss.Projectiles.ConwayGame;

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    [AutoloadBossHead]
    public class ChaosTheory : TargetEnemy
    {
        public const float MAX_DISTANCE = 2000f;
        public static readonly int PHASE_COUNT = Enum.GetNames(typeof(phase)).Length;
        public const int HOVER_DIST = 330;
        public const float ELE_CHARGE_DURATION = 2 * 1.185f* 60;
        public const float CHAOTIC_DURATION = 7.35f/3* 60;


        public enum phase
        {
            INIT = 0,
            PendulumOne1 = 1,
            PendulumOnePhaseTwo2 = 2,
            ElectricCharge3 = 3,
            ConwayGame4 = 4,
            ChuaCircuit5 = 5,
            ChuaCircuitFinale6 = 6,
        }

        
        public static readonly float[] phaseTiming = new float[] {
            0,
            2.25f,
            12.25f,//11.75f,
            23.25f,
            31.5f,//33
            42.3f,
            49.65f,
            52f,
        };
        
        
        /*
        public static readonly float[] phaseTiming = new float[] {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0.1f,
        };
        */
        

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

        private Projectile conwayGameController = null;

        private int lastLife;
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

            lastLife = NPC.lifeMax;
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
                if (target != null)
                {
                    if (dimNode != null)
                        dimNode.setTarget(target);
                    if (brightNode != null)
                        brightNode.setTarget(target);
                }
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
                            NPC.alpha = 0;
                            pendulumeOne1();
                            break;
                        }
                    case phase.PendulumOnePhaseTwo2:
                        {
                            pendulumeOne2();
                            break;    
                        }
                    case phase.ElectricCharge3: {
                            electricCharge3();
                            break;
                        }
                    case phase.ConwayGame4:
                        {
                            conwayGame4();
                            break;
                        }
                    case phase.ChuaCircuit5:
                        {
                            chuaCircuit5();
                            break;
                        }
                    case phase.ChuaCircuitFinale6: 
                        {
                            chuaCircuitFinale6();
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

            // deal with damage
            if (lastLife != NPC.life)
            {
                lastLife = NPC.life;
                if (dimNode!= null)
                {
                    dimNode.NPC.life = NPC.life;
                }

                if (brightNode != null)
                {
                    brightNode.NPC.life = NPC.life;
                }
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
            Lighting.AddLight(NPC.Center, Color.White.ToVector3());
        }

        public override void OnKill()
        {
            if (dimNode != null) {
                dimNode.OnKill();
                dimNode.NPC.active = false;
                dimNode.NPC.life = -1;
            }

            if (brightNode != null)
            {
                brightNode.OnKill();
                brightNode.NPC.active = false;
                brightNode.NPC.life = -1;
            }

            if (conwayGameController != null) {
                conwayGameController.Kill();
                conwayGameController = null;
            }

            base.OnKill();
        }


        #region private methods
        private void init()
        {
            if (NPC.alpha >0 && GeneralTimer < 600) {
                int newA = (int)MathHelper.Max(NPC.alpha - (255f / 600f), 0);
                NPC.alpha = newA;
            }
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
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

        private void electricCharge3()
        {
            if ((int)Timer == 0)
            {
                dimNode.setPhase((int)DimNode.phase.ORBIT);
                dimNode.setDrawConnection(false);

                brightNode.setPhase((int)BrightNode.phase.ORBIT);
                brightNode.setDrawConnection(false);
            }

            // chase or hover
            Vector2 dist = NPC.Center - target.Center;
            if (dist.Length() < 300f)
            {
                hover(dist.SafeNormalize(Vector2.UnitX) * 250f + target.Center, 30f, 0.3f, 1200);
            }
            else
            {
                NPC.velocity.X = -1f * dist.X - 1f * dist.Y;
                NPC.velocity.Y = 1f * dist.X - 1f * dist.Y;

                float speed = NPC.velocity.Length();

                if (dist.Length() < 800f)
                    NPC.velocity *= (10f / speed);
                else if (speed > 50f)
                {
                    NPC.velocity *= (50f / speed);
                }
            }

            // call charges
            if ((int)(Timer % (ELE_CHARGE_DURATION)) == 0)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<ElectricChargeController>(), 0,0);
                SoundEngine.PlaySound(SoundID.Item4);
            }

            Timer++;
        }

        private void conwayGame4()
        {
            if (conwayGameController == null)
                conwayGameController =
                    Projectile.NewProjectileDirect(null, target.Center, Vector2.Zero,
                    ModContent.ProjectileType<ConwayGameController>(), 0, 0);
            
            conwayGameController.Center = target.Center;

            // chase or hover
            Vector2 dist = NPC.Center - target.Center;
            if (dist.Length() < 300f)
            {
                hover(dist.SafeNormalize(Vector2.UnitX) * 250f + target.Center, 30f, 0.3f, 1200);
            }
            else
            {
                NPC.velocity.X = -1f * dist.X - 1f * dist.Y;
                NPC.velocity.Y = 1f * dist.X - 1f * dist.Y;

                float speed = NPC.velocity.Length();

                if (speed > 10f)
                {
                    if (dist.Length() < 800f)
                        NPC.velocity *= (10f / speed);
                    else if (speed > 50f)
                    {
                        NPC.velocity *= (50f / speed);
                    }

                }

            }
            Timer++;
        }
        private void chuaCircuit5()
        {
            if ((int)Timer == 0) {
                conwayGameController = null;
                brightNode.setPhase((int)BrightNode.phase.CHUA_CIRCUIT);
                dimNode.setPhase((int)DimNode.phase.CHUA_CIRCUIT);
            }

            hover(target.Center + (-MathHelper.Pi/6).ToRotationVector2() * 500f, 20f, 0.3f, 1200, 10, 500f, 0.97f);
            Timer++;
        }

        private void chuaCircuitFinale6()
        {
            if ((int)Timer == 0)
            {
                //brightNode.setPhase((int)BrightNode.phase.CHUA_CIRCUIT);
                dimNode.setPhase((int)DimNode.phase.CHUA_CIRCUIT_FINALE);
                dimNode.Timer = 0;
            }
            hover(target.Center + (-MathHelper.Pi / 6).ToRotationVector2() * 500f, 20f, 0.3f, 1200, 10, 500f, 0.97f);
            Timer++;
        }

        #endregion

        #region Helpers
        private void createDimNode() {
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromThis(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DimNode>());
            dimNode = (DimNode)Main.npc[id].ModNPC;
            dimNode.setOwner(this);
            dimNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }


        private void createBrightNode()
        {
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromThis(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BrightNode>());
            brightNode = (BrightNode)Main.npc[id].ModNPC;
            brightNode.setOwner(this);
            brightNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }

        #endregion
    }
}
