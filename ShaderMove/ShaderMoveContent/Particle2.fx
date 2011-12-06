float4x4 WorldViewProjection;
Texture theTexture;
sampler ColoredTextureSampler = sampler_state
{
texture = <theTexture> ;
magfilter = LINEAR;
minfilter = LINEAR;
mipfilter= LINEAR;
AddressU = Clamp;
AddressV = Clamp;
};
struct VertexShaderInput
{
float4 Position : POSITION0;
float2 textureCoordinates : TEXCOORD0;
};
struct VertexShaderOutput
{
float4 Position : POSITION0;
float2 textureCoordinates : TEXCOORD0;
};
struct PixelShaderInput
{
float2 textureCoordinates : TEXCOORD0;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
VertexShaderOutput output;
output.Position = mul(input.Position, WorldViewProjection);
output.textureCoordinates = input.textureCoordinates;
return output;
}
float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
return tex2D( ColoredTextureSampler, input.textureCoordinates);
}
technique Technique1
{
pass Pass1
{
	VertexShader = compile vs_2_0 VertexShaderFunction( );
	PixelShader = compile ps_2_0 PixelShaderFunction( );
}
}