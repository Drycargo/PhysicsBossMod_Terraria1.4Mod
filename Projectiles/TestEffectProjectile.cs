using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles
{
    public class TestEffectProjectile:ModProjectile
    {
        private Texture2D tex;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 180;
            tex = ModContent.Request<Texture2D>(Texture).Value;
        }

        public override void AI()
        {
            base.AI();
            Projectile.velocity *= 0;
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

            PhysicsBoss.maskEffect.Parameters["texSize"].SetValue(Main.ScreenSize.ToVector2());
            PhysicsBoss.maskEffect.Parameters["polarizeShrink"].SetValue(2f);
            PhysicsBoss.maskEffect.Parameters["timer"].SetValue(-(float)Main.time * 0.025f);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicPolarize"].Apply();

            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Main.spriteBatch.Draw(tex, new Rectangle(0,0,Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
