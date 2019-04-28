﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//This file was created by Claire Yeash and Zach Phillips
Shader "Unlit/SnowShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_BumpTex("Normal Map", 2D) = "bump" {}
		_HeightTex("Height Map", 2D) = "height" {}
		_HeightScale("Height Scale", Range(.35, .5)) = 0.005

		// Color variables
		_ColorBlue("Blue", Color) = (0, 0, 1, .05)
		_ColorGreen("Green", Color) = (0, 1, 0, .05)
		_ColorWhite("White", Color) = (1, 1, 1, .05)

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

		#pragma surface surfaceFunction Lambert
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BumpTex;
		sampler2D _DissolveTex;
		float4 _Color;
		float4 _Tint;

		float3 myViewDir;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_HeightTex;
			float2 uv_BumpTex;
			float2 uv_DissolveTex;
			float3 viewDir;
		};

		//set up the normal map
		void surfaceFunction(Input IN, inout SurfaceOutput o)
		{
			//Get view direction
			myViewDir = IN.viewDir;

			//get Main Texture
			half4 tex = tex2D(_MainTex, IN.uv_MainTex);

			//Apply the main texture and normal map
			o.Albedo = tex.rgb * _Color.rgb * _Tint.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Alpha = _Color.a;

			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		}

		ENDCG

		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		//First pass (technically second edit due to the surface shader)
		//Used to add the color gradient
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

			sampler2D _HeightTex;
			float _HeightScale;
			float3 myViewDir;

			float4 _MainTex_ST;
			float4 _ColorBlue;
			float4 _ColorGreen;
			float4 _ColorWhite;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//Create some pixel cooordinated based on a kernal
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

		//Second pass
		//Adds sparkles
		//Reference: https://github.com/LasseWestmark/Sparkle-Shader-Unity
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
				float3 pos : TEXCOORD2;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color, _SpecColor;
			float _SpecPow, _GlitterPow;

			sampler2D _HeightTex;
			float _HeightScale;
			float3 myViewDir;

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
				float4 col = tex2D(_MainTex, i.uv) * _Color * diffuse;
				//Apply Specular and sparkles
				col += _SpecColor * (saturate(sparkles * glitterSpecular * 5) + specular);
				//Apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
		
		//Really cool feature in Unity shaders
		//Takes the data from the previous pass
		GrabPass{ "_GrabTexture" }
		
		//Third Pass
		//Expands the verticies based on a height map and height amount
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0

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
				float3 normal : NORMAL;
				float4 grabUv : TEXCOORD1;
				float3 viewDir : TEXCOORD2s;

				fixed4 diff : COLOR0; // diffuse lighting color
			};

			sampler2D _MainTex;

			sampler2D _HeightTex;
			float _HeightScale;

			float4 _MainTex_ST;

			//https://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//Get height information
				float4 temp = tex2Dlod(_HeightTex, float4(v.uv, 0, 0));
				float h = (normalize(temp.r) + normalize(temp.g) + temp.b) * _HeightScale;

				// get vertex normal in world space
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);

				// dot product between normal and light dir for the lighting
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				// factor in the light color
				o.diff = nl * _LightColor0;

				//multiple the verticies by the height modifier
				v.vertex *= h;

				//takes the verticies and cipts the object and grabsUVs
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.grabUv = ComputeGrabScreenPos(o.vertex);

				return o;
			}

			sampler2D _GrabTexture;

			fixed4 frag(v2f i) : SV_Target
			{
				//tex2D gives the nice color, but not the uniformness
				fixed4 grabTex = tex2D(_GrabTexture, i.grabUv);// *i.uv);// float4(i.grabUv.xyz * worldViewDir, 1.0));

				//tex2Dproj gives uniform but not the nice color
				fixed4 grabTexProj = tex2Dproj(_GrabTexture, i.grabUv);// *i.uv);// float4(i.grabUv.xyz * worldViewDir, 1.0));

				//solution? just do both lol
				float4 combinedTex = normalize(mul(grabTex, grabTexProj));

				//add lighting fanciness so that the outside layer is affected by light as well
				combinedTex *= i.diff;

				//alpha set so we can see through the expanded layer onto the original object
				//This creates our fake 'parallaxing' effect
				combinedTex.a = .7;

				//I was unable to find out how to get the textures to show on the expanded layer
				//I tried many things but the below code was the closest I got. I kept having an 
				//issue with the view direction influencing the color/texture applied.
				//combinedTex.xyz += grabTex.xyz;

				return combinedTex;
			}
			ENDCG
		}
	}
}