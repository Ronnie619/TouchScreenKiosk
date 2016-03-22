// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter.Geometry
{
    using Rendering;
    using Data;
    using Utils;

    public class SVGMesh : System.Object
    {        
        protected static FILL_BLEND lastBlendType = FILL_BLEND.ALPHA_BLENDED;

        protected int _depth;
        public int depth
        {
            get {
                return _depth;
            }
        }

        protected string _name;
        public string name
        {
            get { return _name; }
        }

        protected SVGFill _fill;
        public SVGFill fill 
        { 
            get { return _fill; } 
        }

        protected Vector3[] _vertices;
        public Vector3[] vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        protected Vector2[] _uvs;
        public Vector2[] uvs
        {
            get { return _uvs; }
            set { _uvs = value; }
        }

        protected Vector2[] _uvs2;
        public Vector2[] uvs2
        {
            get { return _uvs2; }
            set { _uvs2 = value; }
        }

        protected Color32[] _colors;
        public Color32[] colors
        {
            get { return _colors; }
            set { _colors = value; }
        }

        protected int[] _triangles;
        public int[] triangles
        {
            get { return _triangles; }
            set { _triangles = value; }
        }

        protected Bounds _bounds;
        public Bounds bounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }
        
        public SVGMesh(Mesh mesh, SVGFill svgFill, float opacity = 1f)
        {
            if (mesh == null)
                return;

            _name = mesh.name;
            _fill = svgFill.Clone();
            if(_fill.blend == FILL_BLEND.OPAQUE)
            {
                if(opacity < 1f) _fill.blend = FILL_BLEND.ALPHA_BLENDED;
            }

            int length = mesh.vertices.Length;
            int trianglesLength = mesh.triangles.Length;

            _vertices = new Vector3[length];

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < length; i++)
            {
                _vertices [i].x = mesh.vertices [i].x * SVGAssetImport.meshScale;
                _vertices [i].y = mesh.vertices [i].y * SVGAssetImport.meshScale;
                _vertices [i].z = _depth * -SVGAssetImport.minDepthOffset;

                if(_vertices [i].x < minX) minX = _vertices [i].x;
                else if(_vertices [i].x > maxX) maxX = _vertices [i].x;

                if(_vertices [i].y < minY) minY = _vertices [i].y;
                else if(_vertices [i].y > maxY) maxY = _vertices [i].y;

                if(_vertices [i].z < minZ) minZ = _vertices [i].z;
                else if(_vertices [i].z > maxZ) maxZ = _vertices [i].z;
            }

            _bounds = new Bounds(new Vector3(Mathf.Lerp(minX, maxX, 0.5f),
                                             Mathf.Lerp(minY, maxY, 0.5f),
                                             Mathf.Lerp(minZ, maxZ, 0.5f))
                                 , new Vector3(maxX - minX,
                                                maxY - minY,
                                                maxZ - minZ
                          ));

            _triangles = new int[trianglesLength];

			// Correct Winding
			int triangle0, triangle1, triangle2;
			Vector2 vector0, vector1, vector2;
			float winding;
            for (int i = 0; i < trianglesLength; i+=3)
            {
				triangle0 = mesh.triangles [i];
				triangle1 = mesh.triangles [i + 1];
				triangle2 = mesh.triangles [i + 2];

				vector0 = _vertices[triangle0];
				vector1 = _vertices[triangle1];
				vector2 = _vertices[triangle2];

				winding = (vector1.x - vector0.x) * (vector1.y + vector0.y);
				winding += (vector2.x - vector1.x) * (vector2.y + vector1.y);
				winding += (vector0.x - vector2.x) * (vector0.y + vector2.y);

				if(winding < 0)
				{				
	                _triangles [i] = mesh.triangles [i];
					_triangles [i + 1] = mesh.triangles [i + 1];
					_triangles [i + 2] = mesh.triangles [i + 2];
				} else {
					_triangles [i] = mesh.triangles [i];
					_triangles [i + 2] = mesh.triangles [i + 1];
					_triangles [i + 1] = mesh.triangles [i + 2];
				}
            }

            if (mesh.colors32 != null && mesh.colors32.Length > 0)
            {
                _colors = new Color32[length];
                for (int i = 0; i < length; i++)
                {
                    _colors [i] = mesh.colors32 [i];
                    if(opacity != 1f) _colors[i].a = (byte)Mathf.RoundToInt(((_colors[i].a / 255f) * opacity) * 255);
                }
            } else {
				_colors = new Color32[length];
                Color32 color = new Color32((byte)255, (byte)255, (byte)255, (byte)Mathf.RoundToInt(opacity * 255));
				for (int i = 0; i < length; i++)
				{
                    _colors [i] = color;
				}
			}

            if (mesh.uv != null && mesh.uv.Length > 0)
            {
                _uvs = new Vector2[length];
                for (int i = 0; i < length; i++)
                {        
                    _uvs [i] = mesh.uv [i];
                }
            } else {
				_uvs = new Vector2[length];
			}

            if (mesh.uv2 != null && mesh.uv2.Length > 0)
            {
                _uvs2 = new Vector2[length];
                for (int i = 0; i < length; i++)
                {        
                    _uvs2 [i] = mesh.uv2 [i];
                }
            } else {
				_uvs2 = new Vector2[length];
			}
        }

        public void UpdateDepth()
        {
            if(_vertices == null || _vertices.Length == 0)
                return;

            int length = _vertices.Length;
            for (int i = 0; i < length; i++)
            {
                _vertices [i].z = _depth * -SVGAssetImport.minDepthOffset;
            }
        }

        public static Mesh CombineMeshes(List<SVGMesh> meshes, out SVGLayer[] layers, out Shader[] shaders, SVGUseGradients useGradients = SVGUseGradients.Always, SVGAssetFormat format = SVGAssetFormat.Transparent, bool compressDepth = true)
        {
            layers = new SVGLayer[0];
            shaders = new Shader[0];

            //if(SVGAssetImport.sliceMesh) Create9Slice();

            SVGFill fill;
            bool useOpaqueShader = false;
            bool useTransparentShader = false;
            bool hasGradients = (useGradients == SVGUseGradients.Always);

            int totalMeshes = meshes.Count, totalTriangles = 0, opaqueTriangles = 0, transparentTriangles = 0;

            // Z Sort meshes
            if(format == SVGAssetFormat.Opaque)
            {
                if(compressDepth)
                {
                    SVGBounds meshBounds = SVGBounds.InfiniteInverse;
                    for (int i = 0; i < totalMeshes; i++)
                    {
                        if (meshes [i] == null) continue;                
                        meshBounds.Encapsulate(meshes [i].bounds);
                    }

                    if(!meshBounds.isInfiniteInverse)
                    {
                        SVGGraphics.depthTree.Clear();
                        SVGGraphics.depthTree = new SVGDepthTree(meshBounds);

                        for (int i = 0; i < totalMeshes; i++)
                        {
                            fill = meshes [i]._fill;
                            SVGMesh[] nodes = SVGGraphics.depthTree.TestDepthAdd(meshes [i], new SVGBounds(meshes [i]._bounds));
                            int nodesLength = 0;
                            if(nodes == null || nodes.Length == 0)
                            {
                                meshes [i]._depth = 0;
                            } else {
                                nodesLength = nodes.Length;
                                int highestDepth = 0;
                                SVGMesh highestMesh = null;
                                for(int j = 0; j < nodesLength; j++)
                                {
                                    if(nodes[j].depth > highestDepth)
                                    {
                                        highestDepth = nodes[j].depth;
                                        highestMesh = nodes[j];
                                    }
                                }
                                
                                if(fill.blend == FILL_BLEND.OPAQUE)
                                {
                                    meshes [i]._depth = highestDepth + 1;
                                } else {
                                    if(highestMesh != null && highestMesh.fill.blend == FILL_BLEND.OPAQUE)
                                    {
                                        meshes [i]._depth = highestDepth + 1;
                                    } else {
                                        meshes [i]._depth = highestDepth;
                                    }
                                }
                            }
                            
                            meshes [i].UpdateDepth();
                        }
                    }
                } else {
                    int highestDepth = 0;
                    for (int i = 0; i < totalMeshes; i++)
                    {
                        fill = meshes [i]._fill;
                        if (fill.blend == FILL_BLEND.OPAQUE || lastBlendType == FILL_BLEND.OPAQUE)
                        {
                            meshes[i]._depth = ++highestDepth;
                        } else 
                        {
                            meshes[i]._depth = highestDepth;
                        }
                        
                        lastBlendType = fill.blend;
                        meshes[i].UpdateDepth();
                    }
                }
            }

            layers = new SVGLayer[totalMeshes];
            int totalVertices = 0, vertexCount, vertexStart, currentVertex;
            for(int i = 0; i < totalMeshes; i++)
            {
                fill = meshes[i]._fill;
                if(fill.blend == FILL_BLEND.OPAQUE) { 
                    opaqueTriangles += meshes[i]._triangles.Length;
                    useOpaqueShader = true; 
                }
                else if(fill.blend == FILL_BLEND.ALPHA_BLENDED) { 
                    transparentTriangles += meshes[i]._triangles.Length;
                    useTransparentShader = true; 
                }
                if(fill.fillType == FILL_TYPE.GRADIENT) hasGradients = true;

                vertexCount = meshes[i]._vertices.Length;
                Bounds bounds = meshes[i]._bounds;
                layers[i] = new SVGLayer(meshes[i]._name, totalVertices, vertexCount, bounds.center, bounds.size);
                totalVertices += vertexCount;
            }

            totalTriangles = opaqueTriangles + transparentTriangles;

            if(useGradients == SVGUseGradients.Never) hasGradients = false;
            if(format != SVGAssetFormat.Opaque)
            { 
                useOpaqueShader = false; 
                useTransparentShader = true;
            }

            Vector3[] vertices = new Vector3[totalVertices];
            Color32[] colors32 = new Color32[totalVertices];
            Vector2[] uv = null;
            Vector2[] uv2 = null;
            int[][] triangles = null;

            for(int i = 0; i < totalMeshes; i++)
            {
                vertexStart = layers[i].vertexStart;
                vertexCount = layers[i].vertexCount;
                for(int j = 0; j < vertexCount; j++)
                {
                    currentVertex = vertexStart + j;
                    vertices[currentVertex] = meshes[i]._vertices[j];
                    colors32[currentVertex] = meshes[i]._colors[j];
                }
            }

            List<Shader> outputShaders = new List<Shader>();
//            Debug.Log("hasGradients: "+hasGradients);
            if(hasGradients)
            {
                uv = new Vector2[totalVertices];
                uv2 = new Vector2[totalVertices];

                for(int i = 0; i < totalMeshes; i++)
                {
                    vertexStart = layers[i].vertexStart;
                    vertexCount = layers[i].vertexCount;
                    for(int j = 0; j < vertexCount; j++)
                    {
                        currentVertex = vertexStart + j;
                        uv[currentVertex] = meshes[i]._uvs[j];
                        uv2[currentVertex] = meshes[i]._uvs2[j];
                    }
                }

                if(useOpaqueShader)
                {
                    outputShaders.Add(SVGShader.GradientColorOpaque);
                }
                if(useTransparentShader)
                {
                    outputShaders.Add(SVGShader.GradientColorAlphaBlended);
                }
            } else {
                if(useOpaqueShader)
                {
                    outputShaders.Add(SVGShader.SolidColorOpaque);
                }
                if(useTransparentShader)
                {
                    outputShaders.Add(SVGShader.SolidColorAlphaBlended);
                }
            }

            if(useOpaqueShader && useTransparentShader)
            {
                triangles = new int[2][]{new int[opaqueTriangles], new int[transparentTriangles]};

                int lastVertexIndex = 0;
                int triangleCount;
                int lastOpauqeTriangleIndex = 0;
                int lastTransparentTriangleIndex = 0;
                
                for(int i = 0; i < totalMeshes; i++)
                {
                    triangleCount = meshes[i]._triangles.Length;
                    if(meshes[i]._fill.blend == FILL_BLEND.OPAQUE)
                    {
                        for(int j = 0; j < triangleCount; j++)
                        {
                            triangles[0][lastOpauqeTriangleIndex++] = lastVertexIndex + meshes[i]._triangles[j];
                        }
                    } else {
                        for(int j = 0; j < triangleCount; j++)
                        {
                            triangles[1][lastTransparentTriangleIndex++] = lastVertexIndex + meshes[i]._triangles[j];
                        }
                    }

                    lastVertexIndex += layers[i].vertexCount;
                }
            } else {
                triangles = new int[1][]{new int[totalTriangles]};
                
                int lastVertexIndex = 0;
                int triangleCount;
                int lastTriangleIndex = 0;
                
                for(int i = 0; i < totalMeshes; i++)
                {
                    triangleCount = meshes[i]._triangles.Length;
                    for(int j = 0; j < triangleCount; j++)
                    {
                        triangles[0][lastTriangleIndex++] = lastVertexIndex + meshes[i]._triangles[j];
                    }
                    lastVertexIndex += layers[i].vertexCount;
                }
            }

            if(outputShaders.Count != 0) shaders = outputShaders.ToArray();

            Mesh output = new Mesh();
            output.vertices = vertices;
            output.colors32 = colors32;

            if(hasGradients)
            {
                output.uv = uv;
                output.uv2 = uv2;
            }

            if(triangles.Length == 1)
            {
                output.triangles = triangles[0];
            } else {
                output.subMeshCount = triangles.Length;
                for(int i = 0; i < triangles.Length; i++)
                {
                    output.SetTriangles(triangles[i], i);
                }
            }

            return output;
        }

        protected static void Create9Slice()
        {
            int meshCount = SVGGraphics.meshes.Count;
            SVGBounds meshBounds = SVGBounds.InfiniteInverse;
            for (int i = 0; i < meshCount; i++)
            {
                if (SVGGraphics.meshes [i] == null) continue;                
                meshBounds.Encapsulate(SVGGraphics.meshes [i].bounds);
            }

            // 9-slice
            if(SVGAssetImport.border.sqrMagnitude > 0f)
            {
                Vector2 min = meshBounds.min;
                Vector2 max = meshBounds.max;

                float bottom = Mathf.Lerp(min.y, max.y, 0.5f);

                for(int i = 0; i < meshCount; i++)
                {
                    if(SVGAssetImport.border.x > 0)
                        SVGMeshCutter.MeshSplit(SVGGraphics.meshes [i], new Vector2(Mathf.Lerp(min.x, max.x, SVGAssetImport.border.x), 0f), Vector2.up); 
                    if(SVGAssetImport.border.y > 0)
                        SVGMeshCutter.MeshSplit(SVGGraphics.meshes [i], new Vector2(0f, bottom), Vector2.right);                     
                    if(SVGAssetImport.border.z > 0)
                        SVGMeshCutter.MeshSplit(SVGGraphics.meshes [i], new Vector2(Mathf.Lerp(min.x, max.x, 1f - SVGAssetImport.border.z), 0f), Vector2.up);                     
                    if(SVGAssetImport.border.w > 0)
                        SVGMeshCutter.MeshSplit(SVGGraphics.meshes [i], new Vector2(0f, Mathf.Lerp(min.y, max.y, SVGAssetImport.border.w)), Vector2.right); 
                }
            }
        }

        public Mesh mesh
        { 
            get
            {
    //        Debug.Log(_vertices);
    //        Debug.Log(_triangles);
                if (_vertices == null || _vertices.Length == 0 || _triangles == null || _triangles.Length == 0)
                    return null;

                Mesh output = new Mesh();
                Bounds meshBounds = new Bounds();
                meshBounds.SetMinMax(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), new Vector3(float.MinValue, float.MinValue, float.MinValue));

                int length = _vertices.Length;
                int trianglesLength = _triangles.Length;
            
                Vector3[] finVertices = new Vector3[length];
                for (int i = 0; i < length; i++)
                {
                    finVertices [i] = _vertices [i];
                    meshBounds.Encapsulate(_vertices [i]);
                }
                output.vertices = finVertices;
            
                int[] finTriangles = new int[trianglesLength];
                for (int i = 0; i < trianglesLength; i++)
                {
                    finTriangles [i] = _triangles [i];
                }
                output.triangles = finTriangles;
            
                if (_colors != null && _colors.Length > 0)
                {
                    Color32[] finColors = new Color32[length];
                    for (int i = 0; i < length; i++)
                    {
                        finColors [i] = _colors [i];
                    }
                    output.colors32 = finColors;
                } else {
					Color32[] finColors = new Color32[length];
					for (int i = 0; i < length; i++)
					{
						finColors [i] = Color.white;
					}
					output.colors32 = finColors;
				}

                if (_uvs != null && _uvs.Length > 0)
                {
                    Vector2[] finUvs = new Vector2[length];
                    for (int i = 0; i < length; i++)
                    {
                        finUvs [i] = _uvs [i];
                    }
                    output.uv = finUvs;
                } else {
					output.uv = new Vector2[length];
				}

                if (_uvs2 != null && _uvs2.Length > 0)
                {
                    Vector2[] finUvs2 = new Vector2[length];
                    for (int i = 0; i < length; i++)
                    {
                        finUvs2 [i] = _uvs2 [i];
                    }
                    output.uv2 = finUvs2;
                } else {
					output.uv2 = new Vector2[length];
				}
            
                return output;
            }
        }
    }
}
