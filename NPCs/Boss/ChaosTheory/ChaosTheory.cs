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
using PhysicsBoss.Projectiles.DoublePendulum;
using PhysicsBoss.Projectiles.TrailingStarMotion;
using PhysicsBoss.Projectiles.ThreeBodyMotion;
using PhysicsBoss.NPCs;
using Terraria.GameContent.Drawing;

namespace PhysicsBoss.NPCs.Boss.ChaosTheory
{
    [AutoloadBossHead]
    public class ChaosTheory : TargetEnemy
    {
        public const float MAX_DISTANCE = 2000f;
        public static readonly int PHASE_COUNT = Enum.GetNames(typeof(phase)).Length;
        public const int HOVER_DIST = 330;
        public const float ELE_CHARGE_DURATION = 2 * 1.185f* 60;
        public const float CHAOTIC_DURATION = 7.35f/3* 60;
        public const float DOUBLE_PENDULUM_TOTAL_LENGTH = 550f;
        public const float DOUBLE_PENDULUM_PERIOD = 9.6f/4f * 60;
        public const float DOUBLE_PENDULUM_PERIOD2 = 9.725f/4f * 60;
        public const float THREE_BODY_PERIOD1 = 9.65f/4f * 60;
        public const float THREE_BODY_PERIOD2 = 9.78f * 60;
        public const float SPIRAL_SINK_RADIUS = 800f;
        public const float LORENZ_PERIOD = 9.9f / 8f * 60;
        public const float G = 7f;
        public const int BLIZZARD_PERIOD = (int)(2.5f * 60);
        public const int LOGISTIC_INITIALIZE_PEIOD = 10;
        public const int LOGISTIC_SPAWN_PERIOD = 25;
        public const int LOGISTIC_SPAWN_START = 300 - LOGISTIC_SPAWN_PERIOD * 8;
        public const float FINALE_RADIUS = 800;

        public const float FINALE_ONE_ANGULAR_ACC = MathHelper.TwoPi /720;
        public const float FINALE_TWO_ANGULAR_ACC = - MathHelper.TwoPi /600;

        
        public enum phase:int
        {
            INIT = 0,
            PendulumOne1 = 1,
            PendulumOnePhaseTwo2 = 2,
            ElectricCharge3 = 3,
            ConwayGame4 = 4,
            ChuaCircuit5 = 5,
            ChuaCircuitFinale6 = 6,
            Halvorsen7 = 7,
            HalvorsenFinale8 = 8,
            DoublePendulumOne9 = 9,
            DoublePendulumTwo10 = 10,
            ThreeBodyPreparation11 = 11,
            ThreeBodyMotionOne12 = 12,
            ThreeBodyMotionTwo13 = 13,
            ThreeBodyMotionFinale14 = 14,
            SpiralSink15 = 15,
            LorenzOne16 = 16,
            LorenzTwo17 = 17,
            TornadoPreparation18 = 18,
            Tornado19 = 19,
            Lightning20 = 20,
            RainFall21 = 21,
            Blizzard22 = 22,
            Logistic23 = 23,
            LorenzFinalePreparation24 = 24,
            LorenzFinaleOne25 = 25,
            LorenzFinaleTwo26 = 26,
            EulogyPreparation27 = 27,
            Eulogy28 = 28
        }
        
        
        
        public static readonly float[] phaseTiming = new float[] {
            0, // Init
            2.25f, // PendulumOne1
            12.25f,// PendulumOnePhaseTwo2, 11.75f
            23.25f, // ElectricCharge3
            31.5f, // ConwayGame4, 33
            42.3f, // ChuaCircuit5
            49.65f, // ChuaCircuitFinale6
            52f, // Halvorsen7
            56.8f, // HalvorsenFinale8
            60 + 1.15f, // DoublePendulumOne9, 1.65 -> 10
            60 + 11.125f, // DoublePendulumTwo10
            60 + 20.85f, // ThreeBodyPreparation11
            60 + 23.2f, // ThreeBodyMotionOne12
            60 + 32.85f, // ThreeBodyMotionTwo13
            60 + 41.53f, // ThreeBodyMotionFinale14
            60 + 43.25f, // SpiralSink15, 44.63
            60 * 2 + 3f, // LorenzOne16
            60 * 2 + 12.9f, // LorenzTwo17
            60 * 2 + 22.3f, // TornadoPreparation18
            60 * 2 + 23.85f, // Tornado19
            60 * 2 + 25f, // Lightning20
            60 * 2 + 28.25f,// RainFall21
            60 * 2 + 32f, // Blizzard22
            60 * 2 + 36.85f, // Logistic23
            60 * 2 + 41.85f, // LorenzFinalePreparation24
            60 * 2 + 44.3f, // LorenzFinaleOne25
            60 * 2 + 50.5f, // LorenzFinaleTwo26
            60 * 2 + 58.75f, // EulogyPreparation27
            60 * 3 + 1.2f, // Eulogy
         };
        

