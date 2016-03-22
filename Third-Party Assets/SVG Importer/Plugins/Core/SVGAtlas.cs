// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

//#define PRIVATE_BETA

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter
{
    using Rendering;

    public class SVGAtlasData
    {
        public List<CCGradient> gradients;
        public Dictionary<string, CCGradient> gradientCache;

        public SVGAtlasData()
        {
            InitGradientCache();
        }

        public void ClearGradientCache()
        {
            if (gradientCache != null) gradientCache.Clear ();            
            gradientCache = null;
        }

        public void InitGradientCache()
        {
            if(gradients == null || gradients.Count == 0){ 
                gradients = new List<CCGradient>(){ GetDefaultGradient() };
            };
            
            if (gradientCache == null) {
                gradientCache = new Dictionary<string, CCGradient> ();          
                if(gradients != null)
                {
                    for (int i = 0; i < gradients.Count; i++) {
                        if (!gradientCache.ContainsKey (gradients [i].hash)) {
                            gradientCache.Add (gradients [i].hash, gradients [i]);
                        }
                    }
                }
            }
        }

        public void RebuildGradientCache()
        {
            ClearGradientCache ();
            InitGradientCache ();
        }

        public static CCGradient GetDefaultGradient()
        {
            CCGradientColorKey[] colorKeys = new CCGradientColorKey[]{
                new CCGradientColorKey(Color.white, 0f), new CCGradientColorKey(Color.white, 1f)
            };
            CCGradientAlphaKey[] alphaKeys = new CCGradientAlphaKey[]{
                new CCGradientAlphaKey(1f, 0f), new CCGradientAlphaKey(1f, 1f)
            };            
            return new CCGradient(colorKeys, alphaKeys);
        }
        
        public CCGradient AddGradient(CCGradient gradient)
        {
            if (gradient == null || !gradient.initialised)
                return null;
            
            if (gradientCache == null || gradientCache.Count == 0)
                RebuildGradientCache ();
            
            if (gradientCache.ContainsKey (gradient.hash)) {          
                gradient = gradientCache [gradient.hash];
                return gradient;
            }

            gradient.index = gradients.Count;
            gradients.Add (gradient);
            gradientCache.Add (gradient.hash, gradient);

            return  gradient;
        }
        
        public CCGradient GetGradient (int index)
        {
            if (gradients == null)
                return null;
            
            if (gradients.Count == 0)
                return null;
            
            index = Mathf.Clamp (index, 0, gradients.Count - 1);
            return gradients [index];
        }
        
        public SVGFill GetGradient (SVGFill gradient)
        {
            gradient.gradientColors = GetGradient (gradient.gradientColors);
            return gradient;
        }
        
        public CCGradient GetGradient (CCGradient gradient)
        {
            if (gradient == null || !gradient.initialised || gradientCache == null)
                return null;

            if (gradientCache.ContainsKey (gradient.hash)) {            
                gradient = gradientCache [gradient.hash];
                gradient.references++;
                return gradient;        
            } else {
                return null;
            }
        }

        public void Clear()
        {
            if(gradients != null)
            {
                gradients.Clear();
                gradients = null;
            }

            if(gradientCache != null)
            {
                gradientCache.Clear();
                gradientCache = null;
            }
        }

    }

    public class SVGAtlas : MonoBehaviour {

		protected static Texture2D _whiteTexture;
		public static Texture2D whiteTexture
		{
			get {
				if(_whiteTexture == null) _whiteTexture = GenerateWhiteTexture();
				return _whiteTexture;
			}
		}

        protected static Texture2D _gradientShapeTexture;
        public static Texture2D gradientShapeTexture
        {
            get {
                if(_gradientShapeTexture == null) _gradientShapeTexture = GenerateGradientShapeTexture(_gradientShapeTextureSize);                
                return _gradientShapeTexture;
            }
        }

        protected  static int _gradientShapeTextureSize = 512;
        public static int gradientShapeTextureSize {
            get {
                return _gradientShapeTextureSize;
            }
            set {
                if(_gradientShapeTextureSize == value) return;
                if(_gradientShapeTexture != null) DestroyImmediate(_gradientShapeTexture);
                _gradientShapeTexture = GenerateGradientShapeTexture(_gradientShapeTextureSize);
            }
        }

        public static void ClearGradientShapeTexture ()
        {
            if (gradientShapeTexture == null)
                return;
            
            DestroyImmediate(_gradientShapeTexture);
            _gradientShapeTexture = null;
        }

        protected SVGAtlasData _atlasData;
        public SVGAtlasData atlasData
        {
            get {
                return _atlasData;
            }
        }

        public Material ui;
        public Material uiMask;
        public Material opaqueSolid;
        public Material transparentSolid;
        public Material opaqueGradient;
        public Material transparentGradient;

    	public List<Texture2D> atlasTextures;		
    	public List<Material> materials;
    	
    	public int gradientWidth = 128;
    	public int gradientHeight = 4;
    	public int atlasTextureWidth = 512;
        public int atlasTextureHeight = 512;
    	public int imageIndex = 0;
    	public int atlasIndex = 0;
    	
        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);

            #if PRIVATE_BETA && !UNITY_EDITOR
            PrivateBetaBuild.Init();
            #endif
        }

        protected static SVGAtlas _Instance;
        public static SVGAtlas Instance
        {
            get {
                if(_Instance == null)
                {
                    GameObject go = new GameObject("SVGAtlas", typeof(SVGAtlas));
                    _Instance = go.GetComponent<SVGAtlas>();
                    _Instance.Init();
                }
                
                return _Instance;
            }
        }

        public bool ContainsMaterial(Material material)
        {
            if(material == ui) return true;
            if(material == uiMask) return true;
            if(material == opaqueSolid) return true;
            if(material == transparentSolid) return true;
            if(material == opaqueGradient) return true;
            if(material == transparentGradient) return true;

            if(materials != null) {
                if(materials.Contains(material)) return true;
            }

            return false;
        }

        public static void ClearAll()
        {
            if(_Instance == null)
                return;
            if(_Instance.ui != null)
            {
                DestroyObjectInternal(_Instance.ui);
                _Instance.ui = null;
            }
            if(_Instance.uiMask != null)
            {
                DestroyObjectInternal(_Instance.uiMask);
                _Instance.uiMask = null;
            }
            if(_Instance.opaqueSolid != null)
            {
                DestroyObjectInternal(_Instance.opaqueSolid);
                _Instance.opaqueSolid = null;
            }
            if(_Instance.transparentSolid != null)
            {
                DestroyObjectInternal(_Instance.transparentSolid);
                _Instance.transparentSolid = null;
            }
            if(_Instance.opaqueGradient != null)
            {
                DestroyObjectInternal(_Instance.opaqueGradient);
                _Instance.opaqueGradient = null;
            }
            if(_Instance.transparentGradient != null)
            {
                DestroyObjectInternal(_Instance.transparentGradient);
                _Instance.transparentGradient = null;
            }

            _Instance.ClearAllData();
            _Instance.ClearMaterials();
            _Instance.ClearAtlasTextures();

            DestroyObjectInternal(_Instance.gameObject);
            _Instance = null;
        }

    	protected void Init ()
    	{
    		if (materials == null) materials = new List<Material> ();
            if(_atlasData == null)
            {
                _atlasData = new SVGAtlasData();
            }
    	}
    	
        const int pixelOffset = 1;
        public static void RenderGradient (Texture2D texture, CCGradient gradient, int x, int y, int gradientWidth, int gradientHeight)
        {
            //Debug.Log(string.Format("x: {0}, y: {1}, gradient: {2}", x, y, gradient));
            if (texture == null || gradient == null || !gradient.initialised)
                return;
            
            float tempWidth = gradientWidth - 1 - pixelOffset * 2;
            Color[] pixels = new Color[gradientWidth * gradientHeight];
            
            Color pixel;
            
            for (int i = 0; i < gradientWidth; i++) {
                pixel = gradient.Evaluate ((float)(i - pixelOffset) / tempWidth);
                for(int j = 0; j < gradientHeight; j++) {
                    pixels [gradientWidth * j + i] = pixel;
                }
            }
            
            texture.SetPixels(x, y, gradientWidth, gradientHeight, pixels);
        }

    	public int imagePerRow {
    		get {
    			return atlasTextureWidth / gradientWidth;
    		}
    	}

        public bool GetCoords (out int x, out int y)
        {
            bool newTexture = (atlasTextures == null || atlasTextures.Count == 0);            
			GetCoords(out x, out y, imageIndex, gradientWidth, gradientHeight, atlasTextureWidth, atlasTextureHeight);
            return newTexture;
        }

		public static void GetCoords(out int x, out int y, int imageIndex, int gradientWidth, int gradientHeight, int atlasTextureWidth, int atlasTextureHeight)
		{
			int index = imageIndex * gradientWidth;
			x = index % atlasTextureWidth;
			y = Mathf.FloorToInt (index / atlasTextureWidth) * gradientHeight;
		}

        public Texture CreateAtlasTexture (int index, int width, int height)
        {
            if (atlasTextures == null)
                atlasTextures = new List<Texture2D> ();

			Texture2D texture = CreateTexture(width, height);            
            texture.name = "Atlas "+index.ToString();
            
            if (index >= atlasTextures.Count - 1) {               
                atlasTextures.Add (texture);
            } else if (index >= 0) {                         
                atlasTextures [index] = texture;
            }

			return texture;
        }

		public static Texture2D CreateTexture(int width, int height)
		{
			Texture2D texture = new Texture2D (width, height, TextureFormat.ARGB32, false);
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Clamp;
//			texture.alphaIsTransparency = true;
			texture.anisoLevel = 0;
			return texture;
		}

        public CCGradient AddGradient(CCGradient gradient, bool renderAtlasTexture = true)
        {
            if (gradient == null || !gradient.initialised)
                return null;
            if(_atlasData == null) return null ;
            gradient = _atlasData.AddGradient(gradient);

            int x = 0, y = 0;
            bool newTexture = GetCoords (out x, out y);
            
            gradient.index = imageIndex++;
            gradient.atlasIndex = atlasIndex;
            
            if(renderAtlasTexture)
            {
                if (newTexture) {
                    CreateAtlasTexture (atlasIndex, atlasTextureWidth, atlasTextureHeight);
                }
                
                RenderGradient (atlasTextures [atlasIndex], gradient, x, y, gradientWidth, gradientHeight);
                atlasTextures [atlasIndex].Apply ();
            }

            return  gradient;
        }

        public void RebuildAtlas ()
        {
            ClearAtlasTextures ();           
            imageIndex = 0;
            atlasIndex = 0;

            if (_atlasData == null) return;
            List<CCGradient> gradients = _atlasData.gradients;
            if(gradients == null) return;

            CreateAtlasTexture (atlasIndex, atlasTextureWidth, atlasTextureHeight);
            int x, y;
            for (int i = 0; i < gradients.Count; i++) {
                bool newTexture = GetCoords (out x, out y);
                if (newTexture) {
                    CreateAtlasTexture (atlasIndex, atlasTextureWidth, atlasTextureHeight);
                }
                
                gradients [i].atlasIndex = atlasIndex;
                gradients [i].index = imageIndex;
				RenderGradient (atlasTextures [atlasIndex], gradients [i], x, y, gradientWidth, gradientHeight);
                
                imageIndex++;
            }
            
            for (int i = 0; i < atlasTextures.Count; i++) {
                atlasTextures [i].Apply (false);
            }       
        }

		public static Texture2D GenerateGradientAtlasTexture(CCGradient[] gradients, int gradientWidth, int gradientHeight)
		{
			if(gradients == null || gradients.Length == 0)
				return null;

			int gradientCount = gradients.Length;
			int atlasTextureWidth = gradientWidth * 2;
			int atlasTextureHeight = Mathf.CeilToInt((gradientCount * gradientWidth) / atlasTextureWidth) * gradientHeight + gradientHeight;
			Texture2D texture = CreateTexture(atlasTextureWidth, atlasTextureHeight);

			int x, y;
			for (int i = 0; i < gradients.Length; i++) {
				GetCoords(out x, out y, i, gradientWidth, gradientHeight, atlasTextureWidth, atlasTextureHeight);
				RenderGradient (texture, gradients [i], x, y, gradientWidth, gradientHeight);		
			}

			texture.Apply (false);
			return texture;
		}

        const float PI2 = Mathf.PI * 2f;
        public static Texture2D GenerateGradientShapeTexture (int textureSize)
        {
            Texture2D texture = new Texture2D (textureSize, textureSize, TextureFormat.ARGB32, false);
            texture.hideFlags = HideFlags.DontSave;
            texture.name = "Gradient Shape Texture";
            texture.anisoLevel = 0;
            texture.filterMode = FilterMode.Trilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            
            int totalPixels = gradientShapeTextureSize * gradientShapeTextureSize;
            Color32[] texturePixels = new Color32[totalPixels];
            float angle;
            
            float x = 0, y = 0, halfSize = gradientShapeTextureSize * 0.5f, sizeMinusOne = gradientShapeTextureSize - 1;
            for (int i = 0; i < totalPixels; i++) {
                x = i % gradientShapeTextureSize;
                y = Mathf.Floor ((float)i / (float)gradientShapeTextureSize);
                
                // linear
                texturePixels [i].r = (byte)Mathf.RoundToInt (x / sizeMinusOne * 255);
                
                // radial
                texturePixels [i].g = (byte)Mathf.RoundToInt (Mathf.Clamp01 (Mathf.Sqrt (Mathf.Pow (halfSize - x, 2f) + Mathf.Pow (halfSize - y, 2f)) / (halfSize - 1f)) * 255);
                
                // conical
                angle = Mathf.Atan2(-halfSize + y, -halfSize + x);
                if(angle < 0)
                {
                    angle = PI2 + angle;
                }

                texturePixels [i].b = (byte)Mathf.RoundToInt(Mathf.Clamp01((angle / PI2)) * 255);

                // solid
                texturePixels [i].a = (byte)255;
            }
            
            texture.SetPixels32 (texturePixels);
            texture.Apply (true);
            return texture;
        }

		public static Texture2D GenerateWhiteTexture ()
		{
			Texture2D texture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
			texture.hideFlags = HideFlags.DontSave;
			texture.name = "White Texture";
			texture.anisoLevel = 0;
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.SetPixel(0, 0, Color.white);
			texture.Apply (false);
			return texture;
		}

        public Material GetMaterial (SVGFill fill)
        {
            Material output = null;
            switch (fill.fillType)
            {
                case FILL_TYPE.SOLID:
                    output = GetColorMaterial(fill);
                    break;
                case FILL_TYPE.GRADIENT:
                    output = GetGradientMaterial(fill);
                    break;
                case FILL_TYPE.TEXTURE:
                    break;
            }
            return output;
        }
        
        protected Material GetGradientMaterial (SVGFill fill)
        {       
            Material output = null;
            Shader shader = null;
            switch (fill.blend) {
                case FILL_BLEND.OPAQUE:
                    shader = SVGShader.GradientColorOpaque;
                    break;
                case FILL_BLEND.ALPHA_BLENDED:
                    shader = SVGShader.GradientColorAlphaBlended;
                    break;
                case FILL_BLEND.ADDITIVE:
                    shader = SVGShader.GradientColorAdditive;
                    break;
                case FILL_BLEND.MULTIPLY:
                    shader = SVGShader.GradientColorMultiply;
                    break;
                default:
                    shader = SVGShader.GradientColorOpaque;
                    break;
            }
            
            for (int i = 0; i < materials.Count; i++) {
                if (materials [i] == null)
                    continue;           
                if (materials [i].shader != shader)
                    continue;           
                if (fill.gradientColors.atlasIndex < 0 || fill.gradientColors.atlasIndex >= atlasTextures.Count)
                    continue;
                Texture texture = atlasTextures [fill.gradientColors.atlasIndex];
                if (texture == null)
                    continue;
                if (materials [i].GetTexture ("_GradientColor") != texture)
                    continue;
                
                output = materials [i];
                output.SetTexture ("_GradientShape", gradientShapeTexture);
                output.SetVector ("_Params", new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
            }
            
            if (output == null) {
                output = new Material (shader);
                Texture2D texture = atlasTextures [fill.gradientColors.atlasIndex];
                output.SetTexture ("_GradientColor", texture);
                output.SetTexture ("_GradientShape", gradientShapeTexture);
                output.SetVector ("_Params", new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
                materials.Add (output);           
            }
         
            return output;
        }

        public void InitMaterials()
        {
            if(opaqueSolid == null)
                opaqueSolid = new Material(SVGShader.SolidColorOpaque);
            if(transparentSolid == null)
                transparentSolid = new Material(SVGShader.SolidColorAlphaBlended);
            if(opaqueGradient == null)
            {
                opaqueGradient = new Material(SVGShader.GradientColorOpaque);
                if(atlasTextures != null && atlasTextures.Count > 0)
                    opaqueGradient.SetTexture ("_GradientColor", atlasTextures[0]);
                opaqueGradient.SetTexture ("_GradientShape", gradientShapeTexture);
                opaqueGradient.SetVector ("_Params", new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
            }
            if(transparentGradient == null)
            {
                transparentGradient = new Material(SVGShader.GradientColorAlphaBlended);
                if(atlasTextures != null && atlasTextures.Count > 0)
                    transparentGradient.SetTexture ("_GradientColor", atlasTextures[0]);
                transparentGradient.SetTexture ("_GradientShape", gradientShapeTexture);
                transparentGradient.SetVector ("_Params", new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
            }
            if(ui == null)
            {
                ui = new Material(SVGShader.UI);
                if(atlasTextures != null && atlasTextures.Count > 0)
                    ui.SetTexture ("_GradientColor", atlasTextures[0]);
                ui.SetTexture ("_GradientShape", gradientShapeTexture);
                ui.SetVector ("_Params", new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
            }
            if(uiMask == null)
            {
                uiMask = new Material(SVGShader.UIMask);
                if(atlasTextures != null && atlasTextures.Count > 0)
                    uiMask.SetTexture ("_GradientColor", atlasTextures[0]);
                uiMask.SetTexture ("_GradientShape", gradientShapeTexture);
                uiMask.SetVector ("_Params", new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
            }
        }

        protected Material GetColorMaterial (SVGFill fill)
        {       
            Material output = null;
            Shader shader = null;
            switch (fill.blend) {
                case FILL_BLEND.OPAQUE:
                    shader = SVGShader.SolidColorOpaque;
                    break;
                case FILL_BLEND.ALPHA_BLENDED:
                    shader = SVGShader.SolidColorAlphaBlended;
                    break;
                case FILL_BLEND.ADDITIVE:
                    shader = SVGShader.SolidColorAdditive;
                    break;
                case FILL_BLEND.MULTIPLY:
                    shader = SVGShader.SolidColorMultiply;
                    break;
                default:
                    shader = SVGShader.SolidColorOpaque;
                    break;
            }
            
            for (int i = 0; i < materials.Count; i++) {
                if (materials [i] == null)
                    continue;           
                if (materials [i].shader != shader)
                    continue;           
                
                output = materials [i];           
            }
            
            if (output == null) {
                output = new Material (shader);
                materials.Add (output);           
            }
            return output;
        }

        public Vector4 textureParams
        {
            get {
                return new Vector4 (atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight);
            }
        }

        protected string GetMegaBytes(int bits)
        {
            float size = bits / 1024 / 1024 / 8;
            if (size < 1f)
            {
                return Mathf.FloorToInt(bits / 1024 / 8).ToString() + " KB";
            } else
            {
                return size.ToString(".0") + " MB";
            }
        }

        public void ClearAllData ()
        {       
            if(_atlasData != null)
            {
                _atlasData.Clear();
            }
        }

        public void ClearMaterials()
        {
            if(materials == null)
                return;

            for(int i = 0; i < materials.Count; i++)
            {
                if(materials[i] == null)
                    continue;
                DestroyObjectInternal(materials[i]);
            }
            materials.Clear();
            materials = null;
        }

        public void ClearAtlasTextures ()
        {
            if (atlasTextures == null || atlasTextures.Count == 0)
                return;
            
            imageIndex = 0;
            atlasIndex = 0;
            
//            string assetPath;
            for (int i = 0; i < atlasTextures.Count; i++) {
                if (atlasTextures [i] == null)
                    continue;
                           
                DestroyObjectInternal(atlasTextures [i]);
                atlasTextures [i] = null;
            }
            
            atlasTextures.Clear ();
        }
        
        static void DestroyObjectInternal(UnityEngine.Object target)
        {
            #if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            } else {
                UnityEngine.Object.DestroyImmediate(target, true);
            }
            #else
            UnityEngine.Object.Destroy(target);
            #endif
        }

        internal static Camera[] GetAllCameras()
        {
            return Camera.allCameras;
        }
        
        internal static void AddComponent<T>(Component component) where T : MonoBehaviour
        {
            if(component == null)
                return;
            
            GameObject gameObject = component.gameObject; 
            
            if(gameObject == null)
                return;
            
            if(gameObject.GetComponent<T>() != null)
                return;
            
            gameObject.AddComponent<T>();
        }
    }

    #if PRIVATE_BETA && !UNITY_EDITOR
    internal sealed class PrivateBetaBuild : MonoBehaviour
    {
        Camera camera;
        
        void OnGUI()
        {
            if(camera == null)
                camera = GetComponent<Camera>();
            
            string labelText = "SVG Importer | Private Beta Build";
            GUI.skin.box.fontSize = Screen.height / 40;
            Vector2 labelSize = GUI.skin.box.CalcSize(new GUIContent(labelText));
            float offset = 10f;
            Rect finRect = new Rect(camera.pixelWidth - labelSize.x - offset, camera.pixelHeight - labelSize.y - offset, labelSize.x, labelSize.y);
            GUI.Box(finRect, new GUIContent(labelText));
        }
        
        internal static void Init()
        {
            Camera[] cameras = SVGAtlas.GetAllCameras();
            if(cameras != null && cameras.Length > 0)
            {
                for(int i = 0; i < cameras.Length; i++)
                {
                    SVGAtlas.AddComponent<PrivateBetaBuild>(cameras[i]);
                }
            }
        }
    }
    #endif
}
