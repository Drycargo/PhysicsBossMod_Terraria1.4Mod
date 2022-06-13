using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Dusts
{
    public class BlockDust:ModDust
    {
        public const int SIDE_LENGTH = 25;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;

            int x = (int)(Math.Round(dust.position.X / SIDE_LENGTH) * SIDE_LENGTH);
            int y = (int)(Math.Round(dust.position.Y / SIDE_LENGTH) * SIDE_LENGTH);
            dust.position = new Vector2(x, y);
            dust.velocity = Vector2.Zero;
            dust.scale = 1.25f;
        }

        public override bool Update(Dust dust)
        {
            dust.scale -= 0.02f;
            if (dust.scale < 0.1f)
                dust.active = false;

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.Transparent;
        }

        public static void drawAll(SpriteBatch spriteBatch) {
            int requiredType = ModContent.DustType<BlockDust>();
            Texture2D tex = ModContent.Request<Texture2D>("PhysicsBoss/Dusts/BlockDust").Value;
            

            foreach (Dust d in Main.dust) {

                if (d.type == requiredType && d.active) {
                    spriteBatch.Draw(tex, d.position - Main.screenPosition, null,
                        Color.White, 0, tex.Size()/2, d.scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}
