namespace PhysicsBoss.Skies
{
    public class HindSky: CustomSky
    {
        public const int TRANSIT = 60;
        public const int PERIOD = 60;
        public const int DEBRIS_COUNT = 50;
        public const int DEBRIS_STANDARD_SCALE = 20;
        public const float DEBRIS_STANDARD_VEL = (1/7f) / 60f;
        public const float DEBRIS_ROT_VEL = MathHelper.PiOver2 / (2f * 60);

        private Texture2D gradientTex = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Gradient").Value;
        private Texture2D debrisTex = ModContent.Request<Texture2D>("PhysicsBoss/Skies/Debris").Value;
        

        private int bossIndex = -1;
        private float progress = 0f;
        private float timer = 0;
        private bool active = false;
        private Debris[] debris;

        private static Color currentColor = Color.Black;
        private static Color frontCover = Color.Transparent;

        private static bool drawDebris = false;
        private static float debrisProgress = 0;

        public static void setColor(Color backC, Color frontC)
        {
            currentColor = backC;
            frontCover = frontC;
        }

        public static void activateDebris()
        {
            drawDebris = true;
        }

        public static void deActivateDebris()
        {
            drawDebris = false;
        }

        public override void OnLoad()
        {
            currentColor = Color.Black;
            frontCover = Color.Transparent;
            drawDebris = false;

            debrisProgress = 0;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
            bossIndex = (int)args[0];
            progress = 0;

            debris = new Debris[DEBRIS_COUNT];
            for (int i = 0; i < DEBRIS_COUNT; i++)
            {
                debris[i].id = Main.rand.Next() % 3;
                debris[i].center = new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
                debris[i].speed = DEBRIS_STANDARD_VEL * (1f + 1.6f * (Main.rand.NextFloat() - 0.5f));
                debris[i].scale = DEBRIS_STANDARD_SCALE * (debris[i].speed / DEBRIS_STANDARD_VEL) * Vector2.One;
                debris[i].origin = debrisTex.Width * (0.5f + 0.2f * (Main.rand.NextFloat() - 0.5f)) * Vector2.One;
                debris[i].rot = Main.rand.NextFloat() * MathHelper.TwoPi;
            }
        }

        public override void Deactivate(params object[] args)
        {
            quickDeAct();
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= float.MaxValue && minDepth < float.MaxValue && IsActive())
            {
                Rectangle dest = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                spriteBatch.Draw(gradientTex, dest,
                    currentColor * progress);

                if (debrisProgress > 0)
                {
                    foreach (Debris d in debris)
                    {
                        drawSingleDebris(d, spriteBatch);
                    }
                }
            }
            else if (maxDepth >= 0 && minDepth < 0 && IsActive()) {
                Rectangle dest = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                spriteBatch.Draw(TextureAssets.BlackTile.Value, dest,
                    frontCover * progress);
            }
        }

        private void drawSingleDebris(Debris d, SpriteBatch spriteBatch)
        {
            Vector2 actualPos = d.center * Main.ScreenSize.ToVector2();
            Color c = Color.White * debrisProgress * (d.speed / DEBRIS_STANDARD_VEL) * progress;

            spriteBatch.Draw(debrisTex,
                new Rectangle((int)(actualPos.X - d.scale.X * 0.5f),
                (int)(actualPos.Y - d.scale.Y * 0.5f), (int)d.scale.X, (int)d.scale.Y),
                new Rectangle(0,d.id * debrisTex.Height/3, debrisTex.Width, debrisTex.Height / 3),
                c, d.rot, d.origin, SpriteEffects.None, 0);

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
            active = false;
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


            // update debris
            if (drawDebris || debrisProgress > 0)
            {
                for (int i = 0; i < DEBRIS_COUNT; i++)
                {
                    debris[i].center.Y -= debris[i].speed;
                    if (debris[i].center.Y < 0)
                    {
                        debris[i].center.X = Main.rand.NextFloat();
                        debris[i].center.Y = 1;
                    }
                    else if (debris[i].center.Y > 1)
                    {
                        debris[i].center.X = Main.rand.NextFloat();
                        debris[i].center.Y = 0;
                    }

                    debris[i].rot += (i % 2 == 0 ? -1 : 1) * DEBRIS_ROT_VEL;
                }
            }

            if (debrisProgress < 1 && drawDebris)
                debrisProgress += 1f / TRANSIT;
            else if (!drawDebris && debrisProgress > 0)
                debrisProgress -= 1f / TRANSIT;

            timer++;
            timer %= 1000000;
        }

        public override float GetCloudAlpha()
        {
            return MathHelper.Lerp(base.GetCloudAlpha(), 0.5f, progress);
        }

        public override Color OnTileColor(Color inColor)
        {
            return Color.Lerp(inColor, currentColor, 0.8f + 0.1f * (Main.rand.NextFloat() - 0.5f));
        }

        private bool bossInactive()
        {
            //return false;

            return bossIndex >= 0 && (!Main.npc[bossIndex].active || Main.npc[bossIndex].life <= 0
                                || Main.npc[bossIndex].type != ModContent.NPCType<ChaosTheory>());

        }

        private struct Debris
        {
            public int id;
            public Vector2 center;
            public float speed;
            public Vector2 scale;
            public Vector2 origin;
            public float rot;
        }
    }
}
