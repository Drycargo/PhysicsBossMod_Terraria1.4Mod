using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles
{
    public class TrailingStar:ModProjectile
    {
        private VertexStrip tail = new VertexStrip();
        public static readonly int TRAILING_CONST = 25;

        private Texture2D backTex;
        private Texture2D luminanceTex;
        private Texture2D colorTex;
        public float Timer {
            get {return Projectile.ai[0];}
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Trailing Star");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(3.75 * 60);
            Projectile.damage = 50;
            Projectile.velocity = Vector2.UnitY*15;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;
            Projectile.rotation = MathHelper.PiOver2; // Must be perpendicular to direction of velocity

            backTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/BlockFractalLaser").Value;
            luminanceTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/LuminanceGradient").Value;
            colorTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/BlueGreenGradient").Value;
        }

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, 0, 0, DustID.FireworkFountain_Blue);
                d.noGravity = true;
                d.velocity *= 10;
            }

            Projectile.velocity = 30f * (MathHelper.Pi*3f/5).ToRotationVector2();
        }

        public override void AI()
        {
            if (Timer < 30)
            {
                if ((int)Timer % 6 == 0)
                    Projectile.velocity = Projectile.velocity.RotatedBy(4f / 5f * MathHelper.Pi);
            }
            else if ((int)Timer == 30)
            {
                Projectile.velocity = -10f * Vector2.UnitY;
            }
            else
            {
                if (Projectile.velocity.Length() < 45)
                {
                    Projectile.velocity.Y -= 0.5f;
                }
            }

            for (int i = 0; i < 3; i++) {
                Dust.NewDustDirect(Projectile.Center, 0,0,DustID.RainbowRod).noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, lightColor);
            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = backTex;
            Main.graphics.GraphicsDevice.Textures[1] = luminanceTex;
            Main.graphics.GraphicsDevice.Textures[2] = colorTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;


            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, 
                progress => {
                    Color c1 = Color.LightGreen * progress;
                    Color c2 = Color.Cyan*((float)(1.0f - progress));
                    return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B)*2.5f;
                }, 
                progress => (progress<0.1 ? MathHelper.Lerp(Projectile.width/6f, 18f, progress*10):MathHelper.Lerp(20f, 0f, progress)), 
                tex.Size()/2 - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, Color.White);

            //Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3());
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            if (Projectile.oldPos[TRAILING_CONST - 1] == Vector2.Zero)
                return base.Colliding(projHitbox, targetHitbox);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.oldPos[TRAILING_CONST-1]+Projectile.Size/2, 35, ref point);
        }

    }
}
