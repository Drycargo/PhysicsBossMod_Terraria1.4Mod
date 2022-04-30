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

namespace PhysicsBoss.Projectiles.ConwayGame
{
    public class ConwayGameController: ModProjectile
    {
        public const int ROW = 9;
        public const int COL = 9;

        private ConwayBlock[][] blocks;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Conway Game Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "康威游戏控制");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(8 * 60);
            Projectile.damage = 0;

            Projectile.width = 0;
            Projectile.height = 0;

            blocks = new ConwayBlock[ROW][];
            for (int i = 0; i < ROW; i++)
            {
                blocks[i] = new ConwayBlock[COL];
                for (int j = 0; j < COL; j++)
                {
                    blocks[i][j] = new ConwayBlock(i - (ROW / 2), j - (COL / 2), 1.0f);
                }
            }
        }

        public override void AI()
        {
            Projectile.velocity *= 0;
        }

        public void preUpdate()
        {
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    updateSingleBlock(i, j);
                }
            }
        }

        public void update()
        {
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    blocks[i][j].setPhase();
                }
            }
        }

        private void updateSingleBlock(int r, int c)
        {
            int aliveCount = 0;
            int row, col;

            for (int i = r - 1; i <= r + 1; i++)
            {
                row = (i < 0) ? ROW - 1 : i % ROW;

                for (int j = c - 1; j <= c + 1; j++)
                {
                    col = (j < 0) ? COL - 1 : j % COL;

                    if (row != r && col!= c && blocks[row][col].alive()){
                        aliveCount++;
                        // overpopulation
                        if (aliveCount > 3)
                        {
                            blocks[r][c].setPrePhase(ConwayBlock.phase.DEAD);
                            return;
                        }
                    }
                }
            }

            // underpopulation
            if (aliveCount < 2)
                blocks[r][c].setPrePhase(ConwayBlock.phase.DEAD);

            // lives
            if (blocks[r][c].alive() && (aliveCount == 2 || aliveCount == 3))
                blocks[r][c].setPrePhase(ConwayBlock.phase.LIVE);

            // reproduction
            if (!blocks[r][c].alive() && aliveCount == 3)
                blocks[r][c].setPrePhase(ConwayBlock.phase.LIVE);

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    blocks[i][j].drawBlock();
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
