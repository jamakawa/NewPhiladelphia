Shader "Custom/Decal" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
			Lighting On
			Offset 0, -1
			SetTexture [_MainTex] { Combine Texture }
		}
	} 
	FallBack "Diffuse"
}
