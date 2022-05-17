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
    public class Sun : ModProjectile
    {
        public const float SPEED_LIMIT = 25f;
        public const float DIST_LIMIT = 200f;
        public const float PERIOD = 1/ 0.0075f;

        public const int TRAILING_CONST = 10;

        private Texture2D tex;
        private Texture2D contentTex;

        private float mass;
        private Vector2 realPos;
        private Vector3 realVel;

        public Vector2 RealPos {
            get { return realPos; }
            set { realPos = value; }
        }

        public Vector3 RealVel
        {
            get { return realVel; }
            set { realVel = value; }
        }

        public float Mass {
            get { return mass; }
            set { mass = value; }
        }

        //private 
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sun");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(20 * 60);
            Projectile.damage = 50;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            contentTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ThreeBodyMotion/SunContent").Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            mass = 1f + 0.5f * (Main.rand.NextFloat() - 0.5f);
        }

        public override void AI() {
            //Projectile.velocity *= 0;
            for (int i = 0; i < 3; i++) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Gold);
            }

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3());
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(contentTex);
            PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.9f);
            PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue( Color.LightGoldenrodYellow.ToVector4());
            PhysicsBoss.maskEffect.Parameters["contentTint"].SetValue(2 * Color.OrangeRed.ToVector4());
            PhysicsBoss.maskEffect.Parameters["timer"].SetValue(Timer / PERIOD);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicMaskTint"].Apply();

            Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }
    }
}
