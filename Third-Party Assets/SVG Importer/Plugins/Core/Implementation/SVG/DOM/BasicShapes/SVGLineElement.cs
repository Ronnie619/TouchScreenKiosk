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

    public class SVGLineElement : SVGParentable, ISVGDrawable
    {
        private SVGLength _x1;
        private SVGLength _y1;
        private SVGLength _x2;
        private SVGLength _y2;

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

        public SVGLength x1
        {
            get
            {
                return this._x1;
            }
        }

        public SVGLength y1
        {
            get
            {
                return this._y1;
            }
        }

        public SVGLength x2
        {
            get
            {
                return this._x2;
            }
        }

        public SVGLength y2
        {
            get
            {
                return this._y2;
            }
        }

        public SVGLineElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null) : base(inheritTransformList)
        {
            this._attrList = node.attributes;
            this._paintable = new SVGPaintable(inheritPaintable, node);
            this._x1 = new SVGLength(attrList.GetValue("x1"));
            this._y1 = new SVGLength(attrList.GetValue("y1"));
            this._x2 = new SVGLength(attrList.GetValue("x2"));
            this._y2 = new SVGLength(attrList.GetValue("y2"));
            this.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));
            
            Rect viewport = _paintable.viewport;
            this.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(_attrList, ref viewport, false)));
            paintable.SetViewport(viewport);
        }

        public void BeforeRender(SVGTransformList transformList)
        {
            this.inheritTransformList = transformList;
        }

        public static List<Vector2> GetPath(SVGLineElement svgElement)
        {
            return GetPath(SVGMatrix.Identity(), svgElement);
        }
        
        public static List<Vector2> GetPath(SVGMatrix matrix, SVGLineElement svgElement)
        {
            List<Vector2> output = new List<Vector2>(){
                matrix.Transform(new Vector2(svgElement.x1.value, svgElement.y1.value)),
                matrix.Transform(new Vector2(svgElement.x2.value, svgElement.y2.value))
            };
            return output;
        }
        
        public static List<List<Vector2>> GetClipPath(SVGMatrix matrix, SVGLineElement svgElement)
        {
            List<Vector2> path = GetPath(matrix, svgElement);
            if(path == null || path.Count == 0) return null;
            
            List<List<Vector2>> clipPath = new List<List<Vector2>>();

            List<StrokeSegment[]> segments = new List<StrokeSegment[]>(){SVGSimplePath.GetSegments(path)};
            List<List<Vector2>> strokePath = SVGLineUtils.StrokeShape(segments, svgElement.paintable.strokeWidth, Color.black, SVGSimplePath.GetStrokeLineJoin(svgElement.paintable.strokeLineJoin), SVGSimplePath.GetStrokeLineCap(svgElement.paintable.strokeLineCap), svgElement.paintable.miterLimit, svgElement.paintable.dashArray, svgElement.paintable.dashOffset, ClosePathRule.NEVER, SVGGraphics.roundQuality);
            if(strokePath != null && strokePath.Count > 0) clipPath.AddRange(strokePath);
            
            return clipPath;
        }

        public void Render()
        {
            Create(this);
        }

        public static void Create(SVGLineElement svgElement)
        {
            if(svgElement.paintable.visibility != SVGVisibility.Visible || svgElement.paintable.display == SVGDisplay.None)
                return;

            SVGGraphics.position_buffer = GetPath(svgElement.transformMatrix, svgElement);

            if(svgElement.paintable.IsStroke())
                CreateStroke(svgElement);
        }
        
        static void CreateStroke(SVGLineElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name))
                name = "Line Stroke ";
            
            List<List<Vector2>> stroke = SVGSimplePath.CreateStroke(SVGGraphics.position_buffer, svgElement.paintable, ClosePathRule.NEVER);
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
    }
}
