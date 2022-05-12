using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class AizawaController:TrailingStarController
    {
        public const int TRAILING_CONST = 15;

        private Vector2 dir;
        private VertexStrip tail = new VertexStrip();
        private Texture2D tex;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aizawa Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "Aizawa托尾星控制");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 30;
            dir = Vector2.Zero;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            tex = ModContent.Request<Texture2D>(Texture).Value;
        }

        public override void AI()
        {
            if (Timer == 0)
                dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            Projectile.velocity -= dir * 0.1f;
            Projectile.velocity += 0.001f * dir.RotatedBy(MathHelper.PiOver2);
            Projectile.rotation = Projectile.velocity.ToRotation();

            Timer++;
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

            Main.graphics.GraphicsDevice.Textures[0] =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/Smoke").Value;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            PhysicsBoss.trailingEffect.Parameters["tailStart"].SetValue(2 * Color.White.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["tailEnd"].SetValue(2 * Color.Purple.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time * 0.02f);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicTrailSimple"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White,
                progress => Projectile.width * 0.35f,
                tex.Size() / 2 - Main.screenPosition, TRAILING_CONST);

            /*
            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress=> Color.Lerp(Color.Purple, Color.White, progress) * (1- 0.3f * progress), 
                progress=> Projectile.width * 0.5f * (1-progress),
                tex.Size() / 2 - Main.screenPosition, TRAILING_CONST);
            */
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,
                Projectile.position - Main.screenPosition, Color.Lerp(Color.Purple, Color.White, 0.5f));

        }
    }
}
