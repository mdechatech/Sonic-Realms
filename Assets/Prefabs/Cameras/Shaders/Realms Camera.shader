Shader "Unlit/Realms Camera"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
		}

		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.color = v.color;

				return o;
			}

			sampler2D _MainTex;
			uniform sampler2D _GlobalForegroundTex;
			uniform sampler2D _GlobalBackgroundTex;
			uniform sampler2D _GlobalOverlayTex;

			float4 frag(v2f i) : SV_Target
			{
				float4 bg = tex2D(_GlobalBackgroundTex, i.uv) * i.color;
				float4 fg = tex2D(_GlobalForegroundTex, i.uv) * i.color;
				float4 over = tex2D(_GlobalOverlayTex, i.uv) * i.color;

				bg.rgb = bg.rgb*(1.0 - fg.a) + fg.rgb*fg.a;
				bg.a = bg.a + fg.a*(1.0 - bg.a);

				bg.rgb = bg.rgb*(1.0 - over.a) + over.rgb*over.a;
				bg.a = bg.a + over.a*(1.0 - bg.a);

				return bg;
			}

			ENDCG
		}
	}
}
