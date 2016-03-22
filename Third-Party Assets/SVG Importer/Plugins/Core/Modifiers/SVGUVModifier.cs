// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter 
{
    using Rendering;
    using Utils;

    [ExecuteInEditMode]
    [RequireComponent(typeof(ISVGShape), typeof(ISVGRenderer))]
    [AddComponentMenu("Rendering/SVG UV Modifier", 22)]
    public class SVGUVModifier : MonoBehaviour, ISVGModify {

        public SVGTransform2D svgTransform;
        public bool worldSpace = false;

        protected ISVGShape svgShape;
        protected ISVGRenderer svgRenderer;

        protected SVGTransform2D tempTransform = new SVGTransform2D();
        Matrix4x4 lastMatrix;
        
        // This method is invoked by Unity when rendering to Camera
        void OnWillRenderObject()
        {
            if(svgRenderer == null || svgRenderer.lastFrameChanged == Time.frameCount) return;
            if(svgTransform == null || lastMatrix == svgTransform.matrix) return;
            svgRenderer.UpdateRenderer();
        }
        
        protected virtual void PrepareForRendering (Mesh sharedMesh, bool force) {
            if(sharedMesh == null) return;

            int vertexCount = sharedMesh.vertexCount;
            tempTransform.SetTransform(svgTransform);

            if(worldSpace)
            {
                tempTransform = SVGTransform2D.DecomposeMatrix(transform.worldToLocalMatrix * svgTransform.matrix);
            }

            Quaternion rotation = Quaternion.Euler(0f, 0f, -tempTransform.rotation);
            Vector2 scale = new Vector2((tempTransform.scale.x == 0f) ? 0f : 1f / tempTransform.scale.x, 
                                        (tempTransform.scale.y == 0f) ? 0f : 1f / tempTransform.scale.y);

            Vector3[] vertices = sharedMesh.vertices;
            Vector2[] uv = sharedMesh.uv;
            if(uv == null || uv.Length != vertices.Length) uv = new Vector2[vertices.Length];

            for (int i = 0; i < vertexCount; i++)
            {
                uv[i].x = -vertices[i].x + tempTransform.position.x;
                uv[i].y = -vertices[i].y + tempTransform.position.y;
                
                uv[i] = rotation * uv[i];
                
                uv[i].x *= scale.x;
                uv[i].y *= scale.y;
                
                uv[i].x += 0.5f;
                uv[i].y += 0.5f;
            }
            
            sharedMesh.uv = uv;
            lastMatrix = svgTransform.matrix;
        }

        void Init()
        {
            svgShape = GetComponent(typeof(ISVGShape)) as ISVGShape;
            svgRenderer = GetComponent(typeof(ISVGRenderer)) as ISVGRenderer;
            svgRenderer.AddModifier(this);
            svgRenderer.OnPrepareForRendering += PrepareForRendering;
        }
        
        void Clear()
        {
            if(svgRenderer != null) 
            {
                svgRenderer.OnPrepareForRendering -= PrepareForRendering;
                svgRenderer.RemoveModifier(this);
                svgRenderer = null;
            }
            svgShape = null;
        }
        
        void OnEnable()
        {
            Init();
        }
        
        void OnDisable()
        {
            Clear();
        }
    }
}
