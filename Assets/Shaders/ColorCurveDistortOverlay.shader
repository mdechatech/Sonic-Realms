// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Color Curve + Distort Overlay"
{
	Properties
	{
		_Curve ("Texture", 2D) = "white" {}
		_Wavelength ("Wavelength", Range(0, 1)) = 0.3
		_Amplitude ("Amplitude", Range(0, 1)) = 0.002
		_ScrollRate ("Scroll Rate", Range(0, 1)) = 0.08
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	   	ZWrite Off

	   	GrabPass { "_GrabTexture" }

		Pass
		{

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

			sampler2D _MainTex;
			sampler2D _GrabTexture;
			sampler2D _Curve;

			float _Wavelength;
			float _Amplitude;
			float _ScrollRate;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv.x += sin(
				((i.uv.y + 
				_Time.y*_ScrollRate)/
				_Wavelength%1)
				*6.28)*_Amplitude;

				fixed4 col = tex2D(_GrabTexture, i.uv);

				col.r = tex2D(_Curve, col.r).r;
				col.g = tex2D(_Curve, col.g).g;
				col.b = tex2D(_Curve, col.b).b;

				return col;
			}
			ENDCG
		}
	}
}
