Shader "Particles/Cutout" {
	Properties{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
	_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}

		Category{
		Tags{ "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" "Queue" = "Transparent" }
		Alphatest Greater[_Cutoff]
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }
		BindChannels{
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}

		// ---- Dual texture cards
		SubShader{
		Pass{
		SetTexture[_MainTex]{
		constantColor[_TintColor]
		combine constant * primary
	}
		SetTexture[_MainTex]{
		combine texture * previous DOUBLE
	}
	}
	}

		// ---- Single texture cards (does not do color tint)
		SubShader{
		Pass{
		SetTexture[_MainTex]{
		combine texture * primary
	}
	}
	}
	}
}
