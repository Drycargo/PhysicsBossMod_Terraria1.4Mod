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
        public const int TRAILING_CONST = 15;
        public const int TRAIL_LENGTH = 1500;

        private Texture2D tex;
        private Texture2D trailTex;

        private Vector2 aim;

        private float tailColorProgress;
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(20 * 60);
            Projectile.damage = 80;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Timer = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            trailTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvance").Value;

            currState = state.STATIC;

            aim = Vector2.Zero;
            tailColorProgress = 0;
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
                        if (tailColorProgress < 1) {
                            tailColorProgress += 1 / 180f;
                        }
                        break;
                    }
                case state.LAUNCHED:
                    {
                        if (Projectile.velocity.Length() < 60f)
                            Projectile.velocity *= 1.08f;
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
                    aim, Color.Lerp(Color.Blue, Color.White, 0.5f) * Math.Min(1f, Timer / 90f), 7.5f);

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

            PhysicsBoss.shineEffect.Parameters["shineColor"].SetValue(
                2.5f * Color.Lerp(Color.Blue , Color.Indigo, tailColorProgress).ToVector4());
            PhysicsBoss.shineEffect.Parameters["threashold"].SetValue(0.9f);
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["Beam"].Apply();

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
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.Center,Projectile.oldPos[TRAILING_CONST - 1], 2f, ref point))
                    return true;
            }
            return base.Colliding(new Rectangle((int)(projHitbox.Left + tex.Width * 0.7f),
                (int)(projHitbox.Top + tex.Height * 0.7f), (int)(tex.Width * 0.6f), (int)(tex.Height * 0.6f)), targetHitbox);
        }

        public void setStateAim(Vector2 aimPos) {
            if (currState == state.STATIC) {
                Timer = 0;
                currState = state.AIM;
            }

            aim = aimPos;
        }

        public void launch() {
            if (currState == state.AIM)
            {
                currState = state.LAUNCHED;
                if (aim != Vector2.Zero)
                    Projectile.velocity = (aim - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
            }
        }
    }
}
