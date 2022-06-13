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
        private Vector2 targetOriginalPos;
        private WaterDropController wdc = null;
        private float rainProgress;

        private LogisticMap.LogisticMap[] maps = new LogisticMap.LogisticMap[10];
        private float lightningProgress = 0;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 600;
            tex = ModContent.Request<Texture2D>(Texture).Value;
            rainProgress = 0;
        }
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void AI()
        {
            base.AI();

            Dust.NewDust(Projectile.Center, 0,0, ModContent.DustType<BlockDust>());
            //Projectile.velocity *= 0;

            /*
            if (lightningProgress < 1f)
                lightningProgress += 1 / 90f;

            if ((int)Timer % 10 == 0)
            {
                float dispY = LargeLightningBolt.WIDTH * (4.25f - lightningProgress * 3.25f);
                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(0,
                        (i % 2 == 0 ? 1f : -1f) * dispY), (MathHelper.Lerp(1, -1, Timer / (600)) * MathHelper.Pi / 6).ToRotationVector2(),
                        ModContent.ProjectileType<LargeLightningBolt>(), 60, 0);
                }
            }*/

            /*
            if (Timer < 120) {
                if ((int)Timer % 12 == 0) {
                    int ind = (int)(Timer / 12);
                    maps[ind] = (LogisticMap.LogisticMap)(Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(),
                        Projectile.Center,Vector2.Zero, ModContent.ProjectileType<LogisticMap.LogisticMap>(), 20,0).ModProjectile);
                    if (ind == 0)
                        maps[ind].initialize();
                    else
                        maps[ind].initialize(maps[ind - 1]);
                }
            } else if ((Timer - 120) % 8 == 0) {
                int ind = (int)((Timer - 120) / 8);
                if (ind < 10)
                {
                    maps[ind].materialize();
                    maps[ind].swing();
                }
            }
            */

            /*
            for (int i = 0; i < 2; i++)
            {
                float devX = 100 + 600 * (1 - rainProgress * Main.rand.NextFloat());

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), new Vector2(
                        Projectile.Center.X + 500 + (i % 2 == 0 ? -1 : 1) * devX, Projectile.Center.Y - 1200),
                        Vector2.UnitY.RotatedBy(MathHelper.Pi / 12) * 35f, ModContent.ProjectileType<RainDrop>(), 15, 0);
                }
            }

            if (rainProgress < 1f)
                rainProgress += 1 / 60f;
            */

            /*
            if (wdc == null) {
                wdc = new WaterDropController(Projectile.Center, 0);
                wdc.summonAll(null);
            }

            if (wdc != null) {
                wdc.aimAll(Projectile.Center);
                wdc.updateAll(Projectile.timeLeft * 0.01f);
            }*/
            Timer++;
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
