#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float Progress;
float3 Albedo;

struct VertexShaderInput
{
	float3 Position : POSITION0;
	float3 Normal: NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : TEXCOORD0;
	float2 TexCoord: TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(float4(input.Position, 1.0), WorldViewProjection);
	output.Normal = normalize(input.Normal);
	output.TexCoord = output.Position.xy;

	return output;
}

float DitherPattern(float2 coord)
{
	return frac(sin(dot(coord, float2(12.9898,78.233))) * 43758.5453);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 lightDirection = normalize(float3(0, 1.0, 0));

	float dither = DitherPattern(input.TexCoord);
	clip(dither - Progress);

	float lightIntensity = max(dot(input.Normal, lightDirection), 0.0);

	return float4(Albedo * lightIntensity, 1.0);

}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};