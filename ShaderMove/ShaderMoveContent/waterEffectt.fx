float4x4 World;
float4x4 View;
float4x4 Projection;

float fx_Red;
float fx_Pos;
float fx_Alpha;

float WaveLength = 0.6;
float WaveHeight = 0.2;
float Time = 0;
float WaveSpeed = 0.04f;


sampler ColorTextureSampler : register(s0);

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
	float2 NormalMapPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	input.Position.x += fx_Pos;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
	output.TexCoord = input.TexCoord;

	output.NormalMapPosition = input.TexCoord/ WaveLength;
	output.NormalMapPosition.y -= Time * WaveSpeed;

    return output;
}

float4 SetColor(float4 inn) {
	float4 f4color;

	f4color.r = inn.r;
	f4color.g = inn.g;
	f4color.b = inn.b;
	f4color.a = fx_Alpha;

	return f4color;
}



float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = tex2D(ColorTextureSampler, input.TexCoord);
	//output * SetColor(input.Color);
	output.a = fx_Alpha;


	return output;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

		AlphaBlendEnable = true;
DestBlend = INVSRCALPHA;
SrcBlend = SRCALPHA;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
