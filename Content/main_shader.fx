#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

/* Parameters */

matrix WorldViewProjection;
float3 Albedo = float3(1, 1, 1);
float3 LightDirection = float3(0, 1, 0);
float Progress = 0;

/* Base 3D model rendering */

struct MainVertexShaderInput
{
	float3 Position : POSITION0;
	float3 Normal: NORMAL0;
};

struct MainVertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
};

MainVertexShaderOutput MainVS(in MainVertexShaderInput input)
{
	MainVertexShaderOutput output;

	output.Position = mul(float4(input.Position, 1.0), WorldViewProjection);
	output.Normal = normalize(input.Normal);

	return output;
}

float4 MainPS(MainVertexShaderOutput input) : COLOR
{
	float lightIntensity = max(dot(input.Normal, LightDirection), 0.0);

	return float4(Albedo * lightIntensity, 1.0);
}

/* Alpha blending shader */

float4 AlphaPS(MainVertexShaderOutput input) : COLOR
{
	// This program is supposed to write only to the Depth buffer, don't waste time on pixel color
	return float4(0, 0, 0, 1);
}

/* Noise blending shader */

struct NoiseVertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 UV: TEXCOORD0;
};


NoiseVertexShaderOutput NoiseVS(in MainVertexShaderInput input)
{
	NoiseVertexShaderOutput output;

	output.Position = mul(float4(input.Position, 1.0), WorldViewProjection);
	output.Normal = normalize(input.Normal);
	output.UV = output.Position.xy;

	return output;
}

float DitherPattern(float2 coord)
{
	return frac(sin(dot(coord, float2(12.9898, 78.233))) * 43758.5453);
}

float4 NoisePS(NoiseVertexShaderOutput input) : COLOR
{
	float dither = DitherPattern(input.UV);
	clip(dither - Progress);

	float lightIntensity = max(dot(input.Normal, LightDirection), 0.0);

	return float4(Albedo * lightIntensity, 1.0);

}

/* Geomorph shader */

struct GeomorphVertexShaderInput
{
	float3 StartPosition : POSITION0;
	float3 StartNormal : NORMAL0;
	float3 EndPosition : POSITION1;
	float3 EndNormal : NORMAL1;
};

MainVertexShaderOutput GeomorphVS(in GeomorphVertexShaderInput input)
{
	MainVertexShaderOutput output;

	float3 avgPos = lerp(input.StartPosition, input.EndPosition, Progress);
	float3 normal = lerp(input.StartNormal, input.EndNormal, Progress);
	output.Position = mul(float4(avgPos, 1.0), WorldViewProjection);
	output.Normal = normalize(normal);

	return output;
}

/* Passes */

technique BasicColorDrawing
{
	pass MainPass
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
	pass AlphaPass
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL AlphaPS();
	}
	pass NoisePass
	{
		VertexShader = compile VS_SHADERMODEL NoiseVS();
		PixelShader = compile PS_SHADERMODEL NoisePS();
	}
	pass GeomorphPass
	{
		VertexShader = compile VS_SHADERMODEL GeomorphVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};