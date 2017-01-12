// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Realms Water"
{
	Properties
	{
		_DisplaceTex("Displacement Texture", 2D) = "white" {}
		_BGColorTex("Background Color Curve Texture", 2D) = "white" {}
		_BGColorTint("Background Color Tint", Color) = (0, 0, 1, 0)
		_FGColorTex("Foreground Color Curve Texture", 2D) = "white" {}
		_FGColorTint("Foreground Color Tint", Color) = (0, 0, 1, 0)

		_Magnitude("Magnitude", Range(0, 0.1)) = 1
		_DisplaceScaleX("Displacement Scale X", Float) = 1
		_DisplaceScaleY("Displacement Scale Y", Float) = 1
		
		_HorizSpeed("Horizontal Speed", Range(0, 1)) = 0
		_VertSpeed("Vertical Speed", Range(0, 1)) = 1
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
			Blend One Zero

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
				float2 screenuv : TEXCOORD1;
				half4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;

				o.screenuv = ComputeScreenPos(o.vertex) / o.vertex.w;

				o.color = v.color;

				return o;
			}

			float4 alpha_blend(float4 src, float4 dst)
			{
				float4 result;

				result.rgb = src.rgb*src.a + dst.rgb*dst.a*(1 - src.a);
				result.a = src.a + dst.a*(1.0 - src.a);

				return result;
			}

			float4 color_curve(float4 color, sampler2D tex)
			{
				return float4(tex2D(tex, color.r).r, tex2D(tex, color.g).g, tex2D(tex, color.b).b, color.a);
			}

			sampler2D _MainTex;
			sampler2D _DisplaceTex;
			sampler2D _BGColorTex;
			sampler2D _FGColorTex;
			sampler2D _GlobalForegroundTex;
			sampler2D _GlobalBackgroundTex;

			float4 _BGColorTint;
			float4 _FGColorTint;

			float _DisplaceScaleX;
			float _DisplaceScaleY;
			float _HorizSpeed;
			float _VertSpeed;
			float _Magnitude;

			float4 frag(v2f i) : SV_Target
			{
				//float3 wpos = mul(unity_ObjectToWorld, i.vertex);
				
				float2 bguv = float2(
					(i.uv.x + _Time.x * _HorizSpeed),
					(i.uv.y + _Time.x * _VertSpeed));

				bguv = float2(
					((bguv.x * 2) - 1) / _DisplaceScaleX / 2,
					((bguv.y * 2) - 1) / _DisplaceScaleY / 2);
				
				float2 disp = tex2D(_DisplaceTex, bguv).xy;

				disp = ((disp * 2) - 1) * _Magnitude;

				float4 fg = tex2D(_GlobalForegroundTex, i.screenuv) * i.color;
				float4 bg = tex2D(_GlobalBackgroundTex, i.screenuv + disp) * i.color;

				bg = color_curve(bg, _BGColorTex);
				fg = color_curve(fg, _FGColorTex);

				bg = alpha_blend(_BGColorTint, bg);
				fg = alpha_blend(_FGColorTint, fg)*fg.a;

				//return tex2D(_DisplaceTex, bguv);

				return alpha_blend(fg, bg);
			}

			ENDCG
		}
	}
}
