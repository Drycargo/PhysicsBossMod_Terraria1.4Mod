using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPC.Boss.ChaosTheory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles
{
    public class BlockFractalLaser: ModProjectile
    {
        public const float LENGTH = 2500;
        public const float TRANSIT = 10;
        public const float WIDTH = 40;

        private float prog;
        private Color drawColor = Color.Red;

        protected Texture2D tex;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Block Fractal Laser");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "块状分型杂色激光");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(0.8 * 60);
            Projectile.damage = 100;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.width = tex.Width;
            Projectile.height = (int)WIDTH;
            Projectile.rotation = 0f;

            Timer = 0;

            prog = 0.0f;
        }

        public override void AI()
        {
            prog = Math.Min(Math.Min(Timer / TRANSIT, Projectile.timeLeft / TRANSIT), 1f);
            Projectile.velocity *= 0;

            if ((int)Timer == 0)
                SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);


            if (Timer < TRANSIT)
            {
                GlobalEffectController.blur(1f * (1 - (Timer / TRANSIT)));
                GlobalEffectController.shake(5f * (1 - (Timer/TRANSIT)));
                
                for (int j = 0; j < 50; j++)
                {
                    Vector2 pos = (Vector2.UnitX * ((float)j + Main.rand.NextFloat() - 0.5f)/ 
                        50 * LENGTH).RotatedBy(Projectile.rotation) + Projectile.Center;
                    for (int i = 0; i < 4; i++)
                    {
                        Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Torch);
                        d.color = drawColor;
                        d.noGravity = true;
                        d.velocity = 7f * (Projectile.rotation +
                            (i % 2 == 0 ? 1 : -1) * MathHelper.PiOver2).ToRotationVector2();
                    }
                }
            }

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

            PhysicsBoss.shineEffect.Parameters["shineColor"].SetValue(prog * 2f * drawColor.ToVector4());
            PhysicsBoss.shineEffect.Parameters["threashold"].SetValue(0.6f);
            PhysicsBoss.shineEffect.Parameters["timer"].SetValue(-(float)Timer * 250);
            PhysicsBoss.shineEffect.Parameters["texSize"].SetValue(tex.Size());
            PhysicsBoss.shineEffect.Parameters["tex0"].SetValue(
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvanceTexture").Value);
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["DynamicBeam"].Apply();

            Main.graphics.GraphicsDevice.Textures[0] = tex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            Main.spriteBatch.Draw(tex, Projectile.Center + LENGTH * Projectile.rotation.ToRotationVector2() / 2 - Main.screenPosition,
                null, drawColor, Projectile.rotation, tex.Size() / 2f, 
                new Vector2(LENGTH / tex.Width, prog * WIDTH / tex.Height), SpriteEffects.None, 0);

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

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * LENGTH, 
                   WIDTH / 2 * prog, ref point);
        }

        public void setColor(Color c) {
            drawColor = c;
        }
    }
}
