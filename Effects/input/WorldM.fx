sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float uOpacity;
float3 uSecondaryColor;
float uTime;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageOffset;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uZoom;
float2 uImageSize0;
float2 uImageSize1;

float2 vibInten;

float4 Inverse(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    return float4(float3(1,1,1)-color.rgb, color.a);
}

float4 Shake(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, float2(coords.x + vibInten.x / uScreenResolution.x, coords.y + vibInten.y / uScreenResolution.y));
}

technique Technique1
{
    pass Inverse
    {
        PixelShader = compile ps_2_0 Inverse();
    }

    pass Shake
    {
        PixelShader = compile ps_2_0 Shake();
    }
}
