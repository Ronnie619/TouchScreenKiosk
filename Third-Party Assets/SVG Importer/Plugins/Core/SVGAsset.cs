// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using System.Collections;
using System.Collections.Generic;

namespace SVGImporter 
{
    using Rendering;
    using Geometry;
    using Utils;
    using Data;
    using Document;

    [System.Serializable]
    public struct SVGLayer
    {
        public string name;
        public int vertexStart;
        public int vertexCount;
        public Vector2 center;
        public Vector2 size;

        public SVGLayer(string name, int vertexStart, int vertexCount, Vector2 center, Vector2 size)
        {
            this.name = name;
            this.vertexStart = vertexStart;
            this.vertexCount = vertexCount;
            this.center = center;
            this.size = size;
        }
    }

    public enum SVGUseGradients
    {
        Always,
        Auto,
        Never
    }

    public enum SVGMeshCompression
    {
        Off,
        Low,
        Medium,
        High
    }

    public enum SVGAssetFormat
    {
        Opaque = 0,
        Transparent = 1,
        uGUI = 2
    }

    public class SVGAsset : ScriptableObject
    {
        [FormerlySerializedAs("lastTimeModified")]
        [SerializeField]
        protected long _lastTimeModified;

        [FormerlySerializedAs("documentAsset")]
        [SerializeField]
        protected SVGDocumentAsset _documentAsset;

        [FormerlySerializedAs("sharedMesh")]
        [SerializeField]
        protected Mesh _sharedMesh;

        /// <summary>
        /// Returns the shared mesh of the SVG Asset. (Read Only)
        /// </summary>
        public Mesh sharedMesh
        {
            get {
    #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    return runtimeMesh;
                } else {
                    _runtimeMesh = null;
                    return _sharedMesh;
                }
    #else
                return runtimeMesh;
    #endif
            }
        }

        public bool isOpaque
        {
            get {
                if(_format == SVGAssetFormat.Transparent || _format == SVGAssetFormat.uGUI) return false;
                if(_sharedShaders == null || _sharedShaders.Length == 0) return true;
                for(int i = 0; i < _sharedShaders.Length; i++)
                {
                    if(string.IsNullOrEmpty(_sharedShaders[i])) continue;
                    if(_sharedShaders[i].ToLower().Contains("opaque")) return true;
                }

                return false;
            }
        }
        
        /// <summary>
        /// Returns the instanced mesh of the SVG Asset. (Read Only)
        /// </summary>
        public Mesh mesh
        {
            get {
                Mesh sharedMeshReference = sharedMesh;
                if(sharedMeshReference == null)
                    return null;
                Mesh clonedMesh = SVGMeshUtils.Clone(sharedMeshReference);
                if(clonedMesh != null)
                {
                    clonedMesh.name += " Instance "+clonedMesh.GetInstanceID();                    
                }
                return clonedMesh;
            }
        }

        protected Mesh _runtimeMesh;
        protected Mesh runtimeMesh
        {
            get {
                if(_runtimeMesh == null)
                {
                    if(!hasGradients)
                    {
                        _runtimeMesh = _sharedMesh;
                    } else {
                        Dictionary<int, int> gradientCache = new Dictionary<int, int>();
                        CCGradient[] gradients = new CCGradient[_sharedGradients.Length];
                        for(int i = 0; i < _sharedGradients.Length; i++)
                        {
                            if(_sharedGradients[i] == null)
                                continue;
                            gradients[i] = SVGAtlas.Instance.AddGradient(_sharedGradients[i].Clone());
                            gradientCache.Add(_sharedGradients[i].index, gradients[i].index);
                        }

                        _runtimeMesh = SVGMeshUtils.Clone(_sharedMesh);
                        if(_runtimeMesh.uv2 != null && _runtimeMesh.uv2.Length > 0)
                        {
                            Vector2[] uv2 = _runtimeMesh.uv2;
                            for(int i = 0; i < uv2.Length; i++)
                            {
                                uv2[i].x = (float)gradientCache[(int)uv2[i].x];
                            }
                            _runtimeMesh.uv2 = uv2;
                        }
                    }    

                    SVGAtlas.Instance.InitMaterials();
                    if(_sharedShaders != null && _sharedShaders.Length > 0)
                    {
                        _runtimeMaterials = new Material[_sharedShaders.Length];
                        string shaderName;
                        for(int i = 0; i < _sharedShaders.Length; i++)
                        {
                            if(_sharedShaders[i] == null)
                                continue;
                            
                            shaderName = _sharedShaders[i];
                            if(shaderName == SVGShader.SolidColorOpaque.name)
                            {
                                _runtimeMaterials[i] = SVGAtlas.Instance.opaqueSolid;
                            } else if(shaderName == SVGShader.SolidColorAlphaBlended.name)
                            {
                                _runtimeMaterials[i] = SVGAtlas.Instance.transparentSolid;
                            } else if(shaderName == SVGShader.GradientColorOpaque.name)
                            {
                                _runtimeMaterials[i] = SVGAtlas.Instance.opaqueGradient;
                            } else if(shaderName == SVGShader.GradientColorAlphaBlended.name)
                            {
                                _runtimeMaterials[i] = SVGAtlas.Instance.transparentGradient;
                            }
                        }
                    }
                }
                return _runtimeMesh;
            }
        }

        protected UIVertex[] _runtimeUIMesh;
        protected UIVertex[] runtimeUIMesh
        {
            get {
                if(_runtimeUIMesh == null)
                    _runtimeUIMesh = CreateUIMesh(sharedMesh);
                return _runtimeUIMesh;
            }
        }

        /// <summary>
        /// Returns the shared UI Mesh of the SVG Asset. (Read Only)
        /// </summary>
        public UIVertex[] sharedUIMesh
        {
            get {
                #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    _runtimeMesh = runtimeMesh;
                    return runtimeUIMesh;
                } else {
                    _runtimeUIMesh = null;
                    return CreateUIMesh(sharedMesh);
                }
                #else
                _runtimeMesh = runtimeMesh;
                return runtimeUIMesh;
                #endif
            }
        }

        protected Material _sharedUIMaterial;

        /// <summary>
        /// Returns the shared UI Material of the SVG Asset. (Read Only)
        /// </summary>
        public Material sharedUIMaterial
        {
            get {
                #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    _runtimeMesh = runtimeMesh;
                    return SVGAtlas.Instance.ui;
                } else {
                    _runtimeMesh = null;
					return _editor_UIMaterial;
                }

                #else
                _runtimeMesh = runtimeMesh;
                return SVGAtlas.Instance.ui;
                #endif
            }
        }

        /// <summary>
        /// Returns the instanced UI Material of the SVG Asset. (Read Only)
        /// </summary>
        public Material uiMaterial
        {
            get {
                #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    _runtimeMesh = runtimeMesh;
                    return CloneMaterial(SVGAtlas.Instance.ui);
                } else {
                    _runtimeMesh = null;
					return CloneMaterial(_editor_UIMaterial);
                }
                
                #else
                _runtimeMesh = runtimeMesh;
                return CloneMaterial(SVGAtlas.Instance.ui);
                #endif
            }
        }

        protected Material _sharedUIMaskMaterial;

        /// <summary>
        /// Returns the shared UI Mask Material of the SVG Asset. (Read Only)
        /// </summary>
        public Material sharedUIMaskMaterial
        {
            get {
                #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    _runtimeMesh = runtimeMesh;
                    return SVGAtlas.Instance.uiMask;
                } else {
                    _runtimeMesh = null;
					return _editor_UIMaskMaterial;
                }                
                #else
                _runtimeMesh = runtimeMesh;
                return SVGAtlas.Instance.uiMask;
                #endif
            }
        }

        /// <summary>
        /// Returns the instanced UI Mask Material of the SVG Asset. (Read Only)
        /// </summary>
        public Material uiMaskMaterial
        {
            get {
                #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    _runtimeMesh = runtimeMesh;
                    return CloneMaterial(SVGAtlas.Instance.uiMask);
                } else {
                    _runtimeMesh = null;
					return CloneMaterial(_editor_UIMaskMaterial);
                }
                #else
                _runtimeMesh = runtimeMesh;
                return CloneMaterial(SVGAtlas.Instance.uiMask);
                #endif
            }
        }

        protected Material[] _sharedMaterials;

        /// <summary>
        /// Returns the shared materials of the SVG Asset. (Read Only)
        /// </summary>
        public Material[] sharedMaterials
        {
            get {
                #if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    return _runtimeMaterials;
                } else {
					_runtimeMaterials = null;
					return _editor_sharedMaterials;
                }                
                #else
                return _runtimeMaterials;
                #endif
            }
        }

        /// <summary>
        /// Returns the instanced materials of the SVG Asset. (Read Only)
        /// </summary>
        public Material[] materials
        {
            get {
                if(sharedMaterials == null)
                    return null;
                
                int sharedMaterialsLength = sharedMaterials.Length;
                Material[] materials = new Material[sharedMaterialsLength];
                for(int i = 0; i < sharedMaterialsLength; i++)
                {
                    materials[i] = CloneMaterial(sharedMaterials[i]);                    
                }
                return materials;
            }
        }

		const string _GradientColorKey = "_GradientColor";
		const string _GradientShapeKey = "_GradientShape";
		const string _ParamsKey = "_Params";
		public static void AssignMaterialGradients(Material material, Texture2D gradientAtlas, Texture2D gradientShape, int gradientWidth, int gradientHeight)
		{
			if(material == null)
				return;

			if(material.HasProperty(_GradientColorKey))
			{
				material.SetTexture(_GradientColorKey, gradientAtlas);
			}
			if(material.HasProperty(_GradientShapeKey))
			{
				material.SetTexture(_GradientShapeKey, gradientShape);
			}
			if(material.HasProperty(_ParamsKey) && gradientAtlas != null)
			{
				Vector4 materialParams = new Vector4(gradientAtlas.width, gradientAtlas.height, gradientWidth, gradientHeight);
				material.SetVector(_ParamsKey, materialParams);
			}
		}

		public static void AssignMaterialGradients(Material[] materials, Texture2D gradientAtlas, Texture2D gradientShape, int gradientWidth, int gradientHeight)
		{
			if(materials == null || materials.Length == 0)
				return;

			for(int i = 0; i < materials.Length; i++)
			{
				AssignMaterialGradients(materials[i], gradientAtlas, gradientShape, gradientWidth, gradientHeight);
			}
		}

        protected Material[] _runtimeMaterials;   

        protected Texture2D[] _atlasTextures;

        /// <summary>
        /// Returns the references to the used gradient textures of the SVG Asset. (Read Only)
        /// </summary>
        public Texture2D[] atlasTextures
        {
            get {
#if UNITY_EDITOR
				if(UnityEditor.EditorApplication.isPlaying)
                {
					return _atlasTextures = SVGAtlas.Instance.atlasTextures.ToArray();
				} else {
					if(_atlasTextures == null || _atlasTextures.Length == 0)
					{
						Texture2D atlasTexture = SVGAtlas.GenerateGradientAtlasTexture(_sharedGradients, 64, 4);
						if(atlasTexture != null)
						{
							atlasTexture.hideFlags = HideFlags.DontSave;
							_atlasTextures = new Texture2D[]{atlasTexture};
						} else {
							_atlasTextures = new Texture2D[]{SVGAtlas.whiteTexture};
						}
					}
				}
#else
                return _atlasTextures = SVGAtlas.Instance.atlasTextures.ToArray();
#endif
                return _atlasTextures;
            }
        }

        [FormerlySerializedAs("antialiasing")]
        [SerializeField]
        protected bool _antialiasing = false;
        /// <summary>
        /// Use antialiasing (Read Only)
        /// </summary>
        public bool antialiasing
        {
            get {
                return _antialiasing;
            }
        }

        [FormerlySerializedAs("antialiasingWidth")]
        [SerializeField]
        protected float _antialiasingWidth = 0f;
        /// <summary>
        /// Antialiasing width, zero value turns antialiasing off (Read Only)
        /// </summary>
        public float antialiasingWidth
        {
            get {
                return _antialiasingWidth;
            }
        }

        [FormerlySerializedAs("generateCollider")]
        [SerializeField]
        protected bool _generateCollider = false;        
        /// <summary>
        /// Returns if the asset has generated collider shape. (Read Only)
        /// </summary>
        public bool generateCollider
        {
            get {
                return _generateCollider;
            }
        }

        [FormerlySerializedAs("keepSVGFile")]
        [SerializeField]
        protected bool _keepSVGFile = true;
        /// <summary>
        /// Keep the SVG file in the final build (Read Only)
        /// </summary>
        public bool keepSVGFile
        {
            get {
                return _keepSVGFile;
            }
        }

        [FormerlySerializedAs("ignoreSVGCanvas")]
        [SerializeField]
        protected bool _ignoreSVGCanvas = true;
        /// <summary>
        /// Trim the document canvas to object bounding box (Read Only)
        /// </summary>
        public bool ignoreSVGCanvas
        {
            get {
                return _ignoreSVGCanvas;
            }
        }

        [FormerlySerializedAs("colliderShape")]
        [SerializeField]
        protected SVGPath[] _colliderShape;
        /// <summary>
        /// Returns the collider shape. (Read Only)
        /// </summary>
        public SVGPath[] colliderShape
        {
            get {
                return _colliderShape;
            }
        }

        [FormerlySerializedAs("format")]
        [SerializeField]
        protected SVGAssetFormat _format = SVGAssetFormat.Transparent;
        /// <summary>
        /// Returns the rendering format of the SVG Asset. (Read Only)
        /// </summary>
        public SVGAssetFormat format
        {
            get {
                return _format;
            }
        }
        
        [FormerlySerializedAs("useGradients")]
        [SerializeField]
        protected SVGUseGradients _useGradients = SVGUseGradients.Always;
        /// <summary>
        /// Returns if the mesh was compressed. (Read Only)
        /// </summary>
        public SVGUseGradients useGradients
        {
            get {
                return _useGradients;
            }
        }

        [FormerlySerializedAs("meshCompression")]
        [SerializeField]
        protected SVGMeshCompression _meshCompression = SVGMeshCompression.Off;
        /// <summary>
        /// Returns if the mesh was compressed. (Read Only)
        /// </summary>
        public SVGMeshCompression meshCompression
        {
            get {
                return _meshCompression;
            }
        }

        [FormerlySerializedAs("optimizeMesh")]
        [SerializeField]
        protected bool _optimizeMesh = true;        
        /// <summary>
        /// Returns if the mesh is optimised for GPU. (Read Only)
        /// </summary>
        public bool optimizeMesh
        {
            get {
                return _optimizeMesh;
            }
        }

        [FormerlySerializedAs("generateNormals")]
        [SerializeField]
        protected bool _generateNormals = false;
        /// <summary>
        /// Returns if the mesh contains normals. (Read Only)
        /// </summary>
        public bool generateNormals
        {
            get {
                return _generateNormals;
            }
        }

        [FormerlySerializedAs("generateTangents")]
        [SerializeField]
        protected bool _generateTangents = false;        
        /// <summary>
        /// Returns if the mesh contains tangents. (Read Only)
        /// </summary>
        public bool generateTangents
        {
            get {
                return _generateTangents;
            }
        }

        [FormerlySerializedAs("scale")]
        [SerializeField]
        protected float _scale = 0.01f;
        /// <summary>
        /// Returns the scale of the mesh relative to the SVG Asset. (Read Only)
        /// </summary>
        public float scale {
            get {
                return _scale;
            }
        }

        [FormerlySerializedAs("vpm")]
        [SerializeField]
        protected float _vpm = 1000f;
        /// <summary>
        /// Returns the number of vertices in the SVG Asset that correspond to one unit in world space. (Read Only)
        /// </summary>
        public float vpm
        {
            get {
                return _vpm;
            }
        }

        [FormerlySerializedAs("depthOffset")]
        [SerializeField]
        protected float _depthOffset = 0.01f;
        /// <summary>
        /// Returns the minimal z-offset in WorldSpace for Opaque Rendering. (Read Only)
        /// </summary>
        public float depthOffset
        {
            get {
                return _depthOffset;
            }
        }

        [FormerlySerializedAs("compressDepth")]
        [SerializeField]
        protected bool _compressDepth = true;
        /// <summary>
        /// Returns the compress overlapping objects to reduce z-offset requirements. (Read Only)
        /// </summary>
        public bool compressDepth
        {
            get {
                return _compressDepth;
            }
        }

        [FormerlySerializedAs("pivotPoint")]
        [SerializeField]
        protected Vector2 _pivotPoint = new Vector2(0.5f, 0.5f);
        /// <summary>
        /// Returns the location of the SVG Asset center point in the original Rect, specified in percents. (Read Only)
        /// </summary>
        public Vector2 pivotPoint
        {
            get {
                return _pivotPoint;
            }
        }

        [FormerlySerializedAs("customPivotPoint")]
        [SerializeField]
        protected bool _customPivotPoint = false;
        /// <summary>
        /// Returns the use of predefined pivot point or custom pivot point. (Read Only)
        /// </summary>
        public bool customPivotPoint
        {
            get {
                return _customPivotPoint;
            }
        }

		[FormerlySerializedAs("border")]
		[SerializeField]
		protected Vector4 _border = new Vector4(0f, 0f, 0f, 0f);		
		/// <summary>
		/// Returns the 9-slice border. (Read Only)
        /// LEFT, BOTTOM, RIGHT, TOP
		/// </summary>
		public Vector4 border
		{
			get {
				return _border;
			}
		}

        [FormerlySerializedAs("sliceMesh")]
        [SerializeField]
        protected bool _sliceMesh = false;        
        /// <summary>
        /// Returns if the mesh is sliced. (Read Only)
        /// </summary>
        public bool sliceMesh
        {
            get {
                return _sliceMesh;
            }
        }

        protected string _svgFile;
        /// <summary>
        /// Returns the original SVG text content available only in the Editor. (Read Only)
        /// </summary>
        public string svgFile
        {
            get {
                if(!string.IsNullOrEmpty(_svgFile))
                {
                    return _svgFile;
                } else {
                    if(_documentAsset != null)
                    {
                        return _documentAsset.svgFile;
                    } else {
                        return null;
                    }
                }
            }
        }

        [FormerlySerializedAs("sharedGradients")]
        [SerializeField]
        protected CCGradient[] _sharedGradients;
        /// <summary>
        /// Returns all the used gradients in the SVG Asset. (Read Only)
        /// </summary>
        public CCGradient[] sharedGradients {
            get {
                return _sharedGradients;
            }
        }

        [FormerlySerializedAs("sharedShaders")]
        [SerializeField]
        protected string[] _sharedShaders;
        /// <summary>
        /// Returns all the used shader names in the SVG Asset. (Read Only)
        /// </summary>
        public string[] sharedShaders {
            get {
                return _sharedShaders;
            }
        }

        /// <summary>
        /// Returns the bounding volume of the mesh of the SVG Asset. (Read Only)
        /// </summary>
        public Bounds bounds
        {
            get {
                if(_sharedMesh == null)
                    return new Bounds();

                return _sharedMesh.bounds;
            }
        }

        [FormerlySerializedAs("canvasRectangle")]
        [SerializeField]
        protected Rect _canvasRectangle;
        /// <summary>
        /// Returns the Original Canvas rectangle of the SVG Asset. (Read Only)
        /// </summary>
        public Rect canvasRectangle
        {
            get {
                return _canvasRectangle;
            }
        }

        [FormerlySerializedAs("layers")]
        [SerializeField]
        protected SVGLayer[] _layers;
        /// <summary>
        /// Returns the SVG layers. (Read Only)
        /// </summary>
        public SVGLayer[] layers
        {
            get {
                return _layers;
            }
        }

        /// <summary>
        /// Returns if the SVG Asset contains any gradients. (Read Only)
        /// </summary>
        public bool hasGradients
        {
            get {
                if(_sharedGradients == null || _sharedGradients.Length == 0) return false;
                return true;
            }
        }

        protected Material CloneMaterial(Material original)
        {
            if(original == null)
                return null;

            Material material = new Material(original.shader);
            material.CopyPropertiesFromMaterial(original);
            return material;
        }

        /// <summary>
        /// Returns the number of vertices in the mesh of the SVG Asset. (Read Only)
        /// </summary>
        public int uiVertexCount
        {
            get {
                if(_sharedMesh == null || _sharedMesh.triangles == null)
                    return 0;

                int trianglesCount = _sharedMesh.triangles.Length;
                return trianglesCount + (trianglesCount / 3);
            }
        }

        protected static UIVertex[] CreateUIMesh(Mesh inputMesh)
        {
            if(inputMesh == null) return new UIVertex[0];
            
            Vector3[] vertices = inputMesh.vertices;
            Color32[] colors = inputMesh.colors32;
            Vector2[] uv = inputMesh.uv;
            Vector2[] uv2 = inputMesh.uv2;
            Vector3[] normals = inputMesh.normals;
            Vector4[] tangents = inputMesh.tangents;

            int[] triangles = inputMesh.triangles;
            int trianglesCount = triangles.Length;
            UIVertex[] sharedUIMesh = new UIVertex[trianglesCount + (trianglesCount / 3)];
            
            UIVertex vertex = new UIVertex();        
            int currentQuad = 0;
            int currentTriangle = 0;

            for(int i = 0; i < trianglesCount; i += 3)
            {
                currentTriangle = triangles[i];
                vertex.position = vertices[currentTriangle];
                vertex.color = colors[currentTriangle];
                sharedUIMesh[currentQuad++] = vertex;
                
                currentTriangle = triangles[i + 1];
                vertex.position = vertices[currentTriangle];
                vertex.color = colors[currentTriangle];
                sharedUIMesh[currentQuad++] = vertex;
                
                currentTriangle = triangles[i + 2];
                vertex.position = vertices[currentTriangle];
                vertex.color = colors[currentTriangle];
                sharedUIMesh[currentQuad++] = sharedUIMesh[currentQuad++] = vertex;                    
            }

            if(uv != null && uv.Length > 0 && uv2 != null && uv2.Length > 0)
            {
                currentQuad = 0;
                currentTriangle = 0;

                for(int i = 0; i < trianglesCount; i += 3)
                {
                    currentTriangle = triangles[i];
                    vertex = sharedUIMesh[currentQuad];
                    vertex.uv0 = uv[currentTriangle];
                    vertex.uv1 = uv2[currentTriangle];
                    sharedUIMesh[currentQuad++] = vertex;
                    
                    currentTriangle = triangles[i + 1];
                    vertex = sharedUIMesh[currentQuad];
                    vertex.uv0 = uv[currentTriangle];
                    vertex.uv1 = uv2[currentTriangle];
                    sharedUIMesh[currentQuad++] = vertex;
                    
                    currentTriangle = triangles[i + 2];
                    vertex = sharedUIMesh[currentQuad];
                    vertex.uv0 = uv[currentTriangle];
                    vertex.uv1 = uv2[currentTriangle];
                    sharedUIMesh[currentQuad++] = sharedUIMesh[currentQuad++] = vertex;                    
                }
            }

            if(normals != null && normals.Length > 0)
            {
                currentQuad = 0;
                currentTriangle = 0;

                for(int i = 0; i < trianglesCount; i += 3)
                {
                    currentTriangle = triangles[i];
                    vertex = sharedUIMesh[currentQuad];
                    vertex.normal = normals[currentTriangle];
                    sharedUIMesh[currentQuad++] = vertex;
                    
                    currentTriangle = triangles[i + 1];
                    vertex = sharedUIMesh[currentQuad];
                    vertex.normal = normals[currentTriangle];
                    sharedUIMesh[currentQuad++] = vertex;
                    
                    currentTriangle = triangles[i + 2];
                    vertex = sharedUIMesh[currentQuad];
                    vertex.normal = normals[currentTriangle];
                    sharedUIMesh[currentQuad++] = sharedUIMesh[currentQuad++] = vertex;                    
                }
            }

            if(tangents != null && tangents.Length > 0)
            {
                currentQuad = 0;
                currentTriangle = 0;

                for(int i = 0; i < trianglesCount; i += 3)
                {
                    currentTriangle = triangles[i];
                    vertex.tangent = tangents[currentTriangle];
                    sharedUIMesh[currentQuad++] = vertex;
                    
                    currentTriangle = triangles[i + 1];                        
                    vertex.tangent = tangents[currentTriangle];
                    sharedUIMesh[currentQuad++] = vertex;
                    
                    currentTriangle = triangles[i + 2];
                    vertex.tangent = tangents[currentTriangle];
                    sharedUIMesh[currentQuad++] = sharedUIMesh[currentQuad++] = vertex;                    
                }
            }
            
            return (UIVertex[])sharedUIMesh.Clone();
        }

        /// <summary>Load SVG at runtime. (Slow Method).
        /// <para>svgText represents the SVG string content</para>
        /// <para>settings holds all the SVG settings</para>
        /// </summary>
        public static SVGAsset Load(string svgText, SVGImporterSettings settings = null)
        {
#if UNITY_EDITOR
            if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("SVG Asset Load works only in playmode!");
                return null;
            }
#endif
            if(string.IsNullOrEmpty(svgText)) return null;

            if(settings == null)
            {
                SVGAssetImport.format = SVGAssetFormat.Transparent;
                SVGAssetImport.pivotPoint = new Vector2(0.5f, 0.5f);
                SVGAssetImport.meshScale = 0.01f;
                SVGAssetImport.border = new Vector4(0f, 0f, 0f, 0f);
                SVGAssetImport.sliceMesh = false;
                SVGAssetImport.minDepthOffset = 0.01f;
                SVGAssetImport.compressDepth = true;
                SVGAssetImport.ignoreSVGCanvas = true;
                SVGAssetImport.useGradients = SVGUseGradients.Always;            
                SVGAssetImport.antialiasingWidth = 0f;
            } else {
                SVGAssetImport.format = settings.defaultSVGFormat;
                SVGAssetImport.pivotPoint = settings.defaultPivotPoint;
                SVGAssetImport.meshScale = settings.defaultScale;
                SVGAssetImport.border = new Vector4(0f, 0f, 0f, 0f);
                SVGAssetImport.sliceMesh = false;
                SVGAssetImport.minDepthOffset = settings.defaultDepthOffset;
                SVGAssetImport.compressDepth = settings.defaultCompressDepth;
                SVGAssetImport.ignoreSVGCanvas = settings.defaultIgnoreSVGCanvas;
                SVGAssetImport.useGradients = settings.defaultUseGradients;
                if(settings.defaultAntialiasing && settings.defaultAntialiasingWidth != 0f)
                {
                    SVGAssetImport.antialiasingWidth = settings.defaultAntialiasingWidth;
                } else {
                    SVGAssetImport.antialiasingWidth = 0f;
                }
            }

            SVGGraphics graphics = new SVGGraphics(1000f);
            SVGDocument svgDocument = null;

            SVGAssetImport.Clear();
            SVGAssetImport.atlasData = new SVGAtlasData();
            SVGParser.Init();
            SVGGraphics.Init();
            
            SVGElement rootSVGElement = null;
            List<SVGError> errors = new List<SVGError>();

            //try {
                // Create new Asset
                svgDocument = new SVGDocument(svgText, graphics);
                rootSVGElement = svgDocument.rootElement;
            /*
            } catch (System.Exception exception) {
                rootSVGElement = null;
                errors.Add(SVGError.Syntax);
                Debug.LogError("SVG Document Exception: "+exception.Message);
                return null;
            }
            */

            if(rootSVGElement == null)
            {
                Debug.LogError("SVG Document is corrupted!");
                return null;
            }

            SVGGraphics.depthTree = new SVGDepthTree(rootSVGElement.paintable.viewport);
            SVGAsset asset = ScriptableObject.CreateInstance<SVGAsset>();

            asset._antialiasing = SVGAssetImport.antialiasingWidth != 0f;
            asset._antialiasingWidth = SVGAssetImport.antialiasingWidth;
            asset._border = SVGAssetImport.border;
            asset._compressDepth = SVGAssetImport.compressDepth;
            asset._depthOffset = SVGAssetImport.minDepthOffset;
            asset._ignoreSVGCanvas = SVGAssetImport.ignoreSVGCanvas;
            asset._meshCompression = SVGMeshCompression.Off;
            asset._scale = SVGAssetImport.meshScale;
            asset._format = SVGAssetImport.format;
            asset._useGradients = SVGAssetImport.useGradients;
            asset._pivotPoint = SVGAssetImport.pivotPoint;
            asset._vpm = SVGAssetImport.vpm;

            if(settings != null)
            {
                asset._generateCollider = settings.defaultGenerateCollider;
                asset._generateNormals = settings.defaultGenerateNormals;
                asset._generateTangents = settings.defaultGenerateTangents;
                asset._sliceMesh = false;
                asset._optimizeMesh = settings.defaultOptimizeMesh;
                asset._keepSVGFile = settings.defaultKeepSVGFile;
            } else {
                asset._generateCollider = false;
                asset._generateNormals = false;
                asset._generateTangents = false;
                asset._sliceMesh = false;
                asset._optimizeMesh = true;
                asset._keepSVGFile = false;
            }

            try {
                rootSVGElement.Render();
                
                // Handle gradients
                bool hasGradients = (asset.useGradients == SVGUseGradients.Always);

                // Create actual Mesh
                Shader[] outputShaders;
                SVGLayer[] outputLayers;
                Mesh mesh = SVGMesh.CombineMeshes(SVGGraphics.meshes, out outputLayers, out outputShaders, asset._useGradients, asset._format, asset._compressDepth);
                if(mesh == null)
                    return null;

                if(outputShaders != null)
                {
                    for(int i = 0; i < outputShaders.Length; i++)
                    {
                        if(outputShaders[i] == null) continue;
                        if(outputShaders[i].name == SVGShader.GradientColorOpaque.name ||
                           outputShaders[i].name == SVGShader.GradientColorAlphaBlended.name)
                        {
                            hasGradients = true;
                            break;
                        }
                    }
                }

                Vector3[] vertices = mesh.vertices;
                Vector2 offset;
                Bounds bounds = mesh.bounds;
                Rect viewport = rootSVGElement.paintable.viewport;
                viewport.x *= SVGAssetImport.meshScale;
                viewport.y *= SVGAssetImport.meshScale;
                viewport.size *= SVGAssetImport.meshScale;
                
                if(SVGAssetImport.ignoreSVGCanvas)
                {
                    offset = new Vector2(bounds.min.x + bounds.size.x * SVGAssetImport.pivotPoint.x,
                                         bounds.min.y + bounds.size.y * SVGAssetImport.pivotPoint.y);
                } else {
                    offset = new Vector2(viewport.min.x + viewport.size.x * SVGAssetImport.pivotPoint.x,
                                         viewport.min.y + viewport.size.y * SVGAssetImport.pivotPoint.y);                        
                }
                
                // Apply pivot point and Flip Y Axis
                for(int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x = vertices[i].x - offset.x;
                    vertices[i].y = (vertices[i].y - offset.y) * -1f;
                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                asset._sharedMesh = mesh;

                if(outputShaders != null && outputShaders.Length > 0)
                {
                    asset._sharedShaders = new string[outputShaders.Length];
                    if(hasGradients)
                    {
                        for(int i = 0; i < outputShaders.Length; i++)
                        {
                            asset._sharedShaders[i] = outputShaders[i].name;
                        }
                    } else {
                        for(int i = 0; i < outputShaders.Length; i++)
                        {
                            if(outputShaders[i].name == SVGShader.GradientColorAlphaBlended.name)
                            {
                                outputShaders[i] = SVGShader.SolidColorAlphaBlended;
                            } else if(outputShaders[i].name == SVGShader.GradientColorOpaque.name)
                            {
                                outputShaders[i] = SVGShader.SolidColorOpaque;                                
                            }
                            asset._sharedShaders[i] = outputShaders[i].name;
                        }
                    }
                }

                // Handle Canvas Rectangle
                asset._canvasRectangle = new Rect(viewport.x, viewport.y, viewport.size.x, viewport.size.y);
                
                if(asset.generateCollider)
                {
                    // Create polygon contour
                    if(SVGGraphics.paths != null && SVGGraphics.paths.Count > 0)
                    {
                        List<List<Vector2>> polygons = new List<List<Vector2>>();
                        for(int i = 0; i < SVGGraphics.paths.Count; i++)
                        {
                            Vector2[] points = SVGGraphics.paths[i].points;
                            for(int j = 0; j < points.Length; j++)
                            {
                                points[j].x = points[j].x * SVGAssetImport.meshScale  - offset.x;
                                points[j].y = (points[j].y * SVGAssetImport.meshScale  - offset.y) * -1f;
                            }
                            
                            polygons.Add(new List<Vector2>(points));
                        }
                        
                        polygons = SVGGeom.MergePolygon(polygons);
                        
                        SVGPath[] paths = new SVGPath[polygons.Count];
                        for(int i = 0; i < polygons.Count; i++)
                        {
                            paths[i] = new SVGPath(polygons[i].ToArray());
                        }

                        if(paths != null && paths.Length > 0)
                        {
                            asset._colliderShape = paths;
                        }
                    }
                }

                if(hasGradients)
                {
                    List<CCGradient> gradients = SVGAssetImport.atlasData.gradients;
                    if(gradients != null && gradients.Count > 0)
                    {
                        asset._sharedGradients = gradients.ToArray();
                    }
                }
            } catch(System.Exception exception) {
                Debug.LogWarning("Asset Failed to import\n"+exception.Message);
                errors.Add(SVGError.CorruptedFile);
            }

            asset._documentAsset = SVGDocumentAsset.CreateInstance(svgText, errors.ToArray());
            if(svgDocument != null) svgDocument.Clear();
            SVGAssetImport.Clear();
            return asset;
        }

        void OnDisable()
        {
    		
        }
    	
        void OnDestroy()
        {
    		
        }	

    #if UNITY_EDITOR

        internal SVGDocumentAsset _editor_documentAsset
        {
            get {
                return _documentAsset;
            }
        }

        internal void _editor_ApplyChanges(bool importMultipleFiles = false)
        {
            if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
                return;

            if(_documentAsset != null)
            {
                _documentAsset.errors = null;
            }

            if(_sharedShaders != null)
                _sharedShaders = null;

            if(_sharedMesh != null)
            {
                Object.DestroyImmediate(_sharedMesh, true);
                _sharedMesh = null;
            }

            if(_atlasTextures != null && _atlasTextures.Length > 0)
            {
                for( int i = 0; i < _atlasTextures.Length; i++)
                {
                    if(_atlasTextures[i] == null)
                        continue;
                    
                    Object.DestroyImmediate(_atlasTextures[i], true);
                    _atlasTextures[i] = null;
                }
                _atlasTextures = null;
            }

            if(_sharedUIMaterial != null)
            {
                Object.DestroyImmediate(_sharedUIMaterial, true);
                _sharedUIMaterial = null;
            }

            if(_sharedUIMaskMaterial != null)
            {
                Object.DestroyImmediate(_sharedUIMaskMaterial, true);
                _sharedUIMaskMaterial = null;
            }

            if(_sharedMaterials != null && _sharedMaterials.Length > 0)
            {
                for( int i = 0; i < sharedMaterials.Length; i++)
                {
                    if(_sharedMaterials[i] == null)
                        continue;
                    
                    Object.DestroyImmediate(_sharedMaterials[i], true);
                    _sharedMaterials[i] = null;
                }
                _sharedMaterials = null;
            }

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if(assets != null && assets.Length > 0)
            {
                for(int i = 0; i < assets.Length; i++)
                {
                    if(assets[i] == null)
                        continue;
                    if(assets[i] == this)
                        continue;

                    if(assets[i] is SVGDocumentAsset)
                        continue;

                    DestroyImmediate(assets[i], true);
                }
            }

            _editor_LoadSVG();        

            // Create Document Asset
            if(_documentAsset == null)
            {
                _documentAsset = AddObjectToAsset<SVGDocumentAsset>(ScriptableObject.CreateInstance<SVGDocumentAsset>(), this, HideFlags.HideInHierarchy);
            }

            var svgAssetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var svgAssetImporter = UnityEditor.AssetImporter.GetAtPath(svgAssetPath);

            if(!string.IsNullOrEmpty(svgFile))
            {
                svgAssetImporter.userData = svgFile;
            }

            if(keepSVGFile)
            {
                _documentAsset.svgFile = svgAssetImporter.userData;
            } else {
                _documentAsset.svgFile = null;
            }

            if(SVGAssetImport.errors != null && SVGAssetImport.errors.Count > 0)
            {
                _documentAsset.errors = SVGAssetImport.errors.ToArray();

				bool critical = false;
				string errors = "";
				int errorsLength = _documentAsset.errors.Length;
				for(int i = 0; i < errorsLength; i++)
				{
					if(i < errorsLength - 1)
					{
						errors += _documentAsset.errors[i].ToString() +", ";
					} else {
						errors += _documentAsset.errors[i].ToString() +".";
					}

					if(_documentAsset.errors[i] == SVGError.CorruptedFile || 
					   _documentAsset.errors[i] == SVGError.Syntax)
					{
						critical = true;
					}
				}

				if(critical)
				{
					Debug.LogError ("SVGAsset: "+this.name+"\nerrors: "+errors+"\npath: "+UnityEditor.AssetDatabase.GetAssetPath(this)+"\n", this);
				} else {
					Debug.LogWarning ("SVGAsset: "+this.name+"\nerrors: "+errors+"\npath: "+UnityEditor.AssetDatabase.GetAssetPath(this)+"\n", this);
				}
            }

            UnityEditor.EditorUtility.SetDirty(_documentAsset);

            _svgFile = null;

            if(SVGAssetImport.errors != null)
            {
                SVGAssetImport.errors.Clear();
                SVGAssetImport.errors = null;
            }

            if(_sharedMesh != null && _sharedMesh.vertexCount > 0)
            {
                _sharedMesh.name = this.name;

                int vertexCount = _sharedMesh.vertexCount;
                UnityEditor.MeshUtility.SetMeshCompression(_sharedMesh, GetModelImporterMeshCompression(_meshCompression));
                if(_optimizeMesh) _sharedMesh.Optimize();
                if(_generateNormals)
                {
                    Vector3[] normals = new Vector3[vertexCount];
                    for(int i = 0; i < vertexCount; i++)
                    {
                        normals[i] = -Vector3.forward;
                    }
                    _sharedMesh.normals = normals;
                    if(_generateTangents)
                    {
                        Vector4[] tangents = new Vector4[vertexCount];
                        for(int i = 0; i < vertexCount; i++)
                        {
                            tangents[i] = new Vector4(-1f, 0f, 0f, -1f);
                        }
                        _sharedMesh.tangents = tangents;
                    }
                }
            }

            _lastTimeModified = System.DateTime.UtcNow.Ticks;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        internal UnityEditor.ModelImporterMeshCompression GetModelImporterMeshCompression(SVGMeshCompression meshCompression)
        {
            switch(meshCompression)
            {
                case SVGMeshCompression.Low:
                return UnityEditor.ModelImporterMeshCompression.Low;
                case SVGMeshCompression.Medium:
                    return UnityEditor.ModelImporterMeshCompression.Medium;
                case SVGMeshCompression.High:
                    return UnityEditor.ModelImporterMeshCompression.High;
            }

            return UnityEditor.ModelImporterMeshCompression.Off;
        }

        internal void _editor_SetGradients(CCGradient[] gradients)
        {
            if(gradients == null || gradients.Length == 0)
            {
                _sharedGradients = null;
                return;
            }
            
            _sharedGradients = new CCGradient[gradients.Length];
            for(int i = 0; i < gradients.Length; i++)
            {
                _sharedGradients[i] = gradients[i].Clone();
            }
        }

        internal void _editor_SetColliderShape(SVGPath[] shape)
        {
            _colliderShape = shape;
        }

        internal void _editor_SetCanvasRectangle(Rect rectangle)
        {
            _canvasRectangle = rectangle;
        }

        internal void _editor_LoadSVG()
        {        
            SVGAssetImport assetImport;
            if (svgFile != null)
            {
                SVGAssetImport.format = _format;
                SVGAssetImport.meshScale = _scale;
                SVGAssetImport.border = _border;
                SVGAssetImport.sliceMesh = _sliceMesh;
                SVGAssetImport.minDepthOffset = _depthOffset;
                SVGAssetImport.compressDepth = _compressDepth;
                SVGAssetImport.ignoreSVGCanvas = _ignoreSVGCanvas;
                SVGAssetImport.useGradients = _useGradients;

                if(_antialiasing)
                {
                    SVGAssetImport.antialiasingWidth = _antialiasingWidth;
                } else {
                    SVGAssetImport.antialiasingWidth = 0f;
                }

                assetImport = new SVGAssetImport(svgFile, _vpm);
                assetImport.StartProcess(this);
            }
        }

        internal T AddObjectToAsset<T>(T obj, SVGAsset asset, HideFlags hideFlags) where T : UnityEngine.Object
        {
            if(obj == null)
                return null;
            
            obj.hideFlags = hideFlags;
            UnityEditor.AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        internal Mesh _editor_sharedMesh
        {
            get {
                return _sharedMesh;
            }
        }

		internal Material[] _editor_sharedMaterials
		{
			get {
				if(_sharedMaterials == null || _sharedMaterials.Length == 0)
				{
					_sharedMaterials = new Material[_sharedShaders.Length];
					for(int i = 0; i < _sharedShaders.Length; i++)
					{
						_sharedMaterials[i] = new Material(Shader.Find(_sharedShaders[i]));
						_sharedMaterials[i].hideFlags = HideFlags.DontSave;
					}
                    if(hasGradients)
                    {
					    AssignMaterialGradients(_sharedMaterials, atlasTextures[0], SVGAtlas.gradientShapeTexture, 64, 4);
                    }
	            }
	            return _sharedMaterials;
			}
        }
		
		internal Material _editor_UIMaterial
		{
			get {
				if(_sharedUIMaterial == null)
				{
					_sharedUIMaterial = new Material(SVGShader.UI);
					_sharedUIMaterial.hideFlags = HideFlags.DontSave;
					AssignMaterialGradients(_sharedUIMaterial, atlasTextures[0], SVGAtlas.gradientShapeTexture, 64, 4);
				}
				return _sharedUIMaterial;
			}
		}

		internal Material _editor_UIMaskMaterial
		{
			get {
				if(_sharedUIMaskMaterial == null)
				{
					_sharedUIMaskMaterial = new Material(SVGShader.UIMask);
					_sharedUIMaskMaterial.hideFlags = HideFlags.DontSave;
					AssignMaterialGradients(_sharedUIMaskMaterial, atlasTextures[0], SVGAtlas.gradientShapeTexture, 64, 4);
				}
				return _sharedUIMaskMaterial;
			}
		}

        internal string _editor_Info
        {
            get {
                if(_sharedMesh == null)
                {
                    return "No info available";
                }
                
                string output;
                int totalVertices = _sharedMesh.vertexCount;
                int totalTriangles = _sharedMesh.triangles.Length / 3;
                
                output = string.Format("{0} Vertices, {1} Triangles", totalVertices, totalTriangles);

                var fileInfo = new System.IO.FileInfo(UnityEditor.AssetDatabase.GetAssetPath(this));
                if(fileInfo != null)
                {
                    output += ", FileSize: "+string.Format(new FileSizeFormatProvider(), "{0:fs}", fileInfo.Length);
                }
                return output;
            }
        }

        internal SVGError[] _editor_errors
        {
            get {
                if(_documentAsset == null)
                    return null;

                return _documentAsset.errors;
            }
        }
    #endif
    }
}
