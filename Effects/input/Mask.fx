sampler uImage0 : register(s0);
texture2D texMask;
texture2D texContent;
texture2D texColorMap;
float2 texSize;
float timer;
float threshold;

float4 ordinaryTint;
float4 contentTint;

float polarizeShrink;
float dispTimer;

float phaseTimer1;
float phaseTimer2;
float darkThreshold;
float brightThreshold;

float range;
float transparency;

float4 tint;
float fadeThreshold;

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

sampler2D uColorMap = sampler_state
{
    Texture = <texColorMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

float dynamicComponent(float c, float dev)
{
    c += dev;
    c %= 2;
    if (c > 1)
        c = 2 - c;
    return c;
}

float4 dynamicColor(float4 rawC, float dev)
{
    rawC.r = dynamicComponent(rawC.r, dev);
    rawC.g = dynamicComponent(rawC.g, dev);
    rawC.b = dynamicComponent(rawC.b, dev);
    return rawC;
}

float4 DynamicComponentRange(float c, float dev, float r)
{
    float realD = dev % (4 * r);
    if (realD > 2* r)
        realD = 4 * r - realD;
    realD -= r;
    c += realD;
    
    if (c < 0)
        c *= -1;
    else if (c > 1)
        c = 2 - c;
    return c;
}

float4 DynamicColorRange(float2 coords : TEXCOORD0) : COLOR0
{

    float4 rawC = tex2D(uImage0, coords);
    if (range == 0)
        return rawC;

    rawC.r = DynamicComponentRange(rawC.r, timer, range);
    rawC.g = DynamicComponentRange(rawC.g, timer, range);
    rawC.b = DynamicComponentRange(rawC.b, timer, range);
    rawC.a = min(transparency, 1);
    
    return rawC;
}

float4 DynamicColor(float2 coords : TEXCOORD0) : COLOR0
{

    float4 rawC = tex2D(uImage0, coords);

    float4 c = dynamicColor(rawC, timer);
    c.a = min(rawC.a, 0.299 * c.r + 0.587 * c.g + 0.114 * c.b);
    
    return c;
}

float4 DynamicColorTint(float2 coords : TEXCOORD0) : COLOR0
{

    float4 rawC = tex2D(uImage0, coords);

    float4 c = dynamicColor(rawC, timer);
    c.a = min(rawC.a, 0.299 * c.r + 0.587 * c.g + 0.114 * c.b);
    
    return c * tint;
}

float4 DynamicColorTintHFade(float2 coords : TEXCOORD0) : COLOR0
{
    float4 c = DynamicColorTint(coords);
    float factor = 0.5 - abs(0.5 - coords.x);
    if (factor < fadeThreshold)
        return factor / fadeThreshold * c;
    
    return c;
}

float4 DynamicColorTintVFade(float2 coords : TEXCOORD0) : COLOR0
{
    float4 c = DynamicColorTint(coords);
    float factor = 0.5 - abs(0.5 - coords.y);
    if (factor < fadeThreshold)
        return factor / fadeThreshold * c;
    
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
    rawC = dynamicColor(rawC, timer);

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
    rawC = dynamicColor(rawC, timer);

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


float4 Polarize(float2 coords : TEXCOORD0) : COLOR0
{
    float realX = (coords.x - 0.5) * texSize.x;
    float realY = (coords.y - 0.5) * texSize.y;
    
    float angle = atan(realY / realX);
    if (realX < 0)
        angle += 3.14159265359;
    if (angle < 0)
        angle += 6.28318530718;
    
    return tex2D(uImage0, float2(angle / 6.28318530718, 
        polarizeShrink * length(float2(texSize.x / texSize.y * (coords.x - 0.5), coords.y - 0.5))));
}

float4 DynamicPolarize(float2 coords : TEXCOORD0) : COLOR0
{
    float realX = (coords.x - 0.5) * texSize.x;
    float realY = (coords.y - 0.5) * texSize.y;
    
    float angle = atan(realY / realX);
    if (realX < 0)
        angle += 3.14159265359;
    if (angle < 0)
        angle += 6.28318530718;
    
    float len = length(float2(texSize.x / texSize.y * (coords.x - 0.5), coords.y - 0.5));
    
    float4 c =  tex2D(uImage0, float2(angle / 6.28318530718,
        polarizeShrink * len / (len+1) + dispTimer));
    
    return c;
}

float4 DynamicColorMap(float2 coords : TEXCOORD0) : COLOR0
{
    float4 rawC = tex2D(uImage0, coords);
    rawC = dynamicColor(rawC, phaseTimer1);
    
    rawC.rgb = (rawC.rgb - float3(darkThreshold, darkThreshold, darkThreshold)) / (brightThreshold - darkThreshold);

    
    float4 tint = dynamicColor(tex2D(uColorMap, coords), phaseTimer2);
    
    return rawC * tint;
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

    pass Polarize
    {
        PixelShader = compile ps_2_0 Polarize();
    }

    pass DynamicPolarize
    {
        PixelShader = compile ps_2_0 DynamicPolarize();
    }

    pass DynamicColorMap
    {
        PixelShader = compile ps_2_0 DynamicColorMap();
    }

    pass DynamicColorRange
    {
        PixelShader = compile ps_2_0 DynamicColorRange();
    }

    pass DynamicColor
    {
        PixelShader = compile ps_2_0 DynamicColor();
    }

    pass DynamicColorTint
    {
        PixelShader = compile ps_2_0 DynamicColorTint();
    }


    pass DynamicColorTintHFade
    {
        PixelShader = compile ps_2_0 DynamicColorTintHFade();
    }

    pass DynamicColorTintVFade
    {
        PixelShader = compile ps_2_0 DynamicColorTintVFade();
    }
}
