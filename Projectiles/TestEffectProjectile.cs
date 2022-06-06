using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Projectiles.ThreeBodyMotion;
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

        WaterDropController wdc = null;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 90;
            tex = ModContent.Request<Texture2D>(Texture).Value;

        }

        public override void AI()
        {
            base.AI();
            Projectile.velocity *= 0;

            /*
            if (wdc == null) {
                wdc = new WaterDropController(Projectile.Center, 0);
                wdc.summonAll(null);
            }

            if (wdc != null) {
                wdc.aimAll(Projectile.Center);
                wdc.updateAll(Projectile.timeLeft * 0.01f);
            }*/

        }

        public override bool PreDraw(ref Color lightColor)
        {
            /*
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width, Main.screenTarget.Height);

            Main.spriteBatch.End();
            graphicsDevice.SetRenderTarget(screenTemp);
            graphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);


            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied);

            PhysicsBoss.maskEffect.Parameters["texSize"].SetValue(Main.ScreenSize.ToVector2());
            PhysicsBoss.maskEffect.Parameters["polarizeShrink"].SetValue(2f);
            PhysicsBoss.maskEffect.Parameters["dispTimer"].SetValue(-(float)Main.time * 0.025f);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicPolarize"].Apply();

            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Main.spriteBatch.Draw(tex, new Rectangle(0,0,Main.screenWidth, Main.screenHeight), Color.White);


            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive);
            Main.spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
            
            PhysicsBoss.maskEffect.Parameters["phaseTimer1"].SetValue((float)Main.time * 0.025f);
            PhysicsBoss.maskEffect.Parameters["phaseTimer2"].SetValue((float)Main.time * 0.025f);
            PhysicsBoss.maskEffect.Parameters["texColorMap"].SetValue(ModContent.Request<Texture2D>("PhysicsBoss/Asset/ColorMap").Value);
            PhysicsBoss.maskEffect.Parameters["brightThreshold"].SetValue(0.8f);
            PhysicsBoss.maskEffect.Parameters["darkThreshold"].SetValue(0.3f);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorMap"].Apply();
            Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            */
            return false;
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            if (wdc != null) {
                wdc.launchAll();
                wdc = null;
            }
        }
    }
}
