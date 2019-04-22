//This file was created by Claire Yeash and Zach Phillips
Shader "Unlit/SnowShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		// Noise variables
		//_Resolution ("Resolution", Range (2, 512)) = 256
		//_Frequency ("Frequency", Int) = 6
		//_Octaves ("Octaves", Range(1, 8)) = 8
		//_Lacunarity ("Lacunarity", Range(1.0, 4.0)) = 2.0
		//_Persistence ("Persistence", Range(0.0, 1.0)) = 0.5
		//_Dimensions ("Dimensions", Range(1, 3)) = 3
		//_NoiseMethodType ("Noise Type", )
		//_Gradient

		_BumpTex("Normalmap", 2D) = "bump" {}
	
		// Color variables
		_ColorBlue ("Blue", Color) = (0, 0, 1, .05)
		_ColorGreen ("Green", Color) = (0, 1, 0, .05)
		_ColorWhite ("White", Color) = (1, 1, 1, .05)

		[Header(Colors)]
		_Color("Color", Color) = (.5,.5,.5,1)
		_SpecColor("Specular Color", Color) = (.5,.5,.5,1)

		[Header(Specular)]
		_SpecPow("Specular Power", Range(1, 50)) = 24
		_GlitterPow("Glitter Power", Range(1, 50)) = 5

		[Header(Sparkles)]
		_SparkleDepth("Sparkle Depth", Range(0, 5)) = 1
		_NoiseScale("noise Scale", Range(0, 5)) = 1
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		LOD 300

		CGPROGRAM
		//#pragma surface surf Lambert alphatest:Zero
		#pragma surface surfaceFunction Lambert
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BumpTex;
		sampler2D _DissolveTex;
		float4 _Color;
		float4 _Tint;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_DissolveTex;
		};

		void surfaceFunction(Input IN, inout SurfaceOutput o)
		{
			half4 tex = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = tex.rgb * _Color.rgb * _Tint.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Alpha = _Color.a;

			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		}

		ENDCG

		Tags { "Queue"="Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ColorBlue;
			float4 _ColorGreen;
			float4 _ColorWhite;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 pixelCoord = i.uv;
				float2 pixelCoord1 = float2(pixelCoord.x - 1, pixelCoord.y + 1);
				float2 pixelCoord2 = float2(pixelCoord.x, pixelCoord.y + 1);
				float2 pixelCoord3 = float2(pixelCoord.x + 1, pixelCoord.y + 1);
				float2 pixelCoord4 = float2(pixelCoord.x - 1, pixelCoord.y);
				float2 pixelCoord5 = float2(pixelCoord.x + 1, pixelCoord.y);
				float2 pixelCoord6 = float2(pixelCoord.x - 1, pixelCoord.y - 1);
				float2 pixelCoord7 = float2(pixelCoord.x, pixelCoord.y - 1);
				float2 pixelCoord8 = float2(pixelCoord.x + 1, pixelCoord.y - 1);

				// sample the texture
				fixed4 col = tex2D(_MainTex, pixelCoord);
				fixed4 col1 = tex2D(_MainTex, pixelCoord1);
				fixed4 col2 = tex2D(_MainTex, pixelCoord2);
				fixed4 col3 = tex2D(_MainTex, pixelCoord3);
				fixed4 col4 = tex2D(_MainTex, pixelCoord4);
				fixed4 col5 = tex2D(_MainTex, pixelCoord5);
				fixed4 col6 = tex2D(_MainTex, pixelCoord6);
				fixed4 col7 = tex2D(_MainTex, pixelCoord7);
				fixed4 col8 = tex2D(_MainTex, pixelCoord8);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				// ****ICE COLOR LAYER****
				// thickness based on noise
				int thickness = 0;
				thickness = ((col.r + col.g + col.b) % 2);
				int thickness1 = 0;
				thickness1 = ((col1.r + col1.g + col1.b) % 3);
				int thickness2 = 0;
				thickness2 = ((col2.r + col2.g + col2.b) % 3);
				int thickness3 = 0;
				thickness3 = ((col3.r + col3.g + col3.b) % 3);
				int thickness4 = 0;
				thickness4 = ((col4.r + col4.g + col4.b) % 3);
				int thickness5 = 0;
				thickness5 = ((col5.r + col5.g + col5.b) % 3);
				int thickness6 = 0;
				thickness6 = ((col6.r + col6.g + col6.b) % 3);
				int thickness7 = 0;
				thickness7 = ((col7.r + col7.g + col7.b) % 3);
				int thickness8 = 0;
				thickness8 = ((col8.r + col8.g + col8.b) % 3);

				// color based on thickness
				int averageThickness = 0;
				averageThickness = (thickness2 + thickness4 + thickness7) / 4;
				float4 thicknessColor = float4(0, 0, 0, 0);
				switch (thickness)
				{
					case 1:
						thicknessColor = _ColorGreen;
						break;
					case 2:
						thicknessColor = _ColorBlue;
						break;
					case 3:
						thicknessColor = _ColorWhite;
						break;
				}

				// lerp between blue, green, and white depending on noise
				float4 temp = float4(0, 0, 0, 0);
				//temp = lerp(_ColorBlue, thicknessColor, float4(pixelCoord, 0, 0));
				temp = lerp(_ColorBlue, _ColorGreen, float4(pixelCoord, 0, 0));

				return temp;
			}
			ENDCG
		}

		// Reference: https://github.com/LasseWestmark/Sparkle-Shader-Unity
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Simplex3D.cginc"
			#include "SparklesCG.cginc"

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
				// ****SPARKLE LAYER****
				//Light Calculation
				float3 normal = normalize(i.normal);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.wPos));
				float3 reflDir = reflect(-viewDir, normal);
				float3 lightDirection;
				float atten = 1.0;
				lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float diffuse = max(0.0, dot(normal, lightDirection) * .5 + .5);
				float specular = saturate(dot(reflDir, lightDirection));
				float glitterSpecular = pow(specular, _GlitterPow);
				specular = pow(specular, _SpecPow);

				// Sparkles
				float sparkles = Sparkles(viewDir, i.wPos);

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
		/*
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

			v2f vert(appdata v)
			{
				v2f o;

				
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.pos = v.vertex;
				o.normal = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;

				foreach vertex v from the mesh do
				{
					float4 c = v; // Current vertex
					float wc = 0.0; // Water coefficient
					while there are higher neighbor vertices to c do
					{
						foreach higher neighbor vertex n do
						{
							// Higher with respect to gravity g
							float4 n; // neighbor vertex
							float4 cn = normalize(c - n); // normalized vector from c to n
							float4 p = dot(cn, g); 
						}
						Select neighbor nmin for which p is minimal
						// The most upward n with respect to g
						if c or nmin ∈ water supply then
						{
							d = distance between c and nmin
							Multiply d by −p
							if only c or nmin ∈ water supply then
							{
								// There is less water since one of the vertices is not in the water supply
								Divide the result by 2
							}
							wc = wc + result
						}
						c = nmin
					}
					Save wc at vertex v
				}
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = (1.0, 1.0, 1.0, 1.0);
				
				//Apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
		*/
	}
}
