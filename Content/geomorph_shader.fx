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

struct VertexShaderInput
{
	float3 StartPosition : POSITION0;
	float3 EndPosition : POSITION1;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float3 avgPos = lerp(input.StartPosition, input.EndPosition, Progress);
	output.Position = mul(float4(avgPos, 1.0), WorldViewProjection);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return float4(1.0, 1.0, 1.0, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};