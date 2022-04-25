sampler uImage0 : register(s0); // original texture
sampler uImage1 : register(s1); // luminance
sampler uImage2 : register(s2); // tint

float4x4 uTransform;
float uTime;
float intensity;
texture2D tex0;

sampler2D uIm1 = sampler_state
{
    Texture = <tex0>;
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
}