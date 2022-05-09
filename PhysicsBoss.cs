using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria;
using PhysicsBoss.Effects;
using Microsoft.Xna.Framework;
using System;

namespace PhysicsBoss
{
	public class PhysicsBoss : Mod
	{
        public static RenderTarget2D tempRender;

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
            loadEffects();

            base.Load();
        }

        // same contents as load
        public override void PostSetupContent()
        {
            loadEffects();
            base.PostSetupContent();
        }

        private void loadEffects()
        {
            // instance effects
            trailingEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Trailing").Value;
            shineEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Shine").Value;

            // world effects
            worldEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/WorldM").Value;
            Filters.Scene["PhysicsBoss:Inverse"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Inverse"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Shake"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Shake"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Blur"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "GaussBlur"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Bloom"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "BlurOnThreshold"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:BlurH"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "BlurThresholdH"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:BlurV"] = new Filter(
                 new ScreenShaderApplier(new Ref<Effect>(worldEffect), "BlurThresholdV"), EffectPriority.Medium);

            Filters.Scene["PhysicsBoss:Inverse"].Load();
            Filters.Scene["PhysicsBoss:Shake"].Load();
            Filters.Scene["PhysicsBoss:Blur"].Load();
            Filters.Scene["PhysicsBoss:Bloom"].Load();
            Filters.Scene["PhysicsBoss:BlurH"].Load();
            Filters.Scene["PhysicsBoss:BlurV"].Load();

            // render target
            On.Terraria.Graphics.Effects.FilterManager.EndCapture += FilterManager_EndCapture;
            Main.OnResolutionChanged += Main_OnResolutionChanged;
        }

        private void Main_OnResolutionChanged(Vector2 obj)
        {
            createRenderTarget();
        }

        private void createRenderTarget()
        {
            tempRender = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }

        private void FilterManager_EndCapture(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }

        public override void Unload()
        {
            trailingEffect = null;
            shineEffect = null;
            worldEffect = null;

            On.Terraria.Graphics.Effects.FilterManager.EndCapture -= FilterManager_EndCapture;
            Main.OnResolutionChanged -= Main_OnResolutionChanged;

            base.Unload();
        }
    }

}