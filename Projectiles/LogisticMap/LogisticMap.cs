using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.LogisticMap
{
    public class LogisticMap : ModProjectile
    {
        public const float SPAN = 4.0f;
        public const int STEP = 25;

        public const int PERIOD = 60;
        public const int FADE_TRANSIT = 15;
        public const int SWING_PERIOD = 120;
        public const float TOT_WIDTH = 750f; 
        public const float TOT_HEIGHT = 450f;

        public virtual float LASER_WIDTH => 35f;
        public virtual float WIDTH => TOT_WIDTH;
        public virtual float HEIGHT => TOT_WIDTH;

        public enum phase: int {
            UNINITIALIZED = 0,
            INITIALZIED = 1,
            MATERIALIZED = 2,
            FALL = 3,
            SWING = 4
        }

        protected phase currPhase;
        private float[] data;
        protected Texture2D tex, contentTex, edgeTex;
        protected VertexStrip tail = new VertexStrip();
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Logistic Map");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "单峰映射");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(10 * 60);
            Projectile.damage = 50;
            currPhase = phase.UNINITIALIZED;
            data = new float[STEP];

            Projectile.oldPos = new Vector2[STEP];
            Projectile.oldRot = new float[STEP];

            Projectile.rotation = 0;

            tex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvanceTransparent").Value;
            edgeTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LogisticMap/CrossStar").Value;
            contentTex = ModContent.Request<Texture2D>(Texture).Value;
        }

        public override void AI()
        {
            if (currPhase != phase.UNINITIALIZED)
            {
                for (int i = 0; i < STEP; i++)
                {
                    Projectile.oldPos[i] = Projectile.Center + new Vector2((float)i / STEP * WIDTH, -data[i] * HEIGHT).
                        RotatedBy(Projectile.rotation);


                    if (i > 0)
                        Projectile.oldRot[i] = (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation();
                    else
                        Projectile.oldRot[i] = Projectile.rotation;
                }
            }

            if (Main.rand.NextFloat() < 0.25f)
                summonDust();

            if (currPhase == phase.FALL)
            {
                Projectile.velocity += 0.25f * Vector2.UnitY;
                Projectile.rotation += (Main.rand.NextFloat() - 0.5f) * 0.01f;
            }
            else
            {
                if (currPhase == phase.SWING)
                {
                    float factor = 1f - (float)Projectile.timeLeft / SWING_PERIOD;
                    Projectile.rotation = factor * factor * factor * MathHelper.TwoPi;
                    if ((int)Projectile.timeLeft == (SWING_PERIOD / 2))
                        SoundEngine.PlaySound(SoundID.Item19, Projectile.Center);
                }
                Projectile.velocity *= 0;
            }

            Timer++;
        }

        protected virtual void summonDust()
        {
            summonDust(Color.Red);
        }

        protected void summonDust(Color dustColor)
        {
            for (int i = 0; i < STEP; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.oldPos[i], (int)(WIDTH / STEP), (int)LASER_WIDTH, DustID.RainbowMk2);
                d.color = dustColor;
                d.noGravity = true;
            }
        }

        public void initialize(LogisticMap map = null) {
            if (map == null)
            {
                for (int i = 0; i < STEP; i++)
                {
                    data[i] = doMap(Main.rand.NextFloat(), (float)i / STEP * SPAN);
                }
            } else {
                float[] prevData = map.getData();
                for (int i = 0; i < STEP; i++)
                {
                    data[i] = doMap(prevData[i], (float)i / STEP * SPAN);
                }
            }

            if (currPhase == phase.UNINITIALIZED)
            {
                currPhase = phase.INITIALZIED;

                SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
            }
        }

        public void materialize() {
            if (currPhase != phase.MATERIALIZED)
            {
                currPhase = phase.MATERIALIZED;

                for (int i = 0; i < 50; i++)
                    summonDust();

                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
            }
        }

        public void setFall() {
            currPhase = phase.FALL;
            Projectile.timeLeft = 150;
        }

        public void swing() {
            Projectile.timeLeft = SWING_PERIOD;
            currPhase = phase.SWING;
        }

        public float[] getData() {
            if (currPhase != phase.UNINITIALIZED)
                return data;

            throw new NullReferenceException("Data not initialized");
        }

        public static float doMap(float x, float r) {
            return r * x * (1 - x);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            switch (currPhase) {
                case phase.UNINITIALIZED: {
                        break;
                    }
                case phase.INITIALZIED:
                    {
                        drawInitialized(Color.Red * 0.5f);
                        break;
                    }
                default:
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                            BlendState.NonPremultiplied,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            RasterizerState.CullNone, null,
                            Main.GameViewMatrix.TransformationMatrix);
                        drawEdges();

                        PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(contentTex);
                        PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.35f);
                        PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue(2 * Color.Red.ToVector4());
                        PhysicsBoss.maskEffect.Parameters["contentTint"].SetValue(Color.White.ToVector4());
                        PhysicsBoss.maskEffect.Parameters["timer"].SetValue(Timer / PERIOD);
                        PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicMaskTint"].Apply();

                        Main.graphics.GraphicsDevice.Textures[0] = tex;

                        tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                            progress => Color.White,
                            progress => LASER_WIDTH * 0.75f * Math.Min(1, (float)Projectile.timeLeft / FADE_TRANSIT),
                            -Main.screenPosition, STEP);

                        tail.DrawTrail();

                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            RasterizerState.CullNone, null,
                            Main.GameViewMatrix.TransformationMatrix);
                        break;
                    }
            }
            return false;
        }

        protected virtual void drawInitialized(Color drawColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            drawEdges();

            Main.graphics.GraphicsDevice.Textures[0] = tex;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => drawColor,
                progress => LASER_WIDTH * 0.3f * Math.Min(1, (float)Projectile.timeLeft / FADE_TRANSIT),
                -Main.screenPosition, STEP);

            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        protected void drawEdges()
        {
            Main.spriteBatch.Draw(edgeTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation,
                                        edgeTex.Size() / 2, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(edgeTex, Projectile.oldPos[STEP - 1] - Main.screenPosition, null, Color.White, Projectile.rotation,
                edgeTex.Size() / 2, 1f, SpriteEffects.None, 0);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (currPhase == phase.UNINITIALIZED || currPhase == phase.INITIALZIED)
                return false;

            for (int i = 0; i < STEP - 1; i++) {
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.oldPos[i], Projectile.oldPos[i + 1], LASER_WIDTH / 2, ref point))
                    return true;
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (currPhase != phase.UNINITIALIZED) {
                for (int i = 0; i < 120; i++) {
                    Dust d = Dust.NewDustDirect((i % 2 == 0? Projectile.Center : Projectile.oldPos[STEP - 1]),
                        0,0,DustID.RainbowRod);
                    d.velocity *= 3f;
                    d.noGravity = true;
                }
            }
        }
    }
}
