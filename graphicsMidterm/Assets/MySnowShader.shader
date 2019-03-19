//3/18/19 - Zach - Bumpiness - https://catlikecoding.com/unity/tutorials/rendering/part-6/
Shader "Unlit/MySnowShader"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset] _HeightMap("Heights", 2D) = "gray" {}
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 0.1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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

			//Declare uniforms
			sampler2D _MainTex;
			sampler2D _HeightMap;
			float4 _MainTex_ST;

			float4 _Tint;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			//My Functions
			

			fixed4 frag (v2f i) : SV_Target
			{
				float h = tex2D(_HeightMap, i.uv);
				i.normal = float3(0, h, 0);
				i.normal = normalize(i.normal);

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);

				float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
				albedo *= tex2D(_HeightMap, i.uv);
				
				return float4(albedo.rgb, 1);
			}
			ENDCG
		}
	}
}
