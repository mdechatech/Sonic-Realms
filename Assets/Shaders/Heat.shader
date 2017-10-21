// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Heat"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		_ScrollTime ("Scroll Time", Range(0, 100)) = 50
		_BandFrequency ("Band Frequency", Range(0, 500)) = 10.0
		_BandHeight ("Band Height", Range(0.0, 1.0)) = 0.5
		_BandOffset ("Band Offset", Range(-100, 100)) = 2
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _ScrollTime;
			float _BandFrequency;
			float _BandHeight;
			float _BandOffset;

			fixed4 frag (v2f i) : SV_Target
			{
				float y = i.uv.y*_ScreenParams.y;
				float band_height = 1 / _BandFrequency;

				if ((y / 1000 + (_ScrollTime == 0 ? 0 : _Time.y/_ScrollTime)) % band_height <
					band_height*_BandHeight)
				{
					i.uv.x += _BandOffset / _ScreenParams.x;
				}
				fixed4 col = tex2D(_MainTex, i.uv);

				return col;
			}
			ENDCG
		}
	}
}
