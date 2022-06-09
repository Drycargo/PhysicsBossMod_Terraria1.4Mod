using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPCs;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
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


namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class ThreeScrollController: TrailingStarController
    {
        public const int TRAILING_CONST = 30;
        public const float SPEED = 20f;
        private VertexStrip tail = new VertexStrip();

        private Vector2 dir;
        private Texture2D tex;
        private bool activated;

        private TargetEnemy owner;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("ThreeScroll Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "三轴托尾星控制");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 30;
            dir = Vector2.UnitX;


            tex = ModContent.Request<Texture2D>(Texture).Value;
            activated = false;
            Projectile.timeLeft = (int)(3 * 60);
        }

        public override void AI()
        {
            if ((int)Timer == 0)
                Projectile.velocity *= Vector2.Zero;
            if (owner != null)
            {
                if (!activated)
                {
                    snipe();
                    Projectile.position = owner.NPC.Center;
                    Projectile.timeLeft++;

                    if (Timer >= ChaosTheory.DOUBLE_PENDULUM_PERIOD / 4)
                        release();
                }
                else
                {
                    /*
                    if (Projectile.velocity.Length() < SPEED) {
                        Projectile.velocity += 5f * dir.SafeNormalize(Vector2.UnitX);
                    }
                    */

                    Projectile.velocity *= 0.95f;

                    if (Projectile.velocity.Length() < 2f) {
                        releaseStarBundle();
                        Projectile.Kill();
                    }
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Timer++;
        }

        private void snipe()
        {
            if (owner.getTarget() != null && owner.getTarget().active)
            {
                Player p = owner.getTarget();
                float dTime = (p.Center - Projectile.Center).Length() / (SPEED/2);
                dir = Vector2.Lerp(dir, p.Center + 1.5f * p.velocity * dTime - Projectile.Center, 0.05f).SafeNormalize(Vector2.UnitX);
            }
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (!activated && dir != Vector2.Zero)
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center, Projectile.Center + dir, 
                    Color.LightBlue * 0.8f * Math.Min(Timer/15f, 1), 20f);

            if (activated) {
                Main.graphics.GraphicsDevice.Textures[0] =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/Smoke").Value;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

                PhysicsBoss.trailingEffect.Parameters["tailStart"].SetValue(3 * Color.White.ToVector4());
                PhysicsBoss.trailingEffect.Parameters["tailEnd"].SetValue(2 * Color.LightBlue.ToVector4());
                PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time * 0.01f);
                PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicTrailSimple"].Apply();

                tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                    progress => Color.White,
                    progress => Projectile.width * 0.15f * (1 - progress),
                    tex.Size() / 2 - Main.screenPosition, TRAILING_CONST);

                tail.DrawTrail();
                
                int l = Projectile.oldPos.Length;
                for (int i = l - 1; i >= 0; i--)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        Color c = Color.Azure * (1f - (float)i / l) * 0.5f;
                        Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + tex.Size()/2 - Main.screenPosition,null,c, 
                            Projectile.oldRot[i], tex.Size() / 2, 1f, SpriteEffects.None, 0);
                    }
                }
            }

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,
                Projectile.position - Main.screenPosition, Color.Lerp(Color.LightBlue, Color.White, 0.5f));

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

        }

        public void release()
        {
            activated = true;
            Projectile.velocity = SPEED * dir;
            SoundEngine.PlaySound(SoundID.Item12, Projectile.Center);
        }

        public void setOwner(TargetEnemy owner) {
            this.owner = owner;
        }
    }
}
