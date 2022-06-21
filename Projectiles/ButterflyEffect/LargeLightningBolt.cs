using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.ButterflyEffect
{
    public class LargeLightningBolt: ModProjectile
    {
        public const int TRAILING_CONST = 20;
        public const float WIDTH = 150f;
        public const float LENGTH = 4000f;
        public const float DEV = 0.2f * WIDTH;
        public const int TRANSIT = 10;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;
        private VertexStrip tail = new VertexStrip();
        private float currWidth;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Large Lightning Bolt");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "巨型闪电");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(50);
            Projectile.damage = 60;
            Projectile.hide = true;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            Projectile.oldPos = new Vector2[TRAILING_CONST];
            Projectile.oldRot = new float[TRAILING_CONST];

            Projectile.width = (int)WIDTH;

            Timer = 0;

            currWidth = 0;
        }


        public override void AI()
        {
            if (Timer < TRANSIT) {
                if (Timer == 0)
                {
                    if (Main.rand.NextBool())
                        SoundEngine.PlaySound(SoundID.Thunder, Projectile.Center);

                    Projectile.rotation = Projectile.velocity.ToRotation();

                    for (int i = 0; i < TRAILING_CONST; i++)
                    {
                        Projectile.oldRot[i] = Projectile.rotation;
                    }

                    Projectile.velocity *= 0;

                    for (int j = 0; j < 100; j++) {
                        Dust.NewDust(Vector2.Lerp(Projectile.Center - Projectile.rotation.ToRotationVector2() * 0.5f * LENGTH,
                            Projectile.Center + Projectile.rotation.ToRotationVector2() * 0.5f * LENGTH, (float)j/100),
                            (int)(LENGTH / 100f), (int)WIDTH, DustID.YellowStarDust);
                    }
                }

                GlobalEffectController.shake(((float)TRANSIT - Timer)/ (float)TRANSIT * 8f);
            }

            if ((int)Timer % 2 == 0) {
                for (int i = 0; i < TRAILING_CONST; i++) {
                    Projectile.oldPos[i].X = ((float)i - TRAILING_CONST / 2) / (float)TRAILING_CONST * LENGTH;
                    Projectile.oldPos[i].Y = (Main.rand.NextFloat() - 0.5f) * 2 * DEV;
                    Projectile.oldPos[i] = Projectile.Center + Projectile.oldPos[i].RotatedBy(Projectile.rotation);
                }
            }

            currWidth = Math.Min(Math.Min(1, Timer / TRANSIT), (float)Projectile.timeLeft/(float)TRANSIT);

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = tex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue(0.03f * Timer);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicContentX"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, progress => Color.White,
                progress => Projectile.width / 2 * currWidth, -Main.screenPosition, 
                TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            return false;
        }


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < TRAILING_CONST - 1; i++)
            {
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.oldPos[i], Projectile.oldPos[i + 1], Projectile.width / 2 * currWidth, ref point))
                    return true;
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}
