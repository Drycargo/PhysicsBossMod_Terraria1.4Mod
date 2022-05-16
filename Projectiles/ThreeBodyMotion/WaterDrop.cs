using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles.TrailingStarMotion;
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


namespace PhysicsBoss.Projectiles.ThreeBodyMotion
{
    public class WaterDrop: ModProjectile
    {
        private VertexStrip tail = new VertexStrip();
        public const int TRAILING_CONST = 8;
        public const int TRAIL_LENGTH = 1500;

        private Texture2D tex;
        private Texture2D trailTex;

        private Vector2 aim;
        public enum state {
            STATIC = 0,
            AIM = 1,
            LAUNCHED = 2,
        }

        private state currState;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Water Drop");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "水滴");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(20 * 60);
            Projectile.damage = 100;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            trailTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvance").Value;

            currState = state.STATIC;

            aim = Vector2.Zero;
        }

        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();
            switch (currState) {
                case state.STATIC: {
                        Projectile.velocity *= 0;
                        for (int i = 0; i < TRAILING_CONST; i++) {
                            Projectile.oldRot[i] = Projectile.rotation;
                            Projectile.oldPos[i] = Projectile.position - 
                                Projectile.rotation.ToRotationVector2() * ((float)TRAIL_LENGTH / (float)TRAILING_CONST * (float)i) ;
                        }
                        break;
                    }
                case state.AIM: {
                        if (Projectile.velocity == Vector2.Zero) {
                            Projectile.velocity = 25f * Projectile.rotation.ToRotationVector2();
                        }
                        break;
                    }
                case state.LAUNCHED:
                    {
                        break;
                    }
                default: break;
            }

            if ((int)Timer % 120 == 0) {
                Vector2 pos = Projectile.Center - Projectile.rotation.ToRotationVector2() * Projectile.width * 0.4f;

                for (int i = 0; i < 60; i++) {
                    Dust d = Dust.NewDustDirect(pos, 0,0,DustID.GemSapphire);
                    d.scale *= 2.5f;
                    d.noGravity = true;
                    d.velocity = Main.rand.NextVector2Unit() * 5f;
                    d.velocity.Y *= 2f;
                    d.velocity.X -= 10;
                    d.velocity = d.velocity.RotatedBy(Projectile.rotation);
                }
            }

            Timer++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (currState == state.AIM)
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    aim, Color.Lerp(Color.Blue, Color.White, 0.5f), 7.5f);

            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White * (1f - progress),
                progress => 10f * (1 - progress),
                tex.Size()/2-Main.screenPosition, TRAILING_CONST);

            Main.graphics.GraphicsDevice.Textures[0] = trailTex;

            PhysicsBoss.shineEffect.Parameters["shineColor"].SetValue(5 * Color.Blue.ToVector4());
            PhysicsBoss.shineEffect.Parameters["threashold"].SetValue(0.9f);
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["Beam"].Apply();

            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                null, Color.Lerp(Color.White, lightColor, 0.25f), Projectile.rotation, tex.Size() / 2, 
                0.6f, SpriteEffects.None, 0);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (currState == state.STATIC) {
                float point = 0;
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.Center,Projectile.oldPos[TRAILING_CONST - 1], 5f, ref point);
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        public void setStateAim(Vector2 aimPos) {
            if (currState == state.STATIC) {
                aim = aimPos;
                currState = state.AIM;
            }
        }

        public void launch() {
            if (currState == state.AIM)
            {
                currState = state.LAUNCHED;
                if (aim != Vector2.Zero)
                    Projectile.velocity = (aim - Projectile.Center).SafeNormalize(Vector2.UnitX) * 80f;
            }
        }
    }
}
