using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Dusts
{
    public class TwistedDust: ModDust
    {
        public static int count = 0;
        public override void OnSpawn(Dust dust)
        {
            TwistedDust.count++;
            dust.noGravity = true;
            dust.scale = 3f * Main.rand.NextFloat() + 2f;
            dust.noLight = true;
        }

        public override bool Update(Dust dust)
        {
            dust.scale -= 0.06f;
            dust.rotation += 0.025f * (dust.dustIndex % 2 == 0 ? 1f : -1f);
            if (dust.scale < 0.3f)
            {
                deActivate(dust);
            }

            dust.velocity *= 0.95f;
            return true;
        }

        private static void deActivate(Dust dust)
        {
            dust.active = false;
            if (TwistedDust.count > 0)
                TwistedDust.count--;

            ParticleOrchestraSettings settings = new ParticleOrchestraSettings
            {
                PositionInWorld = dust.position,
                MovementVector = Main.rand.NextVector2Unit() * 15f
            };

            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.PrincessWeapon, settings);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.Transparent;
        }

        public static void deActivateAll() {
            int requiredType = ModContent.DustType<TwistedDust>();

            foreach (Dust d in Main.dust)
            {

                if (d.type == requiredType && d.active)
                {
                    deActivate(d);
                }
            }

            count = 0;
        }

        public static void drawAll(SpriteBatch spriteBatch, int shrink)
        {
            int requiredType = ModContent.DustType<TwistedDust>();
            Texture2D tex = ModContent.Request<Texture2D>("PhysicsBoss/Dusts/TwistedDust").Value;

            bool nonExist = true;

            foreach (Dust d in Main.dust)
            {

                if (d.type == requiredType && d.active)
                {
                    spriteBatch.Draw(tex, (d.position - Main.screenPosition) / shrink, null,
                        Color.White, d.rotation, tex.Size() / 2, d.scale / (float)shrink, SpriteEffects.None, 0);
                    if (nonExist)
                        nonExist = false;
                }
            }

            if (nonExist)
                count = 0;
        }
    }
}