        public static float sub = 60 * 2 + 41.85f;
        /*
        public static readonly float[] phaseTiming = new float[] {
            0, // Init
            0,//2.25f, // PendulumOne1
            0,//12.25f,// PendulumOnePhaseTwo2, 11.75f
            0,//23.25f, // ElectricCharge3
            0,//31.5f, // ConwayGame4, 33
            0,//42.3f, // ChuaCircuit5
            0,//49.65f, // ChuaCircuitFinale6
            0,//52f, // Halvorsen7
            0,//56.8f, // HalvorsenFinale8
            0,//60 + 1.15f, // DoublePendulumOne9, 1.65 -> 10
            0,//60 + 11.125f, // DoublePendulumTwo10
            0,//60 + 20.85f, // ThreeBodyPreparation11
            0,//60 + 23.2f, // ThreeBodyMotionOne12
            0,//60 + 32.85f, // ThreeBodyMotionTwo13
            0,//60 + 41.53f, // ThreeBodyMotionFinale14
            0,//60 + 43.25f, // SpiralSink15, 44.63
            0,//60 * 2 + 3f, // LorenzOne16
            0,//60 * 2 + 12.9f, // LorenzTwo17
            0,//60 * 2 + 22.3f, // TornadoPreparation18
            0,//60 * 2 + 23.85f, // Tornado19
            0,//60 * 2 + 25f, // Lightning20
            0,//60 * 2 + 28.25f,// RainFall21
            0,//60 * 2 + 32f, // Blizzard22
            0, //60 * 2 + 36.85f, // Logistic23
            60 * 2 + 41.85f - sub, // LorenzFinalePreparation24
            60 * 2 + 44.3f - sub, // LorenzFinaleOne25
            60 * 2 + 50.5f - sub, // LorenzFinaleTwo26
            60 * 2 + 58.75f - sub,
            60 * 3 + 1.2f - sub
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

        // for Conway Game
        private Projectile conwayGameController = null;

        // for double pendulum
        private float len0,len1;
        private float angle0, angle1;
        private float angVel0, angVel1;
        private float m0, m1;
        private Projectile fractalRing = null;

        // for Three Body motion
        private Vector2 initialScreenPos = Vector2.Zero;
        private WaterDropController waterDropController = null;
        private ThreeBodyController threeBodyController = null;
        private float circleRadius;
        private Color circleColor;
        private Vector2 dashTarget;
        private Vector2 dashStart;
        private float aimLineTransparency;

        // for spiral sink
        private float target_Distance = -1;
        private float target_Angle = 0;

        // for Lorentz
        private TrailingStarController lorenzController = null;

        // for ButterFlyEffect
        private Tornado tornado = null;
        private Vector2 targetOriginalPos = Vector2.Zero;
        private float lightningProgress;
        private float rainProgress;
        private TrailingStarController[] sprottControllers;

        // for Logistic Map
        private LogisticMap[] maps;

        // for LorenzFinale
        private LorenzFinaleController lorenzFinaleController = null;
        private Queue<LogisticMap> mapPair;

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

            NPC.lifeMax = 11000;
            NPC.defense = 100;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.value = Item.buyPrice(0, 0, 15, 0);
            NPC.dontCountMe = true;

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

            NPC.lavaImmune = true;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            AIType = -3;

            AnimationType = -3;

            NPC.boss = true;

            currentPhase = phase.INIT;
            //currentPhase = phase.LorenzFinalePreparation24;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Sound/Music/CanonRockCut");
            }

            summoned = false;

            lastLife = NPC.lifeMax;

            len0 = DOUBLE_PENDULUM_TOTAL_LENGTH * 0.5f;
            len1 = DOUBLE_PENDULUM_TOTAL_LENGTH * 0.5f;
            angle0 = angle1 = MathHelper.Pi;
            angVel0 = angVel1 = 0;
            m0 = m1 = 1.0f;

            circleRadius = 0;
            circleColor = Color.White;
            dashTarget = Vector2.Zero;
            aimLineTransparency = 0;

            lightningProgress = 0;
            rainProgress = 0;
            sprottControllers = new TrailingStarController[3];

            maps = new LogisticMap[8];

            lorenzFinaleController = null;
            mapPair = new Queue<LogisticMap>();
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
            if (target == null || !target.active || target.statLife <= 0)
            {
                target = seekTarget(NPC.Center, MAX_DISTANCE);
                if (target != null)
                {
                    if (dimNode != null)
                        dimNode.setTarget(target);
                    if (brightNode != null)
                        brightNode.setTarget(target);
                }
                else {
                    if (dimNode != null)
                        dimNode.NPC.active = false;
                    if (brightNode != null)
                        dimNode.NPC.active = false;
                    NPC.active = false;
                    deActivate();
                }
            }

            if (target != null && target.active && target.statLife > 0)
            {
                switch (currentPhase)
                {
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
                    case phase.ElectricCharge3:
                        {
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
                    case phase.Halvorsen7:
                        {
                            halvorsen7();
                            break;
                        }
                    case phase.HalvorsenFinale8:
                        {
                            halvorsenFinale8();
                            break;
                        }
                    case phase.DoublePendulumOne9:
                        {
                            doublePendulumOne9();
                            break;
                        }
                    case phase.DoublePendulumTwo10:
                        {
                            doublePendulumTwo10();
                            break;
                        }
                    case phase.ThreeBodyPreparation11:
                        {
                            threebodyPreparation11();
                            break;
                        }
                    case phase.ThreeBodyMotionOne12:
                        {
                            CameraPlayer.deActivate();
                            threeBodyMotionOne12();
                            break;
                        }
                    case phase.ThreeBodyMotionTwo13:
                        {
                            threeBodyMotionTwo13();
                            break;
                        }
                    case phase.ThreeBodyMotionFinale14:
                        {
                            threeBodyMotionFinale14();
                            break;
                        }
                    case phase.SpiralSink15:
                        {
                            spiralSink15();
                            break;
                        }
                    case phase.LorenzOne16:
                        {
                            lorenzOne16();
                            break;
                        }
                    case phase.LorenzTwo17:
                        {
                            lorenzTwo17();
                            break;
                        }
                    case phase.TornadoPreparation18: {
                            tornadoPreparation18();
                            break;
                        }
                    case phase.Tornado19: {
                            tornado19();
                            break;
                        }
                    case phase.Lightning20: {
                            lightningStorm20();
                            break;
                        }
                    case phase.RainFall21: {
                            rainFall21();
                            break;
                        }
                    case phase.Blizzard22: {
                            blizzard22();
                            break;
                        }
                    case phase.Logistic23: {
                            logistic23();
                            break;
                        }
                    case phase.LorenzFinalePreparation24: {
                            lorenzFinalePreparation24();
                            break;
                        }
                    case phase.LorenzFinaleOne25: {
                            lorenzFinaleOne25();
                            break;
                        }
                    case phase.LorenzFinaleTwo26: {
                            lorenzFinaleTwo26();
                            break;
                        }
                    case phase.EulogyPreparation27: {
                            eulogyPreparation27();
                            break;
                        }
                    case phase.Eulogy28: {
                            eulogy();
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
            if (aimLineTransparency > 0) {
                GlobalEffectController.drawRayLine(spriteBatch, NPC.Center, target.Center, Color.Orange * aimLineTransparency, 50f);
            }

            Color c = Color.White * ((255f - NPC.alpha)/255f);
            spriteBatch.Draw(tex, NPC.position - Main.screenPosition, new Rectangle(0,NPC.frame.Y, NPC.width, NPC.height) ,c);

            if (circleRadius > 0) {
                GlobalEffectController.drawCircle(spriteBatch, NPC.Center, circleRadius, circleColor);
            }

            Lighting.AddLight(NPC.Center, Color.White.ToVector3());
        }

        public override bool PreKill()
        {
            selfFirework();
            for (int i = 0; i < 24; i++)
            {
                NPC butterfly = NPC.NewNPCDirect(NPC.GetSource_Death(), NPC.Center,
                    (Main.rand.NextFloat() < 0.05f ? NPCID.GoldButterfly : NPCID.Butterfly));
                butterfly.velocity = 5f * Main.rand.NextVector2Unit();
            }
            return true;
        }

        public override void OnKill()
        {
            if (dimNode != null)
            {
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

            deActivate();

            base.OnKill();
        }
        private void deActivate()
        {
            if (conwayGameController != null)
            {
                conwayGameController.Kill();
                conwayGameController = null;
            }

            if (fractalRing != null)
            {
                fractalRing.Kill();
            }

            if (threeBodyController != null)
            {
                threeBodyController.Projectile.Kill();
            }

            if (waterDropController != null)
            {
                waterDropController.killAll();
            }

            if (lorenzController != null) {
                lorenzController.Projectile.Kill();
                lorenzController = null;
            }

            if (tornado != null) {
                tornado.setKill(Vector2.Zero);
                tornado = null;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Sandstorm.StopSandstorm();
                Main.StopRain();
                Main.windSpeedCurrent = 0f;
            }

            if (lorenzFinaleController != null) {
                lorenzFinaleController.Projectile.Kill();
                lorenzFinaleController = null;
            }


            if (SkyManager.Instance["PhysicsBoss:ColorSky"].IsActive())
            {
                SkyManager.Instance.Deactivate("PhysicsBoss:ColorSky");
            }

            if (SkyManager.Instance["PhysicsBoss:BlackSky"].IsActive())
            {
                SkyManager.Instance.Deactivate("PhysicsBoss:BlackSky");
            }

            if (SkyManager.Instance["PhysicsBoss:OpenTheGate"].IsActive())
            {
                SkyManager.Instance.Deactivate("PhysicsBoss:OpenTheGate");
            }


            CameraPlayer.deActivate();
        }

        private void eulogy()
        {
            // 3:01.2
            // 3.06.5
            // 3:12.33
            // 3:14
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
            GlobalEffectController.vignette(Math.Min(1, Timer/60), 0.35f, 0.05f);
            ParticleOrchestraSettings settings = new ParticleOrchestraSettings
            {
                PositionInWorld = NPC.position,
                MovementVector = Main.rand.NextVector2Unit() * 5f
            };

            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Keybrand, settings);

            if ((int)Timer == 0)
            {
                dimNode.Timer = 0;
                minionPendulumFirework();

                if (dimNode != null)
                {
                    dimNode.setPhase((int)DimNode.phase.SIGNLE_PENDULUM_TWO);
                    dimNode.setConnectionTarget(this);
                }
                if (brightNode != null)
                {
                    brightNode.setPhase((int)BrightNode.phase.SIGNLE_PENDULUM_TWO);
                    brightNode.setConnectionTarget(dimNode);
                }
            }
            else if ((int)Timer == (int)(5.3 * 60))
            {
                if (brightNode != null)
                {
                    brightNode.fireWork();
                    brightNode.OnKill();
                    brightNode.NPC.active = false;
                    brightNode.NPC.life = 0;
                    brightNode = null;
                }
                if (dimNode != null)
                {
                    float temp = dimNode.Timer;
                    dimNode.setPhase((int)DimNode.phase.SIGNLE_PENDULUM);
                    dimNode.setConnectionTarget(this);
                    dimNode.Timer = temp;
                }
            }
            else if ((int)Timer == (int)(11.13 * 60))
            {
                if (dimNode != null)
                {
                    dimNode.fireWork();
                    dimNode.OnKill();
                    dimNode.NPC.active = false;
                    dimNode.NPC.life = 0;
                    dimNode = null;
                }
            }
            else if ((int)Timer == 13 * 60)
            {
                NPC.dontTakeDamage = false;
                NPC.life = 1;
            } else if((int)Timer == 13 * 60 + 1) {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(target.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<OneShot>(), int.MaxValue, 0, target.whoAmI);
                }
            } else if (Timer > 14 * 60) {
                NPC.life = -1;
                NPC.active = false;
                OnKill();
            }

            if (brightNode != null)
            {
                if ((int)Timer % (int)(DimNode.SINGLE_PENDULUM_PERIOD / 2 * 60) == 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        brightNode.summonBareLightning(12,
                            (-target.Center + brightNode.NPC.Center).SafeNormalize(
                                Vector2.UnitX).RotatedBy(i * MathHelper.Pi / 3) * 100);
                    }
                }
            }
            Timer++;
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (projectile.type == ProjectileID.FallingStar) {
                projectile.Kill();
            }
        }

        #region private methods
        private void init()
        {
            if (Timer == 0)
            {
                if (!SkyManager.Instance["PhysicsBoss:ColorSky"].IsActive())
                {
                    SkyManager.Instance.Activate("PhysicsBoss:ColorSky", Vector2.Zero, NPC.whoAmI);
                }

                ColorSky.setColor(Color.Black, 1);
                ColorSky.activateDrawBlock(Color.White * 0.45f);
            }

            float factor = Math.Min(1, (Timer / 60f));
            GlobalEffectController.vignette(1 * factor, 0.35f * factor, 0.05f);

            if (NPC.alpha >0 && GeneralTimer < 600) {
                int newA = (int)MathHelper.Max(NPC.alpha - (255f / 600f), 0);
                NPC.alpha = newA;
            }
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
            Timer++;
        }

        private void pendulumeOne1()
        {
            GlobalEffectController.vignette(1, 0.35f, 0.05f);
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
            if (dimNode == null) {
                createDimNode();
                dimNode.setPhase((int)DimNode.phase.SIGNLE_PENDULUM);
                dimNode.setConnectionTarget(this);
            }
        }
        private void pendulumeOne2()
        {
            GlobalEffectController.vignette(1, 0.35f, 0.05f);
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
            dimNode.setPhase((int)DimNode.phase.SIGNLE_PENDULUM_TWO);
            if (brightNode == null)
            {
                createBrightNode();
                brightNode.setPhase((int)BrightNode.phase.SIGNLE_PENDULUM_TWO);
                brightNode.setConnectionTarget(dimNode);
            } else {
                if ((int)(Timer - 10) % (int)(DimNode.SINGLE_PENDULUM_PERIOD / 2 * 60) == 0) {
                    for (int i = -1; i <= 1; i++) {
                        brightNode.summonBareLightning(12, 
                            (-target.Center + brightNode.NPC.Center).SafeNormalize(
                                Vector2.UnitX).RotatedBy(i * MathHelper.Pi/3) * 150);
                    }
                }
            }

            Timer++;
        }

        private void electricCharge3()
        {
            float factor = Timer / 30f;
            GlobalEffectController.vignette(1 *(1 -  factor), 0.35f * (1 + factor), 0.05f);

            if ((int)Timer == 0)
            {
                NPC.dontTakeDamage = false;

                dimNode.setPhase((int)DimNode.phase.ORBIT);
                dimNode.setDrawConnection(false);
                dimNode.NPC.dontTakeDamage = false;

                brightNode.setPhase((int)BrightNode.phase.ORBIT);
                brightNode.setDrawConnection(false);
                brightNode.NPC.dontTakeDamage = false;

                if (!SkyManager.Instance["PhysicsBoss:ColorSky"].IsActive())
                {
                    SkyManager.Instance.Activate("PhysicsBoss:ColorSky", Vector2.Zero, NPC.whoAmI);
                }

                ColorSky.setColor(Color.Lerp(Color.SkyBlue, Color.Black, 0.7f));
                ColorSky.activateDrawWire(Color.SkyBlue * 0.6f);
                ColorSky.deActivateDrawBlock();
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
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<ElectricChargeController>(), 0,0);
                SoundEngine.PlaySound(SoundID.Item4);
            }

            Timer++;
        }

        private void conwayGame4()
        {
            if (Timer == 0) {
                ColorSky.setColor(Color.Lerp(Color.Cyan, Color.Black, 0.7f));
                ColorSky.activateDrawWire(Color.LightGreen * 0.6f);
            }

            if (conwayGameController == null)
                conwayGameController =
                    Projectile.NewProjectileDirect(null, target.Center, Vector2.Zero,
                    ModContent.ProjectileType<ConwayGameController>(), 0, 0);
            
            if (conwayGameController != null)
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

                ColorSky.setColor(Color.Lerp(Color.SkyBlue, Color.Black, 0.65f));
                ColorSky.deActivateDrawWire();
                ColorSky.activateStars();
            }

            //hover(target.Center + (-MathHelper.Pi/6).ToRotationVector2() * 600f, 20f, 0.3f, 1200, 10, 500f, 0.9f * Math.Min(1, Timer/SinLaser.TRANSIT));
            hover(target.Center + 450f * Vector2.UnitY, 10f, 0.3f, 1200, 10, 500, 0.8f * Math.Min(1, Timer / SinLaser.TRANSIT));
            Timer++;
        }

        private void chuaCircuitFinale6()
        {
            if ((int)Timer == 0)
            {
                brightNode.setPhase((int)BrightNode.phase.CHUA_CIRCUIT_FINALE);
                dimNode.setPhase((int)DimNode.phase.CHUA_CIRCUIT_FINALE);
                dimNode.Timer = 0;
            }

            //hover(target.Center + (-MathHelper.Pi / 6).ToRotationVector2() * 600f, 20f, 0.3f, 1200, 10, 500f, 0.9f);
            hover(target.Center - Vector2.UnitY * 450f, 20f, 0.3f, 1200, 10, 500f, 0.9f);
            Timer++;
        }

        private void halvorsen7()
        {
            if ((int)Timer == 0)
            {
                brightNode.setPhase((int)BrightNode.phase.HALVORSEN);
                brightNode.Timer = 0;
                dimNode.setPhase((int)DimNode.phase.HALVORSEN);
                dimNode.Timer = 0;
                ColorSky.setColor(Color.Lerp(Color.HotPink, Color.Black, 0.45f));
                ColorSky.activateStars();
                ColorSky.setStarMotion(true, false);
            }

            //hover(target.Center + (MathHelper.Pi * 5 / 6).ToRotationVector2() * 600f, 20f, 0.3f, 1200, 10, 500f, 0.85f);
            hover(target.Center - Vector2.UnitY * 450f, 20f, 0.3f, 1200, 10, 500f, 0.85f);

            Timer++;
        }

        private void halvorsenFinale8()
        {
            if ((int)Timer == 0)
            {
                brightNode.setPhase((int)BrightNode.phase.HALVORSEN_FINALE);
                brightNode.Timer = 0;
                dimNode.setPhase((int)DimNode.phase.DOUBLE_PENDULUM_PREPARATION);
                dimNode.Timer = 0;
            }
            hover(target.Center - Vector2.UnitY * 0.25f * DOUBLE_PENDULUM_TOTAL_LENGTH, 10f, 0.3f, 1200, 10, 500f, 0.85f);
            Timer++;
        }

        private void doublePendulumOne9()
        {
            // approach
            if (Vector2.Distance(NPC.Center, target.Center) > 0.7 * DOUBLE_PENDULUM_TOTAL_LENGTH)
            {
                NPC.Center = 0.995f * NPC.Center + 0.005f * target.Center;
            }

            if ((int)Timer == 0)
            {
                NPC.velocity *= 0;
                dimNode.setPhase((int)DimNode.phase.DOUBLE_PENDULUM_ONE);
                dimNode.Timer = 0;
                brightNode.setPhase((int)BrightNode.phase.DOUBLE_PENDULUM_ONE);
                brightNode.Timer = 0;
                selfFirework();
                minionPendulumFirework();

                initilizePendulum();
            }

            // summon and maintain ring
            summonMaintainFractalRing();

            // restrict player
            if (fractalRing != null)
                attractPlayer(FractalRing.RADIUS, fractalRing.Center);

            doublePendulumMotion();

            // render
            dimNode.NPC.Center = NPC.Center + len0 * (MathHelper.PiOver2 - angle0).ToRotationVector2();
            brightNode.NPC.Center = dimNode.NPC.Center + len1 * (MathHelper.PiOver2 - angle1).ToRotationVector2();

            float fixedTimer = Timer - 0.5f * 60;


            int fac = (int)fixedTimer % (int)(2 * DOUBLE_PENDULUM_PERIOD);

            // brightNode Laser
            if (fac < DOUBLE_PENDULUM_PERIOD)
            {
                if (fac > 0.05 * DOUBLE_PENDULUM_PERIOD && fac < 0.5 * DOUBLE_PENDULUM_PERIOD)
                {
                    if ((int)Timer % 4 == 0)
                    {
                        try
                        {
                            brightNode.summonLaserPairTangent();
                            brightNode.summonLaserPairNormal();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            foreach (Projectile p in Main.projectile)
                            {
                                if (p.type == ModContent.ProjectileType<LaserSword>())
                                {
                                    p.Kill();
                                    p.active = false;
                                }

                            }
                        }

                        /*
                        if (fixedTimer % (4 * DOUBLE_PENDULUM_PERIOD) < DOUBLE_PENDULUM_PERIOD)
                        {
                            brightNode.summonLaserPairNormal(); 
                        }
                        else
                        {
                            brightNode.summonLaserPairTangent();
                        }*/
                    }
                }
            }


            // dimNode

            if (fac >= DOUBLE_PENDULUM_PERIOD)
            {
                if (((int)(fixedTimer) % (int)(DOUBLE_PENDULUM_PERIOD/2)) == 0)
                {
                    ThreeScrollController tsc = (ThreeScrollController)
                        (Projectile.NewProjectileDirect(dimNode.NPC.GetSource_FromAI(), dimNode.NPC.Center,
                        Vector2.Zero, ModContent.ProjectileType<ThreeScrollController>(), 30, 0).ModProjectile);
                    tsc.setOwner(dimNode);
                    tsc.summonStarBundle<TrailingStarThreeScroll>();
                }
            }

            for (int i = 0; i < 5; i++) {
                Dust.NewDust(NPC.Center + Main.rand.NextVector2Unit() * len0, 0,0,DustID.MagicMirror);
            }

            Timer++;
        }

        private void doublePendulumTwo10()
        {
            // approach
            if (Vector2.Distance(NPC.Center, target.Center) > 0.7 * DOUBLE_PENDULUM_TOTAL_LENGTH)
            {
                NPC.Center = 0.995f * NPC.Center + 0.005f * target.Center;
            }

            if ((int)Timer == 0)
            {
                NPC.velocity *= 0;
                dimNode.setPhase((int)DimNode.phase.DOUBLE_PENDULUM_TWO);
                dimNode.setConnectionTarget(brightNode);
                dimNode.Timer = 0;
                brightNode.setPhase((int)BrightNode.phase.DOUBLE_PENDULUM_TWO);
                brightNode.Timer = 0;
                brightNode.setConnectionTarget(this);

                selfFirework();
                minionPendulumFirework();
            }

            summonMaintainFractalRing();

            if (fractalRing != null)
                attractPlayer(FractalRing.RADIUS, fractalRing.Center);

            doublePendulumMotion();

            brightNode.NPC.Center = NPC.Center + len0 * (MathHelper.PiOver2 - angle0).ToRotationVector2();
            dimNode.NPC.Center = brightNode.NPC.Center + len1 * (MathHelper.PiOver2 - angle1).ToRotationVector2();

            
            float fac = Timer %(DOUBLE_PENDULUM_PERIOD2);

            // brightNode lasers
            if (fac <= 0.35 * DOUBLE_PENDULUM_PERIOD2)
            {
                float prog = fac / (0.35f * DOUBLE_PENDULUM_PERIOD2);
                float angle = ((int)Timer % (int)(2 * DOUBLE_PENDULUM_PERIOD2) > DOUBLE_PENDULUM_PERIOD2) ?
                    MathHelper.Lerp(MathHelper.PiOver2, MathHelper.Pi * (1f - 1f/6f), prog) :
                    MathHelper.Lerp(MathHelper.Pi / 6, MathHelper.PiOver2, prog);

                brightNode.setTriLaserAngle(angle);
            }
            else if ((int)fac == (int)(0.5 * DOUBLE_PENDULUM_PERIOD2)) {
                brightNode.summonTriLasers();
            }

            // dimNode
            if (fac < 0.5f * DOUBLE_PENDULUM_PERIOD2 && ((int)fac%(int)(DOUBLE_PENDULUM_PERIOD2/4)) == 0) {
                float d = 5f * (target.Center.X - dimNode.NPC.Center.X < 0 ? -1f :
                    (target.Center.X - dimNode.NPC.Center.X == 0 ? 0 : 1f));
                Projectile.NewProjectile(dimNode.NPC.GetSource_FromAI(), dimNode.NPC.Center, 
                    new Vector2(d,0),
                    ModContent.ProjectileType<AizawaController>(), 30, 0);
                SoundEngine.PlaySound(SoundID.Item25, dimNode.NPC.Center);
            }


            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(NPC.Center + Main.rand.NextVector2Unit() * len0, 0, 0, DustID.HeartCrystal);
            }

            Timer++;
        }

        private void attractPlayer(float restrictionDist, Vector2 center)
        {
            foreach (Player p in Main.player)
            {
                float dist = Vector2.Distance(p.Center, center);
                if (p.active && dist > restrictionDist)
                    p.velocity += (float)Math.Min(15, 0.35f * dist) * (center - p.Center).SafeNormalize(Vector2.Zero);
            }
        }

        private void attractPlayer(float restrictionDist)
        {
            attractPlayer(restrictionDist, NPC.Center);
        }

        private void summonMaintainFractalRing()
        {
            if (fractalRing == null)
                fractalRing = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<FractalRing>(), 100, 0);
            fractalRing.Center = NPC.Center + 0.2f * DOUBLE_PENDULUM_TOTAL_LENGTH * Vector2.UnitY;
            fractalRing.timeLeft++;
        }

        private void initilizePendulum()
        {
            m0 = 0.8f + 0.2f * Main.rand.NextFloat();
            m1 = 0.8f + 0.2f * Main.rand.NextFloat();

            len0 = (NPC.Center - dimNode.NPC.Center).Length();
            len1 = (brightNode.NPC.Center - dimNode.NPC.Center).Length();

            angle0 = MathHelper.PiOver2 - (dimNode.NPC.Center - NPC.Center).ToRotation();
            angle1 = MathHelper.PiOver2 - (brightNode.NPC.Center - dimNode.NPC.Center).ToRotation();
        }

        private void doublePendulumMotion()
        {
            float l0 = 1;
            float l1 = 1 * len1/len0;
            float wSquare0 = angVel0 * angVel0;
            float wSquare1 = angVel1 * angVel1;

            float denomCommonFactor =
                (float)(2 * m0 + m1 - m1 * Math.Cos(2 * (angle0 - angle1)));

            // for angle 0
            float numerator0 = (float)(
                -G * (2 * m0 + m1) * Math.Sin(angle0)
                - m1 * G * Math.Sin(angle0 - 2 * angle1)
                - 2 * Math.Sin(angle0 - angle1) * m1 
                * (wSquare1 * l1 + wSquare0 * l0 * Math.Cos(angle0 - angle1)));

            float angAcc0 = numerator0 / (l0 * denomCommonFactor);

            float numerator1 = (float)(
                2 * Math.Sin(angle0 - angle1) * 
                (wSquare0 * l0 *(m0 + m1)
                + G * (m0 + m1) * Math.Cos(angle0)
                + wSquare1 * l1 * m1 * Math.Cos(angle0 - angle1)));

            float angAcc1 = numerator1 / (l1 * denomCommonFactor);

            angVel0 += angAcc0/60;
            angVel1 += angAcc1/60;


            angle0 += ((angVel0/60) % MathHelper.TwoPi);
            angle0 %= MathHelper.TwoPi;

            angle1 += ((angVel1/60) % MathHelper.TwoPi);
            angle1 %= MathHelper.TwoPi;

        }

        private void threebodyPreparation11()
        {
            if ((int)Timer == 0) {
                if (fractalRing != null) {
                    fractalRing.Kill();
                    fractalRing = null;
                }

                if (threeBodyController == null) {
                    threeBodyController = (ThreeBodyController)(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                        NPC.Center, Vector2.Zero, ModContent.ProjectileType<ThreeBodyController>(), 0, 0).ModProjectile);
                }

                dimNode.setPhase((int)DimNode.phase.THREEBODY_MOTION);
                dimNode.NPC.dontTakeDamage = true;
                brightNode.setPhase((int)BrightNode.phase.THREEBODY_MOTION);
                brightNode.NPC.dontTakeDamage = true;

                NPC.dontTakeDamage = true;

                initialScreenPos = Main.screenPosition;
            }

            CameraPlayer.activate(NPC.whoAmI);

            if (Timer < 60 && initialScreenPos != Vector2.Zero)
                CameraPlayer.setDisplacement(Vector2.Lerp(initialScreenPos, NPC.Center - Main.ScreenSize.ToVector2() / 2, Timer/60f));
            else
                CameraPlayer.setDisplacement(NPC.Center - Main.ScreenSize.ToVector2() / 2);

            attractPlayer(WaterDropController.RADIUS, NPC.Center);

            NPC.velocity *= 0;

            if (threeBodyController != null)
            {
                threeBodyController.Projectile.Center = NPC.Center;
                threeBodyController.Projectile.timeLeft++;
                if ((int)Timer == 75)
                    threeBodyController.summonSuns();
            }


            if (Timer < 70)
            {
                float factor = (70f - Timer) / 70f;
                circleRadius = 1500 * factor;
                circleColor = Color.Lerp(Color.White, Color.Red, factor);
            }
            else {
                circleRadius = 0;
            }
            
            Timer++;
        }

        private void threeBodyMotionOne12()
        {
            if ((int)Timer == 0) {
                if (waterDropController == null)
                {
                    waterDropController = new WaterDropController(target.Center, -(GeneralTimer / 1800) * MathHelper.TwoPi);
                }
                dimNode.setPhase((int)DimNode.phase.ORBIT);
                dimNode.NPC.dontTakeDamage = false;
                brightNode.setPhase((int)BrightNode.phase.ORBIT);
                brightNode.NPC.dontTakeDamage = false;
                NPC.dontTakeDamage = false;
            }

            if (Timer < 30)
            {
                CameraPlayer.activate(NPC.whoAmI);
                CameraPlayer.setDisplacement(Vector2.Lerp(NPC.Center - Main.ScreenSize.ToVector2() / 2, Main.screenPosition, Timer / 30f));
            }
            else
                CameraPlayer.deActivate();


            if (waterDropController != null)
            {
                if (Timer >= 25 && (int)Timer % 3 == 0 && waterDropController.getCount() < WaterDropController.TOTAL)
                {
                    waterDropController.summonWaterDrop(NPC);
                }
                waterDropController.updateAll(-(GeneralTimer / 1800) * MathHelper.TwoPi);

                // attract player
                attractPlayer(WaterDropController.RADIUS, waterDropController.getCenter());
            }

            if (threeBodyController != null) {
                threeBodyController.Projectile.Center = NPC.Center;
                threeBodyController.Projectile.timeLeft++;
                /*
                if ((int)Timer % (int)THREE_BODY_PERIOD1 == 0) {
                    for (int i = 0; i < 3; i++) {
                        SolarRadiation sr = (SolarRadiation)(Projectile.NewProjectileDirect(
                            NPC.GetSource_FromAI(), NPC.Center, 
                            (threeBodyController[i].Projectile.Center - NPC.Center).SafeNormalize(Vector2.Zero) * (10f),
                            ModContent.ProjectileType<SolarRadiation>(), 30, 0).ModProjectile);
                        sr.setTarget(target);
                    }
                }*/
            }

            // dash
            float factor = Timer % THREE_BODY_PERIOD1;
            if (factor > 0.45 * THREE_BODY_PERIOD1 && factor < 0.8 * THREE_BODY_PERIOD1)
            {
                aimLineTransparency = (float)(0.7f * Math.Max(0, (THREE_BODY_PERIOD1 * 0.55f - factor) / (0.1 * THREE_BODY_PERIOD1)));
                NPC.velocity *= 0;
                dash(dashStart, dashTarget, (factor - THREE_BODY_PERIOD1 * 0.45f) / (0.35f * THREE_BODY_PERIOD1));
                if (Timer % 8 == 0)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<SolarExplosion>(), 50, 0);
            }
            else {
                if (factor < THREE_BODY_PERIOD1 * 0.45f)
                {
                    aimLineTransparency = (float)(0.7f * Math.Min(1, factor / (0.4 * THREE_BODY_PERIOD1)));
                }

                dashTarget = target.Center;
                dashStart = NPC.Center;
                NPC.velocity = -5f * Vector2.Normalize(target.Center - NPC.Center);
            }

            /*
            if (factor < THREE_BODY_PERIOD1 * 0.65f)
            {
                aimLineTransparency = (float)(0.7f * Math.Min(1, factor/(0.4 * THREE_BODY_PERIOD1)));
                dashTarget = target.Center;
                dashStart = NPC.Center;
                NPC.velocity = -5f * Vector2.Normalize(target.Center - NPC.Center);
            }
            else {
                aimLineTransparency = (float)(0.7f * Math.Max(0, (THREE_BODY_PERIOD1 * 0.75f - factor) / (0.1 * THREE_BODY_PERIOD1)));
                NPC.velocity *= 0;
                dash(dashStart, dashTarget, (factor - THREE_BODY_PERIOD1 * 0.65f) /(0.35f * THREE_BODY_PERIOD1));
                if ((int)factor == (int)(THREE_BODY_PERIOD1 * 0.65f))
                    SoundEngine.PlaySound(SoundID.Thunder, NPC.Center);
            }
            */

            Timer++;
        }

        private void dash(Vector2 startPos, Vector2 targetPos, float progress)
        {
            if (targetPos == NPC.Center)
                return;

            if (progress < 0.1f)
                SoundEngine.PlaySound(SoundID.Thunder, NPC.Center);

            Vector2 finalPos;
            if (Vector2.Distance(targetPos, startPos) < 800)
                finalPos = startPos + (targetPos - startPos).SafeNormalize(Vector2.UnitX) * 800f;
            else
                finalPos = targetPos + (targetPos - startPos).SafeNormalize(Vector2.UnitX) * 50f;

            NPC.Center = Vector2.Lerp(startPos, finalPos, (float)Math.Sin(MathHelper.Pi * progress - MathHelper.PiOver2) * 0.5f + 0.5f);
            GlobalEffectController.shake(18f * (1f - Math.Min(1f, Vector2.Distance(NPC.Center, target.Center)/1000)));
        }

        private void threeBodyMotionTwo13()
        {
            if (Timer == 0) {
                if (threeBodyController != null && waterDropController != null) {
                    threeBodyController[0].fireLaser(waterDropController.getCenter());
                    threeBodyController[1].fireLaser(waterDropController.getCenter());
                }
            }

            if (threeBodyController != null)
            {
                threeBodyController.Projectile.Center = NPC.Center;
                if (Timer > 60 && Timer < (7f * (THREE_BODY_PERIOD2 / 8)) && 
                    (int)Timer % (int)(THREE_BODY_PERIOD2 / 8) == 0)
                {
                    threeBodyController[2].fireSpike();
                }
                threeBodyController.Projectile.timeLeft++;
            }


            if (waterDropController != null)
            {
                waterDropController.updateAll(-(GeneralTimer / 1800) * MathHelper.TwoPi);

                // attract player
                attractPlayer(WaterDropController.RADIUS, waterDropController.getCenter());

                hover(waterDropController.getCenter(), WaterDropController.RADIUS * 0.75f, 0, 
                    THREE_BODY_PERIOD2 * MathHelper.Lerp(1f/2f,  1f/ 2.25f, Timer/THREE_BODY_PERIOD2),50, MAX_DISTANCE, 0.01f);
            }


            Timer++;
        }


        private void threeBodyMotionFinale14()
        {
            hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);

            if (Timer == 0) {
                if (threeBodyController != null) {
                    threeBodyController.Projectile.Kill();
                    threeBodyController = null;
                }
            }

            if (Timer < 90) {
                if (waterDropController != null)
                {
                    if (Timer < WaterDropController.CHARGE_TIMES * WaterDropController.CHARGE_PERIOD)
                        waterDropController.aimAll(target.Center);
                    waterDropController.updateAll((GeneralTimer / 1800) * MathHelper.TwoPi);
                }
            } else if (Timer == 90) {
                if (waterDropController != null)
                {
                    waterDropController.launchAll();
                    waterDropController = null;
                    SoundEngine.PlaySound(SoundID.Item33);
                }
            }

            Timer++;
        }

        private void spiralSink15()
        {
            if (Timer < 75)
            {
                if (Timer == 0) {
                    if (!SkyManager.Instance["PhysicsBoss:BlackSky"].IsActive())
                    {
                        SkyManager.Instance.Activate("PhysicsBoss:BlackSky");
                    }
                }

                hover(target.Center - HOVER_DIST * Vector2.UnitY, 25, 0.3f, 600);
                if (Timer < 30)
                {
                    GlobalEffectController.bloom(1.5f * Timer / 15f, 0);
                }
                else
                {
                    if ((int)Timer == 30)
                    {
                        NPC.dontTakeDamage = true;

                        dimNode.setPhase((int)DimNode.phase.SPIRAL_SINK);
                        dimNode.NPC.dontTakeDamage = true;
                        brightNode.setPhase((int)BrightNode.phase.SPIRAL_SINK);
                        brightNode.NPC.dontTakeDamage = true;
                    }
                    GlobalEffectController.bloom(1.5f * ((75f - Timer) / 45f), 0);
                    ParticleOrchestraSettings settings = new ParticleOrchestraSettings
                    {
                        PositionInWorld = Main.screenPosition + new Vector2(Main.screenWidth * Main.rand.NextFloat(),
                            Main.screenHeight * Main.rand.NextFloat()),
                        MovementVector = Main.rand.NextVector2Unit() * 16f
                    };

                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.RainbowRodHit, settings, NPC.whoAmI);
                }
            } else {
                if ((int)Timer % 19 == 0) {
                    for (int i = 0; i < 2; i++) {
                        ButterflySpiralSink b = (ButterflySpiralSink)(NPC.NewNPCDirect(NPC.GetSource_FromAI(),
                            (i % 2 == 0 ? dimNode.NPC : brightNode.NPC).Center, ModContent.NPCType<ButterflySpiralSink>()).ModNPC);
                        b.setOwner(this);
                    }
                }

                if (Timer > 60 && (int)(Timer - 60) % (int)(19f * 60 / 8) == 0) {
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 dir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        Projectile p = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                            NPC.Center + 5f * (i * dir.RotatedBy(MathHelper.PiOver2)), 9f * dir,
                            ProjectileID.RainbowRodBullet, 25, 0);

                        p.friendly = false;
                        p.hostile = true;
                        p.aiStyle = 48;

                        SoundEngine.PlaySound(SoundID.Item43, NPC.Center);
                    }
                }

                NPC.velocity *= 0;

                attractPlayer(SPIRAL_SINK_RADIUS * 0.9f, NPC.Center);

                for (int i = 0; i < 35; i++) {
                    Vector2 pos = NPC.Center + 0.9f * SPIRAL_SINK_RADIUS * Main.rand.NextVector2Unit();
                    Dust d = Dust.NewDustDirect(pos,0,0, i % 2 == 0 ? DustID.XenonMoss : DustID.ArgonMoss);
                    d.noGravity = true;
                    d.velocity = 10f * (NPC.Center - pos).SafeNormalize(Vector2.Zero);
                    d.noLightEmittence = false;
                    d.noLight = false;
                }
            }

            target_Distance = targetDist();
            target_Angle = targetAngle();

            NPC.velocity *= 0;
            Timer++;
        }

        public float getTargetDist() { return target_Distance; }
        public float getTargetAngle() { return target_Angle; }
        
        private void lorenzOne16()
        {
            if ((int)Timer == 0) {
                if (SkyManager.Instance["PhysicsBoss:BlackSky"].IsActive())
                {
                    SkyManager.Instance.Deactivate("PhysicsBoss:BlackSky");
                }


                NPC.dontTakeDamage = false;
                dimNode.setPhase((int)DimNode.phase.ORBIT);
                dimNode.NPC.dontTakeDamage = false;
                brightNode.setPhase((int)BrightNode.phase.ORBIT);
                brightNode.NPC.dontTakeDamage = false;

                lorenzController = (TrailingStarController)(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center, Vector2.Zero, ModContent.ProjectileType<TrailingStarController>(), 0,0).ModProjectile);

                for (int i = 0; i < 5; i++) {
                    lorenzController.summonStarBundle<TrailingStarLorenz>();
                }

                int requiredType = ModContent.NPCType<ButterflySpiralSink>();
                foreach (NPC npc in Main.npc) {
                    if (npc.type == requiredType) {
                        npc.life = -1;
                        npc.active = false;
                    }
                }
            }

            if ((int)Timer % (int)LORENZ_PERIOD == 0) {
                if ((int)Timer >= 4 * (int)LORENZ_PERIOD)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        lorenzController.summonStarBundle<TrailingStarLorenz>();
                        lorenzController.releaseStarBundle(target);
                    }
                }
            }

            follow();
            if (lorenzController != null)
            {
                lorenzController.Projectile.Center = NPC.Center;
                lorenzController.Projectile.timeLeft++;
            }

            Timer++;
        }

        private void lorenzTwo17()
        {
            if ((int)Timer == 0) {
                if (lorenzController != null)
                {
                    lorenzController.Projectile.Kill();
                    lorenzController = null;
                }

                if (tornado == null)
                {
                    tornado = (Tornado)Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<Tornado>(), 0, 0).ModProjectile;
                }
            }

            if (Timer > 4.5 * 60) {
                if (!SkyManager.Instance["PhysicsBoss:OpenTheGate"].IsActive())
                {
                    SkyManager.Instance.Activate("PhysicsBoss:OpenTheGate", Vector2.Zero, NPC.whoAmI);
                }
            }

            if ((int)Timer % (int)(LORENZ_PERIOD * 0.95f) == 0)
            {
                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 12f;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        ((TrailingStarLorenz)Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center,
                            10f * (NPC.velocity.ToRotation() + MathHelper.PiOver2 * (0.5f + i)).ToRotationVector2().
                            RotatedByRandom(5f/180f*MathHelper.Pi),
                            ModContent.ProjectileType<TrailingStarLorenz>(), 25, 0).ModProjectile).releaseProj(target);
                        SoundEngine.PlaySound(SoundID.Item25, NPC.Center);
                    }
                }
            } else {
                NPC.velocity *= 0.98f;
            }

            if (tornado != null)
            {
                tornado.Projectile.Center = NPC.Center;
                tornado.Projectile.timeLeft++;
            }

            Timer++;
        }

        private void tornadoPreparation18()
        {
            if ((int)Timer == 0 && SkyManager.Instance["PhysicsBoss:OpenTheGate"].IsActive())
            {
                SkyManager.Instance.Deactivate("PhysicsBoss:OpenTheGate");
            }

            if (tornado != null) {
                tornado.Projectile.Center = NPC.Center;
                tornado.Projectile.timeLeft++;

                if (Timer < 85) {
                    float factor = Timer / 90f;
                    GlobalEffectController.bloom(factor * 1.75f, 0.1f);
                    GlobalEffectController.vignette(0.5f * factor, 0.5f * factor);
                }else if ((int)Timer == 85) {
                    GlobalEffectController.bloom(-1, 0.1f);
                    GlobalEffectController.vignette(-1, 0);
                    tornado.spawn();
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Sandstorm.StartSandstorm();
                        Main.StartRain();
                        Main.windSpeedCurrent = 0.9f;
                    }
                }
            }

            follow(450f, 1f);


            /*
            if (Timer % 6 == 0)
            {
                Dust.QuickDustLine(new Vector2(NPC.Center.X - 0.75f * TornadoPlate.WIDTH, NPC.Center.Y - 1000),
                    new Vector2(NPC.Center.X - 0.75f * TornadoPlate.WIDTH, NPC.Center.Y + 1000), 100, Color.LightBlue);
                Dust.QuickDustLine(new Vector2(NPC.Center.X + 0.75f * TornadoPlate.WIDTH, NPC.Center.Y - 1000),
                    new Vector2(NPC.Center.X + 0.75f * TornadoPlate.WIDTH, NPC.Center.Y + 1000), 100, Color.LightBlue);
            }*/

            Timer++;
        }

        private void tornado19()
        {
            horizontalFollow(6.5f);
            if (tornado != null)
            {
                tornado.Projectile.Center = NPC.Center;
                tornado.Projectile.timeLeft++;
            }
            Timer++;
        }

        private void horizontalFollow(float v)
        {
            NPC.Center = new Vector2(NPC.Center.X, 0.25f * target.Center.Y + 0.75f * NPC.Center.Y);
            follow(dist:750f,speed: v);
        }

        private void lightningStorm20()
        {
            // 3.25s
            horizontalFollow(7f);
            if (tornado != null)
            {
                tornado.Projectile.Center = NPC.Center;
                tornado.Projectile.timeLeft++;
            }

            /*
            int factor = (int)Timer % 100;

            if (factor == 0 || factor == 15 || factor == 30) {
                if (factor == 0) {
                    targetOriginalPos = target.Center;
                }
                //float dispX = 0.25f * LargeLightningBolt.LENGTH * (targetOriginalPos.X < NPC.Center.X ? -1 : 1);
                float dispY = LargeLightningBolt.WIDTH * ((2 - factor / 15) + 2.25f);

                for (int i = 0; i < 2; i++) {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), targetOriginalPos + new Vector2(0, 
                        (i % 2 == 0 ? 1f : -1f) * dispY), (((int)(Timer / 100) % 2 == 0 ? 1 : -1) 
                        * MathHelper.Pi/6).ToRotationVector2(),
                        ModContent.ProjectileType<LargeLightningBolt>(), 60, 0);
                }
            }
            */

            if (lightningProgress < 1f)
                lightningProgress += 1 / (3.25f * 60);

            if ((int)Timer % 10 == 0)
            {
                float dispY = LargeLightningBolt.WIDTH * (4.25f - lightningProgress * 2.5f);
                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + new Vector2(0,
                        (i % 2 == 0 ? 1f : -1f) * dispY), (MathHelper.Lerp(1, -1, Timer / (3.25f * 60)) * MathHelper.Pi / 6).ToRotationVector2(),
                        ModContent.ProjectileType<LargeLightningBolt>(), 60, 0);
                }
            }

            summonLightning();

            Timer++;
        }

        private static void summonLightning()
        {
            if (Main.rand.NextFloat() < 1f / 60f)
            {
                Main.NewLightning();
            }
        }

        private void rainFall21()
        {
            horizontalFollow(6.5f);
            summonLightning();

            if (tornado != null)
            {
                if ((int)Timer == 15)
                {
                    tornado.setKill(target.Center);
                }
                else if (Timer < 15 + 30)
                {
                    tornado.Projectile.Center = NPC.Center;
                    tornado.Projectile.timeLeft++;
                }
            }

            float devX = 200 + 900 * (1 - rainProgress * Main.rand.NextFloat());
            int relPos = target.Center.X < NPC.Center.X ? -1 : 1;
            for (int i = 0; i < 12; i++) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(
                        NPC.Center.X + 1000 * relPos + (i % 2 == 0 ? -1 : 1) * devX,
                        NPC.Center.Y - 1200), Vector2.UnitY.RotatedBy(relPos * MathHelper.Pi/12) * 60f, 
                        ModContent.ProjectileType<RainDrop>(), 15, 0);
                }
            }

            if (rainProgress < 1f)
                rainProgress += 1 / 90f;           

            Timer++;
        }

        private void blizzard22()
        {
            hover(target.Center - Vector2.UnitY * 500f, 20f, 0.1f, 600);

            if (dimNode != null && brightNode != null)
            {
                if ((int)Timer == 0)
                {
                    dimNode.setPhase((int)DimNode.phase.H_SHIFT);
                    brightNode.setPhase((int)BrightNode.phase.H_SHIFT);
                    minionPendulumFirework();
                }


                if ((int)Timer % 5 == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Projectile p = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                            (i % 2 == 0 ? dimNode.NPC : brightNode.NPC).Center, Vector2.UnitY * 10,
                            ProjectileID.CoolWhipProj, 20, 0, NPC.whoAmI);

                        p.friendly = false;
                        p.hostile = true;
                        p.aiStyle = 1;
                    }
                }
            }

            if ((int)Timer == 0) {
                for (int i = 0; i < 3; i++)
                    sprottControllers[i] = (TrailingStarController)(Projectile.NewProjectileDirect(
                        NPC.GetSource_FromAI(), target.Center, Vector2.Zero, 
                        ModContent.ProjectileType<TrailingStarController>(), 0, 0).ModProjectile);

                fireWork(Color.LightBlue);
                minionPendulumFirework();
            }


            int factor = (int)Timer % BLIZZARD_PERIOD;
            for (int i = 0; i < 3; i++)
            {
                if (sprottControllers[i] != null)
                {
                    sprottControllers[i].Projectile.Center = target.Center +
                        ((Timer/600 + (float)i/3f) * MathHelper.TwoPi).ToRotationVector2() * 200f;

                    if (factor == 0)
                    {
                        sprottControllers[i].summonStarBundle<TrailingStarSprott>();
                    }
                    else if (factor == 45)
                    {
                        sprottControllers[i].releaseStarBundle();
                    }
                }
            }

            Timer++;
        }

        private void logistic23()
        {
            if (Timer == 0) {
                if (dimNode != null && brightNode != null)
                {
                    if ((int)Timer == 0)
                    {
                        dimNode.setPhase((int)DimNode.phase.ORBIT);
                        brightNode.setPhase((int)BrightNode.phase.ORBIT);
                        minionPendulumFirework();
                    }
                }


                for (int i = 0; i < 3; i++)
                {
                    if (sprottControllers[i] != null)
                    {
                        sprottControllers[i].Projectile.Kill();
                        sprottControllers[i] = null;
                    }
                }

                for (int i = 0; i < 8; i++)
                    maps[i] = (LogisticMap)(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                        NPC.Center, Vector2.Zero, ModContent.ProjectileType<LogisticMap>(), 20, 0).ModProjectile);
                fireWork(Color.Crimson);
            }

            if (Timer < LOGISTIC_INITIALIZE_PEIOD * 8)
            {
                hover(target.Center + new Vector2(-LogisticMap.TOT_WIDTH * 0.5f, -50), 20f, 0.1f, 600, inertia: 0.7f);
                if ((int)Timer % LOGISTIC_INITIALIZE_PEIOD == 0)
                {
                    int ind = (int)(Timer / LOGISTIC_INITIALIZE_PEIOD);
                    if (ind < 8)
                    {
                        if (ind == 0)
                            maps[ind].initialize();
                        else
                            maps[ind].initialize(maps[ind - 1]);
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    maps[i].Projectile.Center = NPC.Center;
                }
            } else if (Timer >= LOGISTIC_INITIALIZE_PEIOD * 8) {
                hover(target.Center + new Vector2(-LogisticMap.TOT_WIDTH * 0.5f, -50), 20f, 0.1f, 600, inertia: 0.95f);

                if ((Timer - LOGISTIC_INITIALIZE_PEIOD * 8) % LOGISTIC_SPAWN_PERIOD == 0)
                {
                    int ind = (int)((Timer - LOGISTIC_INITIALIZE_PEIOD * 8) / LOGISTIC_SPAWN_PERIOD);
                    if (ind < 8) {
                        maps[ind].materialize();
                        maps[ind].setFall();
                        //maps[ind].swing();
                    }

                    for (int i = ind + 1; i < 8; i++)
                        maps[i].Projectile.Center = NPC.Center;
                }
            }
            #region oldImp
            /*
            else if (Timer < LOGISTIC_SPAWN_START)
            {
                hover(target.Center - 300 * Vector2.UnitY, 20f, 0.1f, 600);
            }
            else if (Timer < 270)
            {
                hover(target.Center - 300 * Vector2.UnitY, 20f, 0.1f, 600, inertia: 0.98f);

                int ind = (int)((Timer - LOGISTIC_SPAWN_START) / LOGISTIC_SPAWN_PERIOD);

                for (int i = ind + 1; i < 8; i++)
                {
                    maps[i].Projectile.rotation += (float)(MathHelper.Pi) / (8f * LOGISTIC_SPAWN_PERIOD);
                }
            } else if (Timer == 270) {
                for (int i = 0; i < 8; i++)
                {
                    maps[i].materialize();
                    maps[i].setFall();
                }
            }
            */
            #endregion

            Timer++;
        }

        private void lorenzFinalePreparation24()
        {

            if (dimNode != null) {
                dimNode.setPhase((int)DimNode.phase.LORENZ_FINALE);
            }

            if (brightNode != null)
            {
                brightNode.setPhase((int)BrightNode.phase.LORENZ_FINALE);
            }
            follow(speed:1f);
            if (Timer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Sandstorm.StopSandstorm();
                    Main.StopRain();
                    Main.windSpeedCurrent = 0f;
                }

                if (!NPC.dontTakeDamage)
                    NPC.dontTakeDamage = true;

                lorenzFinaleController = (LorenzFinaleController)Projectile.NewProjectileDirect(
                    NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LorenzFinaleController>(),
                    0, 0).ModProjectile;

                
                for (int i = 0; i < 5; i++)
                {
                    lorenzFinaleController.summonStarBundle<TrailingStarLorenzFinale>();
                }
            }

            if (Timer == 30) {

                for (int i = 0; i < 2; i++)
                {
                    BlockMap p = (BlockMap)Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center,
                        Vector2.Zero, ModContent.ProjectileType<BlockMap>(), 30, 0).ModProjectile;
                    p.Projectile.rotation = ((float)(Main.time % MathHelper.TwoPi + i * MathHelper.Pi));
                    p.initialize();

                    mapPair.Enqueue(p);
                }
            }

            
            if (lorenzFinaleController != null) {
                lorenzFinaleController.Projectile.Center = NPC.Center;
                lorenzFinaleController.Projectile.timeLeft++;
                //if ((int)Timer == (int)TrailingStarLorenzFinale.TRANSIT + 30)
                    
            }
            

            maintainMaps(FINALE_ONE_ANGULAR_ACC);

            attractPlayer(FINALE_RADIUS, NPC.Center);

            Timer++;
        }

        private void maintainMaps(float angularSpeed)
        {
            if (mapPair != null)
            {
                foreach (LogisticMap map in mapPair)
                {
                    map.Projectile.rotation += angularSpeed;
                    map.Projectile.Center = NPC.Center;
                    map.Projectile.timeLeft++;
                }
            }

            Timer++;
        }

        private void lorenzFinaleOne25()
        {
            if ((int)Timer == 0) {
                TrailingStarLorenzFinale.generalRotation = 0;
                NPC.velocity *= 0;
                lorenzFinaleController.activateAll();
                foreach (LogisticMap map in mapPair)
                {
                    map.materialize();
                }
            }

            int factor = (int)Timer % 180;

            if (factor == 0)
            {
                foreach (LogisticMap map in mapPair)
                {
                    map.initialize(map);
                    //map.materialize();
                }

            }
            /*
            else if (factor == 60) {
                foreach (LogisticMap map in mapPair)
                {
                    map.initialize(map);
                    map.Projectile.rotation += 15f * FINALE_ONE_ANGULAR_ACC;
                }
            }
            */

            #region oldImp
            /*
            } else if (factor == 60) {
                while (mapPair.Count > 0) {
                    LogisticMap map = mapPair.Dequeue();
                    map.Projectile.timeLeft = 30;
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        BlockMap p = (BlockMap)Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center,
                            Vector2.Zero, ModContent.ProjectileType<BlockMap>(), 30, 0).ModProjectile;
                        p.Projectile.rotation = map.Projectile.rotation + 15f * FINALE_ONE_ANGULAR_ACC;
                        p.initialize(map);
                        mapPair.Enqueue(p);
                    }
                }
            }
            */
            #endregion

            maintainMaps(FINALE_ONE_ANGULAR_ACC);
            if (lorenzFinaleController != null)
            {
                lorenzFinaleController.Projectile.Center = NPC.Center;
                lorenzFinaleController.Projectile.timeLeft++;
            }

            TrailingStarLorenzFinale.generalRotation += TrailingStarLorenzFinale.ROTATION_STEP;

            attractPlayer(FINALE_RADIUS, NPC.Center);
            for (int i = 0; i < 35; i++)
            {
                Vector2 pos = NPC.Center + FINALE_RADIUS * Main.rand.NextVector2Unit();
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RainbowRod);
                d.noGravity = true;
                d.velocity = 10f * (NPC.Center - pos).SafeNormalize(Vector2.Zero);
                d.noLightEmittence = false;
                d.noLight = false;
            }
            Timer++;
        }

        private void lorenzFinaleTwo26()
        {
            if ((int)Timer == 0) {
                NPC.velocity *= 0;

                for (int i = 0; i < 2; i++) {
                    LogisticMap map = mapPair.Dequeue();
                    FractalMap newMap = (FractalMap)Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center,
                        Vector2.Zero, ModContent.ProjectileType<FractalMap>(), 30, 0).ModProjectile;
                    newMap.Projectile.rotation = map.Projectile.rotation;
                    newMap.initialize(map);
                    map.Projectile.Kill();

                    mapPair.Enqueue(newMap);
                }
            }

            if (Timer < 180) {
                circleRadius = 1500 * MathHelper.Lerp(1,0, Timer/180);
                circleColor = Color.Lerp(Color.Blue, Color.Violet,Timer / 180);
            } else {
                if (Timer == 180)
                {
                    foreach (LogisticMap map in mapPair)
                    {
                        map.materialize();
                    }
                    circleRadius = 0;
                }

                int factor = (int)(Timer - 180) % 180;

                if (factor == 0)
                {
                    foreach (LogisticMap map in mapPair)
                    {
                        map.initialize(map);
                        //map.materialize();
                    }
                }
                /*
                else if (factor == 60)
                {
                    foreach (LogisticMap map in mapPair)
                    {
                        map.initialize(map);
                        map.Projectile.rotation += 15f * FINALE_TWO_ANGULAR_ACC;
                    }
                }
                */
            }

            #region oldImp
            /*
            else if (factor == 60)
            {
                while (mapPair.Count > 0)
                {
                    LogisticMap map = mapPair.Dequeue();
                    map.Projectile.timeLeft = 30;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        FractalMap p = (FractalMap)Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center,
                            Vector2.Zero, ModContent.ProjectileType<FractalMap>(), 30, 0).ModProjectile;
                        p.Projectile.rotation = map.Projectile.rotation + 15f * FINALE_ONE_ANGULAR_ACC;
                        p.initialize(map);
                        mapPair.Enqueue(p);
                    }
                }
            }
            */
            #endregion

            maintainMaps(MathHelper.Lerp(FINALE_ONE_ANGULAR_ACC,FINALE_TWO_ANGULAR_ACC, Math.Min(Timer/180, 1)));
            if (lorenzFinaleController != null)
            {
                lorenzFinaleController.Projectile.Center = NPC.Center;
            }

            TrailingStarLorenzFinale.generalRotation += TrailingStarLorenzFinale.ROTATION_STEP;

            attractPlayer(FINALE_RADIUS, NPC.Center);
            for (int i = 0; i < 35; i++)
            {
                Vector2 pos = NPC.Center + FINALE_RADIUS * Main.rand.NextVector2Unit();
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RainbowRod);
                d.noGravity = true;
                d.velocity = 10f * (NPC.Center - pos).SafeNormalize(Vector2.Zero);
                d.noLightEmittence = false;
                d.noLight = false;
            }
            Timer++;
        }

        private void eulogyPreparation27()
        {
            // 2:58.75
            // 2:59.8
            // 3:01.2
            if ((int)Timer == 0)
            {
                if (lorenzFinaleController != null)
                {
                    lorenzFinaleController.Projectile.Kill();
                    lorenzFinaleController = null;
                }
            } else if ((int)Timer == (int)(1.05 * 60)) {
                if (mapPair != null) {
                    while (mapPair.Any())
                    {
                        mapPair.Dequeue().Projectile.timeLeft = LogisticMap.FADE_TRANSIT;
                    }
                    mapPair = null;
                }
                TwistedDust.deActivateAll();
                SoundEngine.PlaySound(SoundID.MaxMana);
            }

            Timer++;
        }


        #endregion

        #region Helpers

        private void minionPendulumFirework()
        {
            brightNode.fireWork();
            dimNode.fireWork();
        }

        private void selfFirework()
        {
            for (int i = 0; i < 100; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.FireworksRGB);
                d.velocity = Main.rand.NextVector2Unit() * 15f;
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
        }


        private void createDimNode()
        {
            int id = NPC.NewNPC(NPC.GetSource_FromAI(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DimNode>());
            dimNode = (DimNode)Main.npc[id].ModNPC;
            dimNode.NPC.dontTakeDamage = true;
            dimNode.setOwner(this);
            dimNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }

        private void createBrightNode()
        {
            int id = NPC.NewNPC(NPC.GetSource_FromAI(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BrightNode>());
            brightNode = (BrightNode)Main.npc[id].ModNPC;
            brightNode.NPC.dontTakeDamage = true;
            brightNode.setOwner(this);
            brightNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }

        public phase getPhase() {
            return currentPhase;
        }

        #endregion
    }
}
