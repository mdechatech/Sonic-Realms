Shader "Unlit/Palette Controller"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		// From zero to sqrt(3), which is the distance between (0, 0, 0) and (1, 1, 1)
		_Threshold ("Threshold", Range(0, 1.733)) = 0.125
		
		_ColorFrom1 ("Color From 1", Color) = (0, 0, 0, 0)
		_ColorFrom2 ("Color From 2", Color) = (0, 0, 0, 0)
		_ColorFrom3 ("Color From 3", Color) = (0, 0, 0, 0)
		_ColorFrom4 ("Color From 4", Color) = (0, 0, 0, 0)
		_ColorFrom5 ("Color From 5", Color) = (0, 0, 0, 0)
		_ColorFrom6 ("Color From 6", Color) = (0, 0, 0, 0)
		_ColorFrom7 ("Color From 7", Color) = (0, 0, 0, 0)
		_ColorFrom8 ("Color From 8", Color) = (0, 0, 0, 0)
			/*
		_ColorFrom9 ("Color From 9", Color) = (0, 0, 0, 0)
		_ColorFrom10 ("Color From 10", Color) = (0, 0, 0, 0)
		_ColorFrom11 ("Color From 11", Color) = (0, 0, 0, 0)
		_ColorFrom12 ("Color From 12", Color) = (0, 0, 0, 0)
		_ColorFrom13 ("Color From 13", Color) = (0, 0, 0, 0)
		_ColorFrom14 ("Color From 14", Color) = (0, 0, 0, 0)
		_ColorFrom15 ("Color From 15", Color) = (0, 0, 0, 0)
		_ColorFrom16 ("Color From 16", Color) = (0, 0, 0, 0)
		*/

		_ColorTo1 ("Color To 1", Color) = (0, 0, 0, 0)
		_ColorTo2 ("Color To 2", Color) = (0, 0, 0, 0)
		_ColorTo3 ("Color To 3", Color) = (0, 0, 0, 0)
		_ColorTo4 ("Color To 4", Color) = (0, 0, 0, 0)
		_ColorTo5 ("Color To 5", Color) = (0, 0, 0, 0)
		_ColorTo6 ("Color To 6", Color) = (0, 0, 0, 0)
		_ColorTo7 ("Color To 7", Color) = (0, 0, 0, 0)
		_ColorTo8 ("Color To 8", Color) = (0, 0, 0, 0)
			/*
		_ColorTo9 ("Color To 9", Color) = (0, 0, 0, 0)
		_ColorTo10 ("Color To 10", Color) = (0, 0, 0, 0)
		_ColorTo11 ("Color To 11", Color) = (0, 0, 0, 0)
		_ColorTo12 ("Color To 12", Color) = (0, 0, 0, 0)
		_ColorTo13 ("Color To 13", Color) = (0, 0, 0, 0)
		_ColorTo14 ("Color To 14", Color) = (0, 0, 0, 0)
		_ColorTo15 ("Color To 15", Color) = (0, 0, 0, 0)
		_ColorTo16 ("Color To 16", Color) = (0, 0, 0, 0)
		*/
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			ZWrite Off
			Tags {"Queue" = "Transparent"}
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _ColorFrom1;
			float4 _ColorFrom2;
			float4 _ColorFrom3;
			float4 _ColorFrom4;
			float4 _ColorFrom5;
			float4 _ColorFrom6;
			float4 _ColorFrom7;
			float4 _ColorFrom8;
			/*
			float4 _ColorFrom9;
			float4 _ColorFrom10;
			float4 _ColorFrom11;
			float4 _ColorFrom12;
			float4 _ColorFrom13;
			float4 _ColorFrom14;
			float4 _ColorFrom15;
			float4 _ColorFrom16;
			*/

			float4 _ColorTo1;
			float4 _ColorTo2;
			float4 _ColorTo3;
			float4 _ColorTo4;
			float4 _ColorTo5;
			float4 _ColorTo6;
			float4 _ColorTo7;
			float4 _ColorTo8;
			/*
			float4 _ColorTo9;
			float4 _ColorTo10;
			float4 _ColorTo11;
			float4 _ColorTo12;
			float4 _ColorTo13;
			float4 _ColorTo14;
			float4 _ColorTo15;
			float4 _ColorTo16;
			*/

			sampler2D _MainTex;
			float _Threshold;

			bool color_within(fixed4 a, fixed4 b)
			{
				return distance(a.rgb, b.rgb) < _Threshold;
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				if (color_within(col, _ColorFrom1)) return _ColorTo1;
				if (color_within(col, _ColorFrom2)) return _ColorTo2;
				if (color_within(col, _ColorFrom3)) return _ColorTo3;
				if (color_within(col, _ColorFrom4)) return _ColorTo4;
				if (color_within(col, _ColorFrom5)) return _ColorTo5;
				if (color_within(col, _ColorFrom6)) return _ColorTo6;
				if (color_within(col, _ColorFrom7)) return _ColorTo7;
				if (color_within(col, _ColorFrom8)) return _ColorTo8;
				/*
				if (color_within(col, _ColorFrom9)) return _ColorTo9;
				if (color_within(col, _ColorFrom10)) return _ColorTo10;
				if (color_within(col, _ColorFrom11)) return _ColorTo11;
				if (color_within(col, _ColorFrom12)) return _ColorTo12;
				if (color_within(col, _ColorFrom13)) return _ColorTo13;
				if (color_within(col, _ColorFrom14)) return _ColorTo14;
				if (color_within(col, _ColorFrom15)) return _ColorTo15;
				if (color_within(col, _ColorFrom16)) return _ColorTo16;
				*/
				return col;
			}
			ENDCG
		}
	}
}
