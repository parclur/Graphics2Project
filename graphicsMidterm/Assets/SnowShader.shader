Shader "Unlit/SnowShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorBlue ("Blue", Color) = (0, 0, 1, 1)
		_ColorGreen ("Green", Color) = (0, 1, 0, 1)
		_ColorWhite ("White", Color) = (1, 1, 1, 1)
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
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				// ****ICE COLOR LAYER****
				// thickness based on noise
				float noiseColSum = 0f;
				noiseColSum = col.r + col.g + col.b;

				// lerp between blue, green, and white depending on noise
				float4 temp = float4(0, 0, 0, 0);

				temp += _ColorBlue * _ColorBlue.a;

				return col;
			}
			ENDCG
		}
	}
}
