sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 shineColor;
float threashold;

float timer;
float2 texSize;

texture2D tex0;

sampler2D uIm1 = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

float gauss[3][3] =
{
    0.075, 0.124, 0.075,
    0.124, 0.204, 0.124,
    0.075, 0.124, 0.075
};


float4 Blur(float2 coords : TEXCOORD0) : COLOR0
{
    float dx = 1 / texSize.x;
    float dy = 1 / texSize.y;
    float4 color = float4(0, 0, 0, 0);
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            color += gauss[i + 1][j + 1] * tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
        }
    }
    return 5 * timer * color + tex2D(uImage0, coords);

}

float4 BeamShine(float2 coords : TEXCOORD0) : COLOR0
{
    float4 origColor = tex2D(uImage0, coords);
    if (!any(origColor))
        return origColor;
    
    float brightness = origColor.r;
    
    if (brightness >= threashold)
        return float4(1, 1, 1, 1);
    
    return 0.5*brightness * float4(1, 1, 1, 1) + (1 - 0.5*brightness) * shineColor;
}

float4 DynamicBeam(float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = BeamShine(float2(coords.x + timer / texSize.x, coords.y));
    baseColor.a = (0.2 + 0.8 * tex2D(uIm1, coords).r);

    return baseColor;
}

float4 ColorGradient()
{
    return tex2D(uIm1, float2(timer, 0));
}

float4 DynamicColorTail(float2 coords : TEXCOORD0) : COLOR0
{
    float4 c = ColorGradient();
    float4 origC = tex2D(uImage0, coords);
    c.a = (origC.r * 0.7 + 0.3) * (1.0 - coords.x);

    return c;
}

float4 DynamicContourShine(float2 coords : TEXCOORD0) : COLOR0
{
    float4 origColor = tex2D(uImage0, coords);
    if (origColor.a != 0)
        return origColor;
    
    float stepX = 1.0 / texSize.x;
    float stepY = 1.0 / texSize.y;
   
    
    float4 c = ColorGradient();
    
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            if (i != 0 && j != 0)
            {
                float4 neighbourC = tex2D(uImage0, coords + float2(stepX * (float) i, stepY * (float) j));
                if (any(neighbourC))
                    return c;
            }
        }
    }

    return origColor;
}

float4 ContourShine(float2 coords : TEXCOORD0) : COLOR0
{
    float4 origColor = tex2D(uImage0, coords);
    if (origColor.a != 0)
        return origColor;
    
    float stepX = 1.0 / texSize.x;
    float stepY = 1.0 / texSize.y;
    float4 c = shineColor;
    c.a = 0.5;
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            if (i != 0 && j != 0)
            {
                float4 neighbourC = tex2D(uImage0, coords + float2(stepX * (float) i, stepY * (float) j));
                if (any(neighbourC))
                    return c;
            }
        }
    }

    return origColor;
}


technique Technique1
{
    pass Beam
    {
        PixelShader = compile ps_2_0 BeamShine();
    }

    pass DynamicBeam
    {
        PixelShader = compile ps_2_0 DynamicBeam();
    }

    pass Blur
    {
        PixelShader = compile ps_2_0 Blur();
    }

    pass Contour
    {
        PixelShader = compile ps_2_0 ContourShine();
    }

    pass DynamicContour
    {
        PixelShader = compile ps_2_0 DynamicContourShine();
    }

    pass DynamicColorTail
    {
        PixelShader = compile ps_2_0 DynamicColorTail();
    }
}
