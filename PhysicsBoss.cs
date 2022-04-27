using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace PhysicsBoss
{
	public class PhysicsBoss : Mod
	{
        public static Effect trailingEffect;
        public static Effect shineEffect;

        private static PhysicsBoss instance;

        public PhysicsBoss()
        {
            instance = this;
        }

        public static PhysicsBoss Instance
        {
            get;
        }
        public override void Load()
        {
            trailingEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Trailing").Value;
            shineEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Shine").Value;
            base.Load();
        }

        public override void PostSetupContent()
        {
            trailingEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Trailing").Value;
            shineEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Shine").Value;
            base.PostSetupContent();
        }

        public override void Unload()
        {
            trailingEffect = null;
            shineEffect = null;
            base.Unload();
        }
    }

}