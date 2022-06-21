using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.ButterflyEffect
{
    public class RainDrop: ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rain Drop");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "雨滴");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;

            Projectile.timeLeft = (int)(0.5f * 60);
            Projectile.damage = 15;

            Projectile.width = 60;
            Projectile.height = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Projectile.position - Main.screenPosition, null,
                Color.White, Projectile.rotation, Vector2.Zero, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.rand.NextFloat() < 0.25) {
                SoundEngine.PlaySound(SoundID.Item12, Projectile.Center);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center - Projectile.rotation.ToRotationVector2() * 0.5f * Projectile.width,
                Projectile.Center + Projectile.rotation.ToRotationVector2() * 0.5f * Projectile.width,
                Projectile.height, ref point);
        }
    }
}
