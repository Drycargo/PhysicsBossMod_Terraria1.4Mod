sampler uImage0 : register(s0);
float4 shineColor;
float threashold;

float timer;
float2 texSize;

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

technique Technique1
{
    pass Beam
    {
        PixelShader = compile ps_2_0 BeamShine();
    }

    pass Blur
    {
        PixelShader = compile ps_2_0 Blur();
    }
}
