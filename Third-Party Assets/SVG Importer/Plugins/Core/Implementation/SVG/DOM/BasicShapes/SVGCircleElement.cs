// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter.Rendering 
{
    using Geometry;
    using Utils;
    using Document;

    public class SVGCircleElement : SVGParentable, ISVGDrawable
    {
        private SVGLength _cx;
        private SVGLength _cy;
        private SVGLength _r;

        private AttributeList _attrList;
        public AttributeList attrList
        {
            get {
                return _attrList;
            }
        }
        private SVGPaintable _paintable;
        public SVGPaintable paintable
        {
            get {
                return _paintable;
            }
        }

        public SVGLength cx
        {
            get
            {
                return this._cx;
            }
        }

        public SVGLength cy
        {
            get
            {
                return this._cy;
            }
        }

        public SVGLength r
        {
            get
            {
                return this._r;
            }
        }

        public SVGCircleElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null) : base(inheritTransformList)
        {
            this._attrList = node.attributes;
            this._paintable = new SVGPaintable(inheritPaintable, node);
            this._cx = new SVGLength(attrList.GetValue("cx"));
            this._cy = new SVGLength(attrList.GetValue("cy"));
            this._r = new SVGLength(attrList.GetValue("r"));
            this.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));

            Rect viewport = _paintable.viewport;
            this.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(this._attrList, ref viewport, false)));
            paintable.SetViewport(viewport);
        }

        public void BeforeRender(SVGTransformList transformList)
        {
            this.inheritTransformList = transformList;
        }
        
        public static List<Vector2> GetPath(SVGCircleElement svgElement)
        {
            return GetPath(SVGMatrix.Identity(), svgElement);
        }
        
        public static List<Vector2> GetPath(SVGMatrix matrix, SVGCircleElement svgElement)
        {
            List<Vector2> output = Circle(svgElement.cx.value, svgElement.cy.value, svgElement.r.value, matrix);
            output.Add(output[0]);            
            return output;
        }

        public static List<List<Vector2>> GetClipPath(SVGMatrix matrix, SVGCircleElement svgElement)
        {
            List<Vector2> path = GetPath(matrix, svgElement);
            if(path == null || path.Count == 0) return null;
            
            List<List<Vector2>> clipPath = new List<List<Vector2>>();
            if(svgElement.paintable.IsFill())
            {
                clipPath.Add(path);
            }
            
            if(svgElement.paintable.IsStroke())
            {
                List<StrokeSegment[]> segments = new List<StrokeSegment[]>(){SVGSimplePath.GetSegments(path)};
                List<List<Vector2>> strokePath = SVGLineUtils.StrokeShape(segments, svgElement.paintable.strokeWidth, Color.black, SVGSimplePath.GetStrokeLineJoin(svgElement.paintable.strokeLineJoin), SVGSimplePath.GetStrokeLineCap(svgElement.paintable.strokeLineCap), svgElement.paintable.miterLimit, svgElement.paintable.dashArray, svgElement.paintable.dashOffset, ClosePathRule.ALWAYS, SVGGraphics.roundQuality);
                if(strokePath != null && strokePath.Count > 0) clipPath.AddRange(strokePath);
            }
            
            return clipPath;
        }

        public void Render()
        {   
            Create(this);
        }

        public static void Create(SVGCircleElement svgElement)
        {        
            if(svgElement.paintable.visibility != SVGVisibility.Visible || svgElement.paintable.display == SVGDisplay.None)
                return;

            SVGGraphics.position_buffer = GetPath(svgElement.transformMatrix, svgElement);
            if(svgElement.paintable.IsFill())
                CreateFill(svgElement);
            if(svgElement.paintable.IsStroke())
                CreateStroke(svgElement);
        }
        
        static void CreateFill(SVGCircleElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name))
                name = "Circle Fill";

            List<List<Vector2>> path;
            if(svgElement.paintable.clipPathList != null && svgElement.paintable.clipPathList.Count > 0)
            {
                path = SVGGeom.ClipPolygon(new List<List<Vector2>>(){SVGGraphics.position_buffer}, svgElement.paintable.clipPathList);
            } else {
                path = new List<List<Vector2>>(){SVGGraphics.position_buffer};
            }

            Mesh antialiasingMesh;
            Mesh mesh = SVGSimplePath.CreatePolygon(path, svgElement.paintable, svgElement.transformMatrix, out antialiasingMesh);
            if(mesh == null) return;
            mesh.name = name;
            SVGGraphics.AddMesh(new SVGMesh(mesh, svgElement.paintable.svgFill, svgElement.paintable.opacity));
            if(antialiasingMesh != null)
            {
                SVGFill svgFill = svgElement.paintable.svgFill.Clone();
                svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
                SVGGraphics.AddMesh(new SVGMesh(antialiasingMesh, svgFill, svgElement.paintable.opacity));
            }
        }
        
        static void CreateStroke(SVGCircleElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name))
                name = "Circle Stroke ";

            List<List<Vector2>> stroke = SVGSimplePath.CreateStroke(SVGGraphics.position_buffer, svgElement.paintable, ClosePathRule.ALWAYS);
            if(svgElement.paintable.clipPathList != null && svgElement.paintable.clipPathList.Count > 0)
            {
                stroke = SVGGeom.ClipPolygon(stroke, svgElement.paintable.clipPathList);
            }

            Mesh antialiasingMesh;
            Mesh mesh = SVGLineUtils.TessellateStroke(stroke, SVGSimplePath.GetStrokeColor(svgElement.paintable), out antialiasingMesh);
            if(mesh == null) return;            
            mesh.name = name;
            SVGGraphics.AddMesh(new SVGMesh(mesh, svgElement.paintable.svgFill, svgElement.paintable.opacity));
            if(antialiasingMesh != null)
            {
                SVGFill svgFill = svgElement.paintable.svgFill.Clone();
                svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
                SVGGraphics.AddMesh(new SVGMesh(antialiasingMesh, svgFill, svgElement.paintable.opacity));
            }
        }
        
        const float circleConstant = 0.551915024494f;
        public static List<Vector2> Circle(float x0, float y0, float radius, SVGMatrix matrix) {
            List<Vector2> output = new List<Vector2>();
            
            x0 -= radius;
            y0 -= radius;
            
            float handleDistance = circleConstant * radius;
            Vector2 handleRight = new Vector2(handleDistance, 0f);
            Vector2 handleLeft = new Vector2(-handleDistance, 0f);
            Vector2 handleUp = new Vector2(0f, -handleDistance);
            Vector2 handleDown = new Vector2(0f, handleDistance);
            
            Vector2 topCenter = new Vector2(x0 + radius, y0);
            Vector2 left = new Vector2(x0, y0 + radius);
            Vector2 right = new Vector2(x0 + radius * 2f, y0 + radius );
            Vector2 bottomCenter = new Vector2(x0 + radius, y0 + radius * 2f);
            
            output.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(topCenter), 
                                                    matrix.Transform(topCenter + handleRight), 
                                                    matrix.Transform(right + handleUp),
                                                    matrix.Transform(right)));
            
            output.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(right), 
                                                    matrix.Transform(right + handleDown), 
                                                    matrix.Transform(bottomCenter + handleRight),
                                                    matrix.Transform(bottomCenter)));
            
            output.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(bottomCenter), 
                                                    matrix.Transform(bottomCenter + handleLeft), 
                                                    matrix.Transform(left + handleDown),
                                                    matrix.Transform(left)));
            
            output.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(left), 
                                                    matrix.Transform(left + handleUp), 
                                                    matrix.Transform(topCenter + handleLeft),
                                                    matrix.Transform(topCenter)));
            return output;
        }
    }
}
