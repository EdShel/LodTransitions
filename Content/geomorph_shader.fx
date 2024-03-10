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
	float3 StartNormal : NORMAL0;
	float3 EndPosition : POSITION1;
	float3 EndNormal : NORMAL1;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float3 avgPos = lerp(input.StartPosition, input.EndPosition, Progress);
	float3 normal = lerp(input.StartNormal, input.EndNormal, Progress);
	output.Position = mul(float4(avgPos, 1.0), WorldViewProjection);
	output.Normal = normalize(normal);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 lightDirection = normalize(float3(0, 1.0, 0));
	float lightIntensity = max(dot(input.Normal, lightDirection), 0.0);
	
	return float4(float3(1.0, 1.0, 1.0) * lightIntensity, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};