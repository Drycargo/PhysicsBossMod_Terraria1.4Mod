﻿sampler uImage0 : register(s0);
texture2D texMask;
texture2D texContent;
float2 texSize;
float timer;
float threshold;

float4 ordinaryTint;
float4 contentTint;

sampler2D uMask = sampler_state
{
    Texture = <texMask>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

sampler2D uContent = sampler_state
{
    Texture = <texContent>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

float dynamicColor(float c)
{
    float dev = timer;
    c += dev;
    c %= 2;
    if (c > 1)
        c = 1 - (c - 1);
    return c;
}

float4 DynamicMask(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uMask, coords);
    if (!any(color))
        return tex2D(uImage0, coords);
    if (color.r < 0.9 * threshold)
        return color + tex2D(uImage0, coords);

    float4 rawC = tex2D(uContent, coords);
    rawC.r = dynamicColor(rawC.r);
    rawC.g = dynamicColor(rawC.g);
    rawC.b = dynamicColor(rawC.b);

    return rawC * threshold + tex2D(uImage0, coords) * (1 - threshold);
}

float4 DynamicMaskTint(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return tex2D(uImage0, coords);
    if (color.r < threshold || color.a < threshold)
        return color * ordinaryTint;

    float4 rawC = tex2D(uContent, coords);
    rawC.r = dynamicColor(rawC.r);
    rawC.g = dynamicColor(rawC.g);
    rawC.b = dynamicColor(rawC.b);

    return rawC * contentTint;
}

float4 Mask(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uMask, coords);
    if (!any(color))
        return tex2D(uImage0, coords);
    if (color.r < 0.9 * threshold)
        return color + tex2D(uImage0, coords);

    return tex2D(uContent, coords) * threshold + tex2D(uImage0, coords) * (1 - threshold);
}


technique Technique1
{
    pass StaticMask
    {
        PixelShader = compile ps_2_0 Mask();
    }

    pass DynamicMask
    {
        PixelShader = compile ps_2_0 DynamicMask();
    }

    pass DynamicMaskTint
    {
        PixelShader = compile ps_2_0 DynamicMaskTint();
    }
}
