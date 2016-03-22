// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;

namespace SVGImporter.Rendering
{
    public class SVGShader {
    	
        protected static Shader _GradientColorAdditive;
    	public static Shader GradientColorAdditive {
    		get {
                if(_GradientColorAdditive == null)
                    _GradientColorAdditive = Shader.Find ("SVG Importer/GradientColor/GradientColorAdditive");
                return _GradientColorAdditive;
    		}
    	}
        protected static Shader _GradientColorAlphaBlended;
    	public static Shader GradientColorAlphaBlended {
    		get {
                if(_GradientColorAlphaBlended == null)
                    _GradientColorAlphaBlended = Shader.Find ("SVG Importer/GradientColor/GradientColorAlphaBlended");
                return _GradientColorAlphaBlended;
    		}
    	}
        protected static Shader _GradientColorMultiply;
    	public static Shader GradientColorMultiply {
    		get {
                if(_GradientColorMultiply == null)
                    Shader.Find ("SVG Importer/GradientColor/GradientColorMultiply");
                return _GradientColorMultiply;                
    		}
    	}
        protected static Shader _GradientColorOpaque;
    	public static Shader GradientColorOpaque {
    		get {
                if(_GradientColorOpaque == null)
                    _GradientColorOpaque = Shader.Find ("SVG Importer/GradientColor/GradientColorOpaque");
                return _GradientColorOpaque;
    		}
        }
        protected static Shader _SolidColorAdditive;
    	public static Shader SolidColorAdditive {
    		get {
                if(_SolidColorAdditive == null)
                    _SolidColorAdditive = Shader.Find ("SVG Importer/SolidColor/SolidColorAdditive");
                return _SolidColorAdditive;
    		}
    	}
        protected static Shader _SolidColorAlphaBlended;
    	public static Shader SolidColorAlphaBlended {
    		get {
                if(_SolidColorAlphaBlended == null)
                    _SolidColorAlphaBlended = Shader.Find ("SVG Importer/SolidColor/SolidColorAlphaBlended");
                return _SolidColorAlphaBlended;
    		}
    	}
        protected static Shader _SolidColorMultiply;
    	public static Shader SolidColorMultiply {
    		get {
                if(_SolidColorMultiply == null)
                    _SolidColorMultiply = Shader.Find ("SVG Importer/SolidColor/SolidColorMultiply");
                return _SolidColorMultiply;
    		}
    	}
        protected static Shader _SolidColorOpaque;
    	public static Shader SolidColorOpaque {
    		get {
                if(_SolidColorOpaque == null)
                    _SolidColorOpaque = Shader.Find ("SVG Importer/SolidColor/SolidColorOpaque");
                return _SolidColorOpaque;
    		}
    	}
        protected static Shader _UI;
        public static Shader UI {
            get {
                if(_UI == null)
                    #if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
                    _UI = Shader.Find ("SVG Importer/UI/DefaultLegacy");
                    #else
                    _UI = Shader.Find ("SVG Importer/UI/Default");
                    #endif
                return _UI;
            }
        }
        protected static Shader _UIMask;
        public static Shader UIMask {
            get {
                if(_UIMask == null)
                #if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
                    _UIMask = Shader.Find ("SVG Importer/UI/DefaultMask");
                #else
                    _UIMask = Shader.Find ("SVG Importer/UI/DefaultMaskLegacy");
                #endif
                return _UIMask;
            }
        }
    }
}
