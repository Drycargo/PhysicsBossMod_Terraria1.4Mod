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
        public const float DOUBLE_PENDULUM_TOTAL_LENGTH = 550f;
        public const float DOUBLE_PENDULUM_PERIOD = 10/4f * 60;
        public const float DOUBLE_PENDULUM_PERIOD2 = 9.725f/4f * 60;
        public const float G = 7f;

        public enum phase
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
            //ThreeBodyMotionOne12 = 12,
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
            56.8f,
            60 + 1.15f, // 1.65 -> 10
            60 + 11.125f,
            60 + 20.85f,
            //60 + 23.2f,
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
            0,//0.1f,
            0,//4.9f,
            1f,//9.25f
            11f,
            20f,
            22.5f
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
        private WaterDropController waterDropController = null;

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

            NPC.dontTakeDamage = true;
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

            len0 = DOUBLE_PENDULUM_TOTAL_LENGTH * 0.5f;
            len1 = DOUBLE_PENDULUM_TOTAL_LENGTH * 0.5f;
            angle0 = angle1 = MathHelper.Pi;
            angVel0 = angVel1 = 0;
            m0 = m1 = 1.0f;

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
                    case phase.ThreeBodyPreparation11: {
                            threebodyPreparation11();
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

            if (fractalRing != null) {
                fractalRing.timeLeft = FractalRing.TRANSIT - 1;
            }

            CameraPlayer.deActivate();

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
                dimNode.setConnectionTarget(this);
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
                brightNode.setConnectionTarget(dimNode);
            }
        }

        private void electricCharge3()
        {
            if ((int)Timer == 0)
            {
                NPC.dontTakeDamage = false;

                dimNode.setPhase((int)DimNode.phase.ORBIT);
                dimNode.setDrawConnection(false);
                dimNode.NPC.dontTakeDamage = false;

                brightNode.setPhase((int)BrightNode.phase.ORBIT);
                brightNode.setDrawConnection(false);
                brightNode.NPC.dontTakeDamage = false;
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

            hover(target.Center + (-MathHelper.Pi/6).ToRotationVector2() * 600f, 20f, 0.3f, 1200, 10, 500f, 0.9f * Math.Min(1, Timer/SinLaser.TRANSIT));
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
            hover(target.Center + (MathHelper.Pi * 5 / 6).ToRotationVector2() * 600f, 20f, 0.3f, 1200, 10, 500f, 0.95f);
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
            }

            hover(target.Center + (MathHelper.Pi * 5 / 6).ToRotationVector2() * 600f, 20f, 0.3f, 1200, 10, 500f, 0.85f);

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
            hover(target.Center + Vector2.UnitY * 0.25f * DOUBLE_PENDULUM_TOTAL_LENGTH, 20f, 0.3f, 1200, 10, 500f, 0.85f);
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
            attractPlayer(FractalRing.RADIUS);

            doublePendulumMotion();

            // render
            dimNode.NPC.Center = NPC.Center + len0 * (MathHelper.PiOver2 - angle0).ToRotationVector2();
            brightNode.NPC.Center = dimNode.NPC.Center + len1 * (MathHelper.PiOver2 - angle1).ToRotationVector2();

            float fixedTimer = Timer - 0.5f * 60;


            int fac = (int)fixedTimer % (int)(2 * DOUBLE_PENDULUM_PERIOD);

            // brightNode Laser
            if (fac < DOUBLE_PENDULUM_PERIOD)
            {
                if (fac > 0.05 * DOUBLE_PENDULUM_PERIOD && fac < 0.65 * DOUBLE_PENDULUM_PERIOD)
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

            attractPlayer(FractalRing.RADIUS);

            doublePendulumMotion();

            brightNode.NPC.Center = NPC.Center + len0 * (MathHelper.PiOver2 - angle0).ToRotationVector2();
            dimNode.NPC.Center = brightNode.NPC.Center + len1 * (MathHelper.PiOver2 - angle1).ToRotationVector2();

            
            float fac = Timer %(DOUBLE_PENDULUM_PERIOD2);

            // brightNode lasers
            if (fac <= 0.25 * DOUBLE_PENDULUM_PERIOD2)
            {
                float prog = fac / (0.25f * DOUBLE_PENDULUM_PERIOD2);
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

        private void attractPlayer(float restrictionDist)
        {
            foreach (Player p in Main.player)
            {
                float dist = Vector2.Distance(p.Center, NPC.Center);
                if (p.active && dist > restrictionDist)
                    p.velocity += (float)Math.Min(15, 2.5 * dist) * (NPC.Center - p.Center).SafeNormalize(Vector2.Zero);
            }
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

                if (waterDropController == null) {
                    waterDropController = new WaterDropController(NPC.Center, -(GeneralTimer/1800) * MathHelper.TwoPi);
                }

                dimNode.setPhase((int)DimNode.phase.THREEBODY_MOTION);
                brightNode.setPhase((int)DimNode.phase.THREEBODY_MOTION);

                CameraPlayer.activate();
            }

            NPC.velocity *= 0;

            if (waterDropController != null)
                waterDropController.updateAll(-(GeneralTimer / 1800) * MathHelper.TwoPi);

            if (Timer <= 90)
            {
                CameraPlayer.setDisplacement(Vector2.Lerp(Vector2.Zero,
                    (NPC.Center - Main.ScreenSize.ToVector2() / 2) - Main.screenPosition, (Timer / 90f) * (Timer / 90f)));
            }
            else {
                CameraPlayer.setDisplacement(NPC.Center - Main.ScreenSize.ToVector2() / 2 -Main.screenPosition);
            }

            if (Timer >= 30 && Timer < 150 && (int)Timer % 12 == 0) {
                waterDropController.summonWaterDrop();
            }

            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustDirect(NPC.Center + Main.rand.NextVector2Unit() * 450, 0,0,
                    DustID.FlameBurst);
                d.noGravity = true;
                d.velocity *= 0;
                d.scale *= 3;
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
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RainbowRod);
                d.velocity = Main.rand.NextVector2Unit() * 30f;
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
        }


        private void createDimNode()
        {
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DimNode>());
            dimNode = (DimNode)Main.npc[id].ModNPC;
            dimNode.NPC.dontTakeDamage = true;
            dimNode.setOwner(this);
            dimNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }

        private void createBrightNode()
        {
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
                    (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BrightNode>());
            brightNode = (BrightNode)Main.npc[id].ModNPC;
            brightNode.NPC.dontTakeDamage = true;
            brightNode.setOwner(this);
            brightNode.setTarget(target);
            SoundEngine.PlaySound(SoundID.DrumTomHigh);
        }

        #endregion
    }
}
