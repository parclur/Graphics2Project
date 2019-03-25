//3/18/19 - Zach - Bumpiness - https://catlikecoding.com/unity/tutorials/rendering/part-6/
Shader "Unlit/MySnowShader"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpTex("Normalmap", 2D) = "bump" {}
		_HeightTex("Heightmap", 2D) = "bump" {}
		_Amount("Extrusion Amount", Range(-1,1)) = 0.5
	}
   
    SubShader
    {
		Tags {"RenderType" = "Transparent"}
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
    
		//Tags { "RenderType"="Opaque" }
		//LOD 100
		//
		//Cull Back
		//CGPROGRAM
		//#pragma vertex vert
		//#pragma fragment frag
		//// make fog work
		//#pragma multi_compile_fog
		//
		//#include "UnityCG.cginc"
		//
		//struct appdata
		//{
		//	float4 vertex : POSITION;
		//	float2 uv : TEXCOORD0;
		//};
		//
		//struct v2f
		//{
		//	float2 uv : TEXCOORD0;
		//	UNITY_FOG_COORDS(1)
		//	float4 vertex : SV_POSITION;
		//};
		//
		////Declare uniforms
		//sampler2D _MainTex;
		//sampler2D _HeightMap;
		//float4 _MainTex_ST;
		//
		//float4 _Tint;
		//
		//v2f vert (appdata v)
		//{
		//	v2f o;
		//	o.vertex = UnityObjectToClipPos(v.vertex);
		//	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		//	UNITY_TRANSFER_FOG(o,o.vertex);
		//	return o;
		//}		
		//
		//fixed4 frag (v2f i) : SV_Target
		//{
		//	float h = tex2D(_HeightMap, i.uv);
		//	i.normal = float3(0, h, 0);
		//	i.normal = normalize(i.normal);
		//
		//	// sample the texture
		//	fixed4 col = tex2D(_MainTex, i.uv);
		//	// apply fog
		//	//UNITY_APPLY_FOG(i.fogCoord, col);
		//
		//	float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
		//	albedo *= tex2D(_HeightMap, i.uv);
		//	
		//	return float4(albedo.rgb, 1);
		//}
		//ENDCG
	}
}
