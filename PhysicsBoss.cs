using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria;
using PhysicsBoss.Effects;

namespace PhysicsBoss
{
	public class PhysicsBoss : Mod
	{
        public static Effect trailingEffect;
        public static Effect shineEffect;
        public static Effect worldEffect;

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

            worldEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/WorldM").Value;
            Filters.Scene["PhysicsBoss:Inverse"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Inverse"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Shake"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Shake"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Blur"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "GaussBlur"), EffectPriority.Medium);

            Filters.Scene["PhysicsBoss:Inverse"].Load();
            Filters.Scene["PhysicsBoss:Shake"].Load();
            Filters.Scene["PhysicsBoss:Blur"].Load();
            base.Load();
        }

        public override void PostSetupContent()
        {
            trailingEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Trailing").Value;
            shineEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Shine").Value;

            worldEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/WorldM").Value;
            Filters.Scene["PhysicsBoss:Inverse"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Inverse"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Shake"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Shake"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Blur"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "GaussBlur"), EffectPriority.Medium);

            Filters.Scene["PhysicsBoss:Inverse"].Load();
            Filters.Scene["PhysicsBoss:Shake"].Load();
            Filters.Scene["PhysicsBoss:Blur"].Load();
            base.PostSetupContent();
        }

        public override void Unload()
        {
            trailingEffect = null;
            shineEffect = null;
            worldEffect = null;
            base.Unload();
        }
    }

}