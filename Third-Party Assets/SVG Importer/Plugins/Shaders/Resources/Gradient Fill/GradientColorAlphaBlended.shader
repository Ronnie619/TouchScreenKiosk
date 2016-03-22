// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

Shader "SVG Importer/GradientColor/GradientColorAlphaBlended" {
	
	// texcoord0.x : Gradient X Transformation
	// texcoord0.y : Gradient Y Transformation
	// texcoord1.x : Image Index Integer
	// texcoord1.y : Gradient Type Integer
	
	// _Params.x : Atlas Width
	// _Params.y : Atlas Height
	// _Params.z : Gradient width
	// _Params.w : Gradient height
	
	Properties {
		_GradientColor ("Gradient Color (RGBA)", 2D) = "white" { }
		_GradientShape ("Gradient Shape (RGBA)", 2D) = "white" { }
		_Params ("Params", Vector) = (1.0, 1.0, 1.0, 1.0)
	}
	
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off
		Fog { Mode Off }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			sampler2D _GradientColor;
			sampler2D _GradientShape;
			float4 _Params;
			
			struct vertex_input
			{
			    float4 vertex : POSITION;	
			    float2 texcoord0 : TEXCOORD0;
			    float2 texcoord1 : TEXCOORD1;
			    half4 color : COLOR;
			};
			
			struct vertex_output
			{
			    float4 vertex : POSITION;			    
			    float2 uv0 : TEXCOORD0;
			    float2 uv1 : TEXCOORD1;
			    float4 uv2 : TEXCOORD2;
			    half4 color : COLOR;
			};
			
			vertex_output vert(vertex_input v)
			{
			    vertex_output o;
			    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);			    
			    o.uv0 = v.texcoord0;
			    o.color = v.color;
				
				// half pixel
				float2 texelOffset = float2(0.5 / _Params.x, 0.5 / _Params.y);
				float imageIndex = v.texcoord1.x * _Params.z;
				
				// Horizontal Start
			    o.uv1.x = saturate((fmod(imageIndex, _Params.x) / _Params.x) + texelOffset.x);
			    // Horizontal Width
			    o.uv2 = saturate((1.0 - abs(float4(0.0, 1.0, 2.0, 3.0) - v.texcoord1.y)) * (_Params.z / _Params.x - texelOffset.x * 2.0));
			    // Vertical Start
			    o.uv1.y = saturate((floor((imageIndex / _Params.x) * _Params.w) / _Params.y) + texelOffset.y);
			    
			    return o;
			}
			
			float4 frag(vertex_output i) : COLOR
			{
				float gradient = dot(tex2D(_GradientShape, i.uv0), i.uv2) ;
				float2 gradientColorUV = float2(i.uv1.x + gradient, i.uv1.y);
				return tex2D(_GradientColor, gradientColorUV) * i.color;
			}
			ENDCG
        }
	}
}
