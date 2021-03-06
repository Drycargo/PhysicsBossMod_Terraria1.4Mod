sampler uImage0 : register(s0); // original texture
sampler uImage1 : register(s1); // luminance
sampler uImage2 : register(s2); // tint

float4x4 uTransform;
float uTime;
float intensity;
texture2D tex0;

texture2D tailColor;
float4 tailStart;
float4 tailEnd;

float4 outsideBlade;
float4 insideBlade;
float amplitude;

sampler2D uIm1 = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

sampler2D uColor = sampler_state
{
    Texture = <tailColor>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

struct VSInput{
    float2 Pos : POSITION0;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};

struct PSInput{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
    float3 myPos : TEXCOORD1;
};

float4 DefaultFn(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, coords);
}

float4 Displacement(float2 coords: TEXCOORD0) : COLOR0
{
    float4 originalC = tex2D(uImage0, coords);
    float4 displaceC = tex2D(uIm1, coords);
    
    float4 temp = displaceC; //float4(displaceC.rgb, 0);
    if (!any(temp))
        return originalC;

    //return float4(1,1,1,0);

    float rot = displaceC.r * 6.28;
    float2 disVec = float2(cos(rot), sin(rot)) * displaceC.g * intensity;

    return tex2D(uImage0, coords + disVec);
}

float4 BladeTrail(float2 coords : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(uImage0, coords);
    float transparency = min(c.a, c.r) * coords.y * (1 - coords.x) * amplitude;

    if (coords.y < 0.4)
        return transparency * outsideBlade;
    if (coords.y > 0.6)
        return transparency * insideBlade;
    
    return transparency * lerp(outsideBlade, insideBlade, (coords.y - 0.4) / 0.2);
}

float4 DynamicTrailSimple(float2 coords : TEXCOORD0) : COLOR0
{
    float4 origC = tex2D(uImage0, float2(coords.x + uTime, coords.y + uTime));
    
    float transparency = sqrt(2 * (0.5 - abs(0.5 - coords.y))) * min(origC.a, origC.r);
    
    return origC * transparency * lerp(tailStart, tailEnd, coords.x);
}

float4 DynamicTrailSimpleX(float2 coords : TEXCOORD0) : COLOR0
{
    float4 origC = tex2D(uImage0, float2(coords.x + uTime, coords.y));
    
    float transparency = sqrt(2 * (0.5 - abs(0.5 - coords.y))) * min(origC.a, origC.r);
    
    return origC * transparency * lerp(tailStart, tailEnd, coords.x);
}

float4 DynamicContentX(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, float2(coords.x + uTime, coords.y));
}

float4 DynamicTrailSimpleFade(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = DynamicTrailSimple(coords);
    color.a *= sqrt(1 - coords.x);
    return color;

}

float4 StaticTrail(PSInput input) : COLOR0
{
    float3 coord = input.Texcoord;
    float4 lumColor = tex2D(uImage1, coord.xy);
    lumColor.a *= lumColor.r;
    float4 tintColor = tex2D(uImage2, float2(coord.x, coord.y + uTime * 0.03));
    tintColor *= lumColor;
    
    return tintColor * 1.25;
}

float4 DynamicTrail(PSInput input) : COLOR0
{
    float3 coord = input.Texcoord;
    float4 lumColor = tex2D(uImage1, coord.xy);
    lumColor.a *= lumColor.r;
    float4 tintColor = tex2D(uImage2, float2(coord.x+ uTime, coord.y));
    tintColor *= lumColor;
    
    float4 actualColor = tex2D(uImage0, float2(coord.x + uTime*0.03, coord.y));
    
    return actualColor * tintColor*3;
}

PSInput VertexFn(VSInput input) {
    PSInput output;
    output.Color = input.Color;
    output.Texcoord = input.Texcoord;
    output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    output.myPos = output.Pos.xyz;
    return output;
}

technique Technique1 {
    pass Trail {
        VertexShader = compile vs_2_0 VertexFn();
        PixelShader = compile ps_2_0 DynamicTrail();
    }

    pass DynamicTrailSimple
    {
        PixelShader = compile ps_2_0 DynamicTrailSimple();
    }

    pass DynamicTrailSimpleX
    {
        PixelShader = compile ps_2_0 DynamicTrailSimpleX();
    }

    pass DynamicTrailSimpleFade
    {
        PixelShader = compile ps_2_0 DynamicTrailSimpleFade();
    }

    pass StaticTrail
    {
        VertexShader = compile vs_2_0 VertexFn();
        PixelShader = compile ps_2_0 StaticTrail();
    }

    Pass Displacement
    {
        PixelShader = compile ps_2_0 Displacement();
    }

    Pass Default
    {
        PixelShader = compile ps_2_0 DefaultFn();

    }

    Pass DefaultTrail
    {
        VertexShader = compile vs_2_0 VertexFn();
        PixelShader = compile ps_2_0 DefaultFn();
    }

    Pass BladeTrail
    {
        PixelShader = compile ps_2_0 BladeTrail();
    }

    Pass DynamicContentX
    {
        PixelShader = compile ps_2_0 DynamicContentX();
    }
}