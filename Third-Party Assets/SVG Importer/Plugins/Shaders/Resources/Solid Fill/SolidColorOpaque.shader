// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

Shader "SVG Importer/SolidColor/SolidColorOpaque" {
	
	Properties {
	}
	
	SubShader
	{
		Tags {"RenderType"="Opaque" "Queue"="Geometry"}
		LOD 200
		Lighting Off
		Blend Off
		ZWrite On
		Cull Off
		Fog { Mode Off }	
			
		Pass
		{		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			struct appdata
			{
			    float4 vertex : POSITION;			    
			    half4 color : COLOR;			    
			};
			
			// vertex output
			struct vertdata
			{
			    float4 vertex : SV_POSITION;			    
			    half4 color : COLOR;			    
			};
			
			vertdata vert(appdata ad)
			{
			    vertdata o;
			    o.vertex = mul(UNITY_MATRIX_MVP, ad.vertex);			    
			    o.color = ad.color;			    
			    return o;
			}
			
			half4 frag(vertdata i) : COLOR
			{
				return i.color;
			}
			ENDCG
        }
	}
}
