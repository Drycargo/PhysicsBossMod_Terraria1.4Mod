﻿using Microsoft.Xna.Framework;
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
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarController: ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "托尾星控制");
            base.SetStaticDefaults();
        }
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public static Color[] colors = { Color.Green, Color.Cyan, Color.Blue, Color.DarkViolet};

        private Queue<TrailingStarChaotic> stars;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(8 * 60);
            Projectile.damage = 0;
            Projectile.velocity = Vector2.Zero;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;
            Timer = 0;

            stars = new Queue<TrailingStarChaotic>();
        }

        public override void AI()
        {
            Projectile.velocity *= 0;

            Timer++;
        }

        public void summonStarBundle() {
            for (int i = 0; i < 4; i++)
                summonStar(colors[i]);
            SoundEngine.PlaySound(SoundID.Item25, Projectile.Center);
        }

        private void summonStar(Color c) {
            int id = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    Projectile.Center + 20 * Main.rand.NextFloat() * Main.rand.NextVector2Unit(), Vector2.Zero, ModContent.ProjectileType<TrailingStarChua>(), 50, 0);
            TrailingStarChaotic tsc = (TrailingStarChaotic)Main.projectile[id].ModProjectile;
            tsc.setOwner(this);
            tsc.setColor(c);

            stars.Enqueue(tsc);
        }

        public void releaseStarBundle(Player target)
        {
            for (int i = 0; i < 4; i++)
                releaseStar(target);
            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);
        }

        private void releaseStar(Player target) {
            try
            {
                TrailingStarChaotic tsc = stars.Dequeue();
                tsc.releaseProj(target);
            }
            catch (System.InvalidOperationException e) {
                Main.NewText("TrailingStarController.stars is empty.", Color.Red);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
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

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,
                Projectile.position - Main.screenPosition, Color.LightCyan);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
