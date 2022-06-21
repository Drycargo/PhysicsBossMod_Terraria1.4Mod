using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.ButterflyEffect
{
    public class Tornado: ModProjectile
    {
        public const int TRANSIT = 30;
        public const float ORBIT_RADIUS = 35f;
        public const int ORBIT_PERIOD = 60;
        public const float FOCAL = 160;
        public const int KERNEL_WIDTH = 80;
        public const int PLATE_COUNT = 15;
        public const int RELEASE_INTERVAL = 35;

        public enum phase: int {
            PREPARATION,
            SPAWNED,
            KILLED
        }

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;
        private phase currPhase;
        private TornadoPlate[] plates = new TornadoPlate[PLATE_COUNT];
        private int plateCount;

        private Player target = null;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tornado");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "龙卷风");

            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 0;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;

            currPhase = phase.PREPARATION;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            Projectile.timeLeft = (int)(3 * 60);
            plateCount = 0;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 1 && currPhase != phase.KILLED)
                setKill();

            switch (currPhase)
            {
                case phase.PREPARATION:
                    {
                        break;
                    }
                case phase.SPAWNED:
                    {
                        if ((int)Timer % 3 == 0 && plateCount < PLATE_COUNT) {
                            try
                            {
                                plates[plateCount++] = (TornadoPlate)(Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(),
                                    Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TornadoPlate>(), 80, 2).ModProjectile);
                            }
                            catch (IndexOutOfRangeException e) { 
                            }
                        }

                        if (Timer < 120) {
                            GlobalEffectController.shake((120f - Timer)/120f * 20f);
                            GlobalEffectController.blur((120f - Timer)/120f);
                        }

                        for (int i = 0; i < plateCount; i++) {
                            plates[i].Projectile.Center = new Vector2(Projectile.Center.X, 
                                Projectile.Center.Y - (i - (PLATE_COUNT/2)) * TornadoPlate.HEIGHT * 0.75f);
                        }

                        break;
                    }
                case phase.KILLED: {
                        if (Projectile.timeLeft % RELEASE_INTERVAL == 0)
                        {
                            int topIndex = Projectile.timeLeft / RELEASE_INTERVAL - 1 + PLATE_COUNT / 2;
                            int bottomIndex = PLATE_COUNT - topIndex - 1;

                            Vector2 dir = Vector2.UnitX;
                            if (target != null && target.Center.X < Projectile.Center.X)
                                dir *= -1;

                            try
                            {
                                if (topIndex == bottomIndex)
                                {
                                    plates[topIndex].release(dir);
                                }
                                else
                                {
                                    plates[topIndex].release(dir);
                                    plates[bottomIndex].release(dir);
                                }
                            }
                            catch (NullReferenceException e)
                            {

                            }
                        }
                        break;
                    }
            }
            Projectile.velocity *= 0;

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            switch (currPhase) {
                case phase.PREPARATION: {
                    Main.spriteBatch.End();

                    Main.spriteBatch.Begin(SpriteSortMode.Immediate,BlendState.Additive);

                        /*
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                            BlendState.Additive,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            RasterizerState.CullNone, null,
                            Main.GameViewMatrix.TransformationMatrix);
                        */

                    for (int i = 0; i < 3; i++) {
                        drawTornadoKernel(i);
                    }

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        Main.DefaultSamplerState,
                        DepthStencilState.None,
                        RasterizerState.CullNone, null,
                        Main.GameViewMatrix.TransformationMatrix);
                        break;
                }
                case phase.SPAWNED:
                    {   
                        break;
                    }
                case phase.KILLED: {
                        break;
                    }
                default: break;
            }


            return false;
        }

        public void spawn() {
            if (currPhase == phase.PREPARATION) {
                GlobalEffectController.flash(0.2f, Projectile.Center - Main.screenPosition, 40, 30);
                currPhase = phase.SPAWNED;
                Timer = 0;
            }
        }

        private void drawTornadoKernel(int index) {
            float angle = (index/3f + Timer / ORBIT_PERIOD) * MathHelper.TwoPi;

            float x = (float)(Math.Cos(angle) * ORBIT_RADIUS);
            float z = (float)(Math.Sin(angle) * ORBIT_RADIUS);

            float factor = FOCAL/(FOCAL - z);

            PhysicsBoss.maskEffect.Parameters["timer"].SetValue((Timer + index * 10) * 0.03f);
            PhysicsBoss.maskEffect.Parameters["tint"].SetValue(Color.DeepSkyBlue.ToVector4() * factor * Math.Min(1, Timer/TRANSIT));
            PhysicsBoss.maskEffect.Parameters["fadeThreshold"].SetValue(0.25f);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorTintHFade"].Apply();

            Main.spriteBatch.Draw(tex, new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X 
                + factor * (x - 0.5f * KERNEL_WIDTH)), 0, (int)(factor * KERNEL_WIDTH), Main.screenHeight), Color.White);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public void setKill(Player t = null) {
            if (currPhase != phase.KILLED) {
                currPhase = phase.KILLED;
                Projectile.timeLeft = (PLATE_COUNT + 1) / 2 * RELEASE_INTERVAL + 1;
                target = t;
            }
        }
    }
}
