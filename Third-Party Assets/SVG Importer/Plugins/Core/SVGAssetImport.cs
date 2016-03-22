// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#define IGNORE_EXCEPTIONS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter 
{
    using Document;
    using Rendering;
    using Geometry;
    using Data;
    using Utils;

    public class SVGAssetImport
    {    
        public static SVGAtlasData atlasData;

        public static List<SVGError> errors;	
        protected static bool _importingSVG = false;
        public static bool importingSVG
        {
            get {
                return _importingSVG;
            }
        }

        public static SVGUseGradients useGradients;
        public static float antialiasingWidth;
        public static float vpm;
        public static Vector2 pivotPoint;
        public static bool ignoreSVGCanvas;
        public static float meshScale = 1f;        
        public static Vector4 border;
        public static bool sliceMesh = false;
        public static float minDepthOffset = 0.001f;
        public static SVGAssetFormat format = SVGAssetFormat.Opaque;
        public static bool compressDepth = true;

        private string _SVGFile;
        private Texture2D _texture = null;
        private SVGGraphics _graphics;
        private SVGDocument _svgDocument;
    
        public SVGAssetImport(string svgFile, float vertexPerMeter = 1000f)
        {
            vpm = vertexPerMeter;
            this._SVGFile = svgFile;
            _graphics = new SVGGraphics(vertexPerMeter);
        }

        private void CreateEmptySVGDocument()
        {
            _svgDocument = new SVGDocument(_SVGFile, this._graphics);
        }

#if UNITY_EDITOR
        public void StartProcess(SVGAsset asset)
        {
            if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if(errors == null)
            {
                errors = new List<SVGError>();
            } else {
                errors.Clear();
            }
            _importingSVG = true;

            UnityEditor.SerializedObject svgAsset = new UnityEditor.SerializedObject(asset);
            UnityEditor.SerializedProperty sharedMesh = svgAsset.FindProperty("_sharedMesh");            
            UnityEditor.SerializedProperty sharedShaders = svgAsset.FindProperty("_sharedShaders");

            Clear();
            SVGParser.Init();
            SVGGraphics.Init();
            atlasData = new SVGAtlasData();
            SVGElement _rootSVGElement = null;

#if IGNORE_EXCEPTIONS
            try {
#else
                Debug.LogWarning("Exceptions are turned on!");
#endif
                // Create new Asset
                CreateEmptySVGDocument();
                _rootSVGElement = this._svgDocument.rootElement;
#if IGNORE_EXCEPTIONS
            } catch (System.Exception exception) {
                _rootSVGElement = null;
                errors.Add(SVGError.Syntax);
                Debug.LogError("SVG Document Exception: "+exception.Message, asset);
            }
#endif

            if(_rootSVGElement == null)
            {
                Debug.LogError("SVG Document is corrupted! "+UnityEditor.AssetDatabase.GetAssetPath(asset), asset);
                _importingSVG = false;
                return;
            }

            SVGGraphics.depthTree = new SVGDepthTree(_rootSVGElement.paintable.viewport);

#if IGNORE_EXCEPTIONS
            try {
#endif
                _rootSVGElement.Render();

                // Handle gradients
                bool hasGradients = (useGradients == SVGUseGradients.Always);

                // Create actual Mesh
                Shader[] outputShaders;
                SVGLayer[] outputLayers;
                Mesh mesh = SVGMesh.CombineMeshes(SVGGraphics.meshes, out outputLayers, out outputShaders, useGradients, format, compressDepth);
                if(mesh == null)
                    return;

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
                Rect viewport = _rootSVGElement.paintable.viewport;
                viewport.x *= SVGAssetImport.meshScale;
                viewport.y *= SVGAssetImport.meshScale;
                viewport.size *= SVGAssetImport.meshScale;

                if(asset.ignoreSVGCanvas)
                {
                    offset = new Vector2(bounds.min.x + bounds.size.x * asset.pivotPoint.x,
                                         bounds.min.y + bounds.size.y * asset.pivotPoint.y);
                } else {
                    offset = new Vector2(viewport.min.x + viewport.size.x * asset.pivotPoint.x,
                                         viewport.min.y + viewport.size.y * asset.pivotPoint.y);                        
                }

                // Apply pivot point and Flip Y Axis
				for(int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x = vertices[i].x - offset.x;
                    vertices[i].y = (vertices[i].y - offset.y) * -1f;
                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                sharedMesh.objectReferenceValue = AddObjectToAsset<Mesh>(mesh, asset, HideFlags.HideInHierarchy);

//                Material sharedMaterial;
                if(outputShaders != null && outputShaders.Length > 0)
                {
                    sharedShaders.arraySize = outputShaders.Length;
                    if(hasGradients)
                    {
                        for(int i = 0; i < outputShaders.Length; i++)
                        {
                            sharedShaders.GetArrayElementAtIndex(i).stringValue = outputShaders[i].name;                                                
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
                            sharedShaders.GetArrayElementAtIndex(i).stringValue = outputShaders[i].name;
                        }
                    }
                }

                // Serialize the Asset
                svgAsset.ApplyModifiedProperties();

                // Handle Canvas Rectangle
                System.Reflection.MethodInfo _editor_SetCanvasRectangle = typeof(SVGAsset).GetMethod("_editor_SetCanvasRectangle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                _editor_SetCanvasRectangle.Invoke(asset, new object[]{new Rect(viewport.x, viewport.y, viewport.size.x, viewport.size.y)});

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

                        System.Reflection.MethodInfo _editor_SetColliderShape = typeof(SVGAsset).GetMethod("_editor_SetColliderShape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if(paths != null && paths.Length > 0)
                        {
                            _editor_SetColliderShape.Invoke(asset, new object[]{paths});
                        } else {
                            _editor_SetColliderShape.Invoke(asset, new object[]{null});
                        }
                    }
                } else {
                    System.Reflection.MethodInfo _editor_SetColliderShape = typeof(SVGAsset).GetMethod("_editor_SetColliderShape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    _editor_SetColliderShape.Invoke(asset, new object[]{null});
                }

                if(hasGradients)
                {
                    System.Reflection.MethodInfo _editor_SetGradients = typeof(SVGAsset).GetMethod("_editor_SetGradients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if(atlasData.gradients != null && atlasData.gradients.Count > 0)
                    {
                        _editor_SetGradients.Invoke(asset, new object[]{atlasData.gradients.ToArray()});
                    } else {
                        _editor_SetGradients.Invoke(asset, new object[]{null});
                    }
                }
#if IGNORE_EXCEPTIONS
            } catch(System.Exception exception) {
                Debug.LogWarning("Asset: "+UnityEditor.AssetDatabase.GetAssetPath(asset)+" Failed to import\n"+exception.Message, asset);
                errors.Add(SVGError.CorruptedFile);
            }
#endif
            if(_svgDocument != null)
            {
                _svgDocument.Clear();
                _svgDocument = null;
            }
            Clear();

            UnityEditor.EditorUtility.SetDirty(asset);
            _importingSVG = false;
        }
#endif

        public static void Clear()
        {
            if(atlasData != null)
            {
                atlasData.Clear();
                atlasData = null;
            }
            SVGParser.Clear();
            SVGGraphics.Clear();
        }

#if UNITY_EDITOR
        protected T AddObjectToAsset<T>(T obj, SVGAsset asset, HideFlags hideFlags) where T : UnityEngine.Object
        {
            if(obj == null)
                return null;

            obj.hideFlags = hideFlags;
            UnityEditor.AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }
#endif

        public void NewSVGFile(string svgFile)
        {
            this._SVGFile = svgFile;
        }

        public Texture2D GetTexture()
        {
            if (this._texture == null)
            {
                return new Texture2D(0, 0, TextureFormat.ARGB32, false);
            }
            return this._texture;
        }

        public Texture2D CloneTexture(Texture2D texture)
        {
            if(texture == null)
                return null;

            Texture2D output = new Texture2D(texture.width, texture.height, texture.format, false);
            output.name = texture.name;
            output.SetPixels32(texture.GetPixels32());
            output.wrapMode = TextureWrapMode.Clamp;
            output.anisoLevel = 0;
#if UNITY_EDITOR
            output.alphaIsTransparency = true;
#endif
            output.filterMode = FilterMode.Bilinear;
            output.Apply();
            return output;
        }  
    }
}
