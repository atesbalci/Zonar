Shader "Zonar/DiffuseCube" {
Properties {
	[PreRendererData]_OverrideColor("Override Color", Color) = (0, 0, 0, 0)
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

	CGPROGRAM
	#pragma surface surf Lambert noforwardadd

	uniform float _MIN;
	uniform float _MAX;
	uniform float _MULTIPLIER;
	uniform float4 _MAIN;
	uniform float4 _IDLE;

	float4 _OverrideColor;

	struct Input {
		float3 worldPos;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		o.Albedo = lerp(_MULTIPLIER * _IDLE.rgb,
		lerp(_MAIN.rgb, _OverrideColor.rgb, clamp(min(_OverrideColor.a, (IN.worldPos.y - _MIN)), 0.0f, 1.0f)),
		(IN.worldPos.y - _MIN) / (_MAX - _MIN));
	}
	ENDCG
}

Fallback "Mobile/VertexLit"
}
