// Reference: https://github.com/LasseWestmark/Sparkle-Shader-Unity

float _NoiseScale;
float _SparkleDepth;

float Sparkles(float3 viewDir, float3 wPos)
{
	float noiseScale = _NoiseScale * 10;
	float sparkles = snoise(wPos * noiseScale * viewDir * _SparkleDepth) * snoise(wPos * noiseScale);
	sparkles = smoothstep(0.5, 0.6, sparkles);
	return sparkles;
}