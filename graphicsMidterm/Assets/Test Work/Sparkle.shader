Shader "Unlit/Sparkle"
{
	Properties
	{
		[Header(Colors)]
	_Color("Color", Color) = (.5,.5,.5,1)
		_SpecColor("Specular Color", Color) = (.5,.5,.5,1)
		_MainTex("Texture", 2D) = "white" {}
	[Header(Specular)]
	_SpecPow("Specular Power", Range(1, 50)) = 24
		_GlitterPow("Glitter Power", Range(1, 50)) = 5
		[Header(Sparkles)]
	_SparkleDepth("Sparkle Depth", Range(0, 5)) = 1
		_NoiseScale("noise Scale", Range(0, 5)) = 1
		_AnimSpeed("Animation Speed", Range(0, 5)) = 1
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		Tags{ "LightMode" = "ForwardBase" }
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
		float3 wPos : TEXCOORD1;
		float3 pos : TEXCOORD3;
		float3 normal : NORMAL;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _Color, _SpecColor;
	float _SpecPow, _GlitterPow;

	float _NoiseScale;
	float _AnimSpeed;
	float _SparkleDepth;

	float3 mod289(float3 x)
	{

		return x - floor(x * (1.0 / 289.0)) * 289.0;

	}



	float4 mod289(float4 x)
	{

		return x - floor(x * (1.0 / 289.0)) * 289.0;

	}



	float4 permute(float4 x)
	{

		return mod289(((x*34.0) + 1.0)*x);

	}



	float4 taylorInvSqrt(float4 r)

	{

		return 1.79284291400159 - 0.85373472095314 * r;

	}


	float snoise(float3 v)

	{

		const float2  C = float2(1.0 / 6.0, 1.0 / 3.0);

		const float4  D = float4(0.0, 0.5, 1.0, 2.0);



		// First corner

		float3 i = floor(v + dot(v, C.yyy));

		float3 x0 = v - i + dot(i, C.xxx);



		// Other corners

		float3 g = step(x0.yzx, x0.xyz);

		float3 l = 1.0 - g;
		float3 i1 = min(g.xyz, l.zxy);

		float3 i2 = max(g.xyz, l.zxy);



		//   x0 = x0 - 0.0 + 0.0 * C.xxx;

		//   x1 = x0 - i1  + 1.0 * C.xxx;

		//   x2 = x0 - i2  + 2.0 * C.xxx;

		//   x3 = x0 - 1.0 + 3.0 * C.xxx;

		float3 x1 = x0 - i1 + C.xxx;

		float3 x2 = x0 - i2 + C.yyy;

		// 2.0*C.x = 1/3 = C.y
		float3 x3 = x0 - D.yyy;

		// -1.0+3.0*C.x = -0.5 = -D.y


		// Permutations
		i = mod289(i);

		float4 p = permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
			+ i.y + float4(0.0, i1.y, i2.y, 1.0))
			+ i.x + float4(0.0, i1.x, i2.x, 1.0));



		// Gradients: 7x7 points over a square, mapped onto an octahedron.

		// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)

		float n_ = 0.142857142857;
		// 1.0/7.0

		float3  ns = n_ * D.wyz - D.xzx;


		float4 j = p - 49.0 * floor(p * ns.z * ns.z);
		//  mod(p,7*7)


		float4 x_ = floor(j * ns.z);

		float4 y_ = floor(j - 7.0 * x_);
		// mod(j,N)


		float4 x = x_ * ns.x + ns.yyyy;

		float4 y = y_ * ns.x + ns.yyyy;

		float4 h = 1.0 - abs(x) - abs(y);


		float4 b0 = float4(x.xy, y.xy);

		float4 b1 = float4(x.zw, y.zw);


		//float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;

		//float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;

		float4 s0 = floor(b0)*2.0 + 1.0;

		float4 s1 = floor(b1)*2.0 + 1.0;

		float4 sh = -step(h, float4(0, 0, 0, 0));


		float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;

		float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;


		float3 p0 = float3(a0.xy, h.x);

		float3 p1 = float3(a0.zw, h.y);

		float3 p2 = float3(a1.xy, h.z);

		float3 p3 = float3(a1.zw, h.w);



		//Normalise gradients

		float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));

		p0 *= norm.x;

		p1 *= norm.y;

		p2 *= norm.z;

		p3 *= norm.w;



		// Mix final noise value

		float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);

		m = m * m;

		return 42.0 * dot(m*m, float4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));

	}

	float Sparkles(float3 viewDir, float3 wPos)
	{
		float noiseScale = _NoiseScale * 10;
		float sparkles = snoise(wPos * noiseScale + viewDir * _SparkleDepth - _Time.x * _AnimSpeed) * snoise(wPos * noiseScale + _Time.x * _AnimSpeed);
		sparkles = smoothstep(.5, .6, sparkles);
		return sparkles;
	}

	v2f vert(appdata v)
	{
		v2f o;

		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		o.pos = v.vertex;
		o.normal = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
		o.vertex = UnityObjectToClipPos(v.vertex);
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		//Light Calculation
		float3 normal = normalize(i.normal);
		float3 viewDir = normalize(UnityWorldSpaceViewDir(i.wPos));
		float3 reflDir = reflect(-viewDir, normal);
		float3 lightDirection;
		float atten = 1.0;
		lightDirection = normalize(_WorldSpaceLightPos0.xyz);
		float diffuse = max(0.0, dot(normal, lightDirection) * .5 + .5);
		float specular = saturate(dot(reflDir, lightDirection));
		float glitterSpecular = pow(specular,_GlitterPow);
		specular = pow(specular,_SpecPow);

		//Sparkles
		float sparkles = Sparkles(viewDir,i.wPos);

		//Sample the texture
		fixed4 col = tex2D(_MainTex, i.uv) * _Color * diffuse;
		//Apply Specular and sparkles
		col += _SpecColor * (saturate(sparkles * glitterSpecular * 5) + specular);
		//Apply fog
		UNITY_APPLY_FOG(i.fogCoord, col);

		return col;
	}
		ENDCG
	}
	}
}
