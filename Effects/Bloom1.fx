sampler uImage0 : register(s0);

float2 uScreenResolution;
float m;
float n;
float uIntensity;
float uRange;
bool hell;
float gauss[9] = { 0.005,0.02, 0.12, 0.26, 0.4, 0.26, 0.12, 0.02,0.005};//瞎写的

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0//提取亮度超过m的部分
{
    float4 c = tex2D(uImage0,coords);
    if (hell && (c.r*0.3+c.g*0.6+c.b*0.1)>m)
        return c;
        
        if (max(c.r, max(c.g, c.b)) > m && (c.r + c.g + c.b) < n)
            return c;
        else
            return float4(0, 0, 0, 0);
}
float4 GlurH(float2 coords : TEXCOORD0) : COLOR0//水平方向模糊
{
    float4 color = float4(0, 0, 0, 1);
    float dx = uRange / uScreenResolution.x;
    color = float4(0, 0, 0, 1);
    for (int i = -4; i <= 4; i++)
    {
        color.rgb += gauss[i + 4] * tex2D(uImage0, float2(coords.x + dx * i, coords.y)).rgb * uIntensity;
    }
    return color;
}
float4 GlurV(float2 coords : TEXCOORD0) : COLOR0//竖直方向模糊
{
    float4 color = float4(0, 0, 0, 1);
    float dy = uRange / uScreenResolution.y;
    for (int i = -4; i <= 4; i++)
    {
        color.rgb += gauss[i + 4] * tex2D(uImage0, float2(coords.x, coords.y + dy * i)).rgb * uIntensity;
    }
    return color;
}
technique Technique1
{
	pass Bloom
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
    pass GlurH
    {
        PixelShader = compile ps_2_0 GlurH();
    }
    pass GlurV
    {
        PixelShader = compile ps_2_0 GlurV();
    }
}