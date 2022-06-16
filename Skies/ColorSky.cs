using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Skies
{
    public class ColorSky : CustomSky
    {
        public const int TRANSIT = 60;
        public const int PERIOD = 30;
        public const int STAR_COUNT = 25;
        public const float STAR_STANDARD_VEL = (1f / 1.5f)/60f;

        private Texture2D beamTex = ModContent.Request<Texture2D>("PhysicsBoss/Asset/BeamHorizontal").Value;
        private Texture2D wireTex = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Wire").Value;
        private Texture2D blockTex = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Blocks").Value;

        private int bossIndex = -1;
        private float progress = 0f;
        private float timer = 0;
        private bool active = false;
        private Star[] stars;
        
        private static Color currentColor = Color.Black;
        private static Color colorToSet = Color.Black;
        private static float colorProgress = 1;

        private static bool drawWire = false;
        private static Color wireColor = Color.White;
        private static float wireProgress = 0;
        private static bool drawBlock = false;
        private static Color blockColor = Color.White;
        private static float blockProgress = 0;

        private static bool drawStars = false;
        private static float starProgress = 0;
        private static bool starRotate = false;
        private static bool starReverse = false;

        public static void setColor(Color c, float prog = 0) {
            colorToSet = c;
            colorProgress = prog;
        }

        public static void activateDrawWire(Color c) {
            drawWire = true;
            wireColor = c;
        }

        public static void deActivateDrawWire()
        {
            wireColor = Color.Black;
            drawWire = false;
        }

        public static void activateDrawBlock(Color c)
        {
            drawBlock = true;
            blockColor = c;
        }

        public static void deActivateDrawBlock()
        {
            drawBlock = false;
            blockColor = Color.Black;
        }

        public static void activateStars() {
            drawStars = true;
        }

        public static void deActivateStars()
        {
            drawStars = false;
        }

        public static void setStarMotion(bool reverse, bool rotate) {
            starRotate = rotate;
            starReverse = reverse;
        }

        public override void OnLoad()
        {
            currentColor = Color.Black;
            colorToSet = Color.Black;
            colorProgress = 1;

            drawWire = false;
            wireColor = Color.White;
            wireProgress = 0;
            drawBlock = false;
            blockColor = Color.White;
            blockProgress = 0;

            drawStars = false;
            starRotate = false;
            starReverse = false;
            starProgress = 0;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
            bossIndex = (int)args[0];
            colorProgress = 1;
            progress = 0;

            drawWire = false;
            wireProgress = 0;
            drawBlock = false;
            blockProgress = 0;

            stars = new Star[STAR_COUNT];
            for (int i = 0; i < STAR_COUNT; i++) {
                stars[i].center = new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
                stars[i].speed = STAR_STANDARD_VEL * (1f + 0.2f * (Main.rand.NextFloat() - 0.5f));
                stars[i].scale = (stars[i].speed / STAR_STANDARD_VEL) * new Vector2(75,3);
            }
        }

        public override void Deactivate(params object[] args)
        {
            quickDeAct();
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0 && IsActive())
            {
                Rectangle dest = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                spriteBatch.Draw(beamTex, dest,
                    currentColor * progress);

                if (wireProgress > 0) {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    PhysicsBoss.maskEffect.Parameters["timer"].SetValue(timer * 0.015f);
                    PhysicsBoss.maskEffect.Parameters["tint"].SetValue(wireColor.ToVector4() * wireProgress);
                    PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorTint"].Apply();

                    spriteBatch.Draw(wireTex, dest, Color.White);
                }

                if (blockProgress > 0)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    PhysicsBoss.maskEffect.Parameters["timer"].SetValue(timer * 0.0075f);
                    PhysicsBoss.maskEffect.Parameters["tint"].SetValue(blockColor.ToVector4() * blockProgress);
                    PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorTint"].Apply();

                    spriteBatch.Draw(blockTex, dest, Color.White);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                if (starProgress > 0)
                {
                    foreach (Star star in stars)
                    {
                        drawStar(star, spriteBatch);
                    }
                }
            }
        }

        private void drawStar(Star star, SpriteBatch spriteBatch)
        {
            Vector2 actualPos = star.center * Main.ScreenSize.ToVector2();
            Color c = Color.White * starProgress * (star.speed / STAR_STANDARD_VEL) * 0.8f;
            if (starRotate)
            {
                spriteBatch.Draw(TextureAssets.BlackTile.Value,
                    new Rectangle((int)(actualPos.Y - star.scale.Y * 0.5f),
                    (int)(actualPos.X - star.scale.X * 0.5f), (int)star.scale.Y, (int)star.scale.X),
                    c * Math.Abs(0.5f - star.center.X) * 1.5f);
            }
            else {
                spriteBatch.Draw(TextureAssets.BlackTile.Value,
                    new Rectangle((int)(actualPos.X - star.scale.X * 0.5f),
                    (int)(actualPos.Y - star.scale.Y * 0.5f), (int)star.scale.X, (int)star.scale.Y),
                    c * Math.Abs(0.5f - star.center.Y) * 1.5f);
            }
        }

        public override bool IsActive()
        {
            return (active && !bossInactive()) || progress > 0;
        }

        public override void Reset()
        {
            quickDeAct();
        }

        private void quickDeAct()
        {
            bossIndex = -1;
            colorProgress = 1;
            active = false;
            drawBlock = false;
            drawWire = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (active)
            {
                if (bossInactive())
                {
                    Deactivate();
                }
            }

            if (active && progress < 1f)
                progress += 1f / PERIOD;
            else if (!active && progress > 0)
                progress -= 0.05f;

            if (colorProgress < 1) {
                colorProgress += 1f / (float)TRANSIT;
                currentColor = Color.Lerp(currentColor, colorToSet, colorProgress);
            }

            if (drawBlock && blockProgress < 1)
            {
                blockProgress += 1f / TRANSIT;
            }
            else if (!drawBlock && blockProgress > 0) {
                blockProgress -= 1f / TRANSIT;
            }

            if (drawWire && wireProgress < 1)
            {
                wireProgress += 1f / TRANSIT;
            }
            else if (!drawWire && wireProgress > 0)
            {
                wireProgress -= 1f / TRANSIT;
            }

            // update stars
            if (drawStars || starProgress > 0)
            {
                for (int i = 0; i < STAR_COUNT; i++)
                {
                    stars[i].center.X -= (starReverse ? -1 : 1) * stars[i].speed;
                    if (stars[i].center.X < 0)
                    {
                        stars[i].center.Y = Main.rand.NextFloat();
                        stars[i].center.X = 1;
                    }
                    else if (stars[i].center.X > 1) {
                        stars[i].center.Y = Main.rand.NextFloat();
                        stars[i].center.X = 0;
                    }
                }
            }

            if (starProgress < 1 && drawStars)
                starProgress += 1f / TRANSIT;
            else if (!drawStars && starProgress > 0)
                starProgress -= 1f / TRANSIT;

            timer++;
            timer %= 1000000;
        }


        public override Color OnTileColor(Color inColor)
        {
            return Color.Lerp(inColor, currentColor, 0.5f + 0.1f * (Main.rand.NextFloat() - 0.5f));
        }

        private bool bossInactive()
        {
            //return false;
            
            return bossIndex >= 0 && (!Main.npc[bossIndex].active || Main.npc[bossIndex].life <= 0
                                || Main.npc[bossIndex].type != ModContent.NPCType<ChaosTheory>());
            
        }

        private struct Star {
            public Vector2 center;
            public float speed;
            public Vector2 scale;
        }
    }
}
