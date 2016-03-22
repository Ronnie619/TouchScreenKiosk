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

    public class SVGRectElement : SVGParentable, ISVGDrawable
    {
        private SVGLength _x;
        private SVGLength _y;
        private SVGLength _width;
        private SVGLength _height;
        private SVGLength _rx;
        private SVGLength _ry;

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

        public SVGLength x
        {
            get
            {
                return this._x;
            }
        }

        public SVGLength y
        {
            get
            {
                return this._y;
            }
        }

        public SVGLength width
        {
            get
            {
                return this._width;
            }
        }

        public SVGLength height
        {
            get
            {
                return this._height;
            }
        }

        public SVGLength rx
        {
            get
            {
                return this._rx;
            }
        }

        public SVGLength ry
        {
            get
            {
                return this._ry;
            }
        }

        public SVGRectElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null) : base(inheritTransformList)
        {
            this._attrList = node.attributes;
            this._paintable = new SVGPaintable(inheritPaintable, node);
            this._x = new SVGLength(attrList.GetValue("x"));
            this._y = new SVGLength(attrList.GetValue("y"));
            this._width = new SVGLength(attrList.GetValue("width"));
            this._height = new SVGLength(attrList.GetValue("height"));
            this._rx = new SVGLength(attrList.GetValue("rx"));
            this._ry = new SVGLength(attrList.GetValue("ry"));
            this.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));

            Rect viewport = _paintable.viewport;
            this.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(_attrList, ref viewport, false)));
            paintable.SetViewport(viewport);
        }

        public void BeforeRender(SVGTransformList transformList)
        {
            this.inheritTransformList = transformList;
        }

        public static List<Vector2> GetPath(SVGRectElement svgElement)
        {
            return GetPath(SVGMatrix.Identity(), svgElement);
        }

        public static List<Vector2> GetPath(SVGMatrix matrix, SVGRectElement svgElement)
        {
            List<Vector2> output = new List<Vector2>();

            float width = svgElement.width.value, height = svgElement.height.value;
            float x = svgElement.x.value, y = svgElement.y.value, rx = svgElement.rx.value, ry = svgElement.ry.value;
            
            Vector2 p1 = new Vector2(x, y),
            p2 = new Vector2(x + width, y),
            p3 = new Vector2(x + width, y + height),
            p4 = new Vector2(x, y + height);
            
            if(rx == 0.0f && ry == 0.0f) {
                output = new List<Vector2>(new Vector2[]{
                    matrix.Transform(p1),
                    matrix.Transform(p2),
                    matrix.Transform(p3),
                    matrix.Transform(p4)
                });
            } else {
                float t_rx = (rx == 0.0f) ? ry : rx;
                float t_ry = (ry == 0.0f) ? rx : ry;
                
                t_rx = (t_rx > (width * 0.5f - 2f)) ? (width * 0.5f - 2f) : t_rx;
                t_ry = (t_ry > (height * 0.5f - 2f)) ? (height * 0.5f - 2f) : t_ry;
                
                float angle = svgElement.transformAngle;
                
                Vector2 t_p1 = matrix.Transform(new Vector2(p1.x + t_rx, p1.y));
                Vector2 t_p2 = matrix.Transform(new Vector2(p2.x - t_rx, p2.y));
                Vector2 t_p3 = matrix.Transform(new Vector2(p2.x, p2.y + t_ry));
                Vector2 t_p4 = matrix.Transform(new Vector2(p3.x, p3.y - t_ry));
                
                Vector2 t_p5 = matrix.Transform(new Vector2(p3.x - t_rx, p3.y));
                Vector2 t_p6 = matrix.Transform(new Vector2(p4.x + t_rx, p4.y));
                Vector2 t_p7 = matrix.Transform(new Vector2(p4.x, p4.y - t_ry));
                Vector2 t_p8 = matrix.Transform(new Vector2(p1.x, p1.y + t_ry));
                
                output = SVGGeomUtils.RoundedRect(t_p1, t_p2, t_p3, t_p4, t_p5, t_p6, t_p7, t_p8, t_rx, t_ry, angle);
            }
            
            output.Add(output[0]);

            return output;
        }
        
        public static List<List<Vector2>> GetClipPath(SVGMatrix matrix, SVGRectElement svgElement)
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

        public static void Create(SVGRectElement svgElement)
        {
            if(svgElement.paintable.visibility != SVGVisibility.Visible || svgElement.paintable.display == SVGDisplay.None)
                return;

            SVGGraphics.position_buffer = GetPath(svgElement.transformMatrix, svgElement);

            if(svgElement.paintable.IsFill())
                CreateFill(svgElement);
            if(svgElement.paintable.IsStroke())
                CreateStroke(svgElement);
        }
        
        static void CreateFill(SVGRectElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name)) name = "Rectangle Fill";
            
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
        
        static void CreateStroke(SVGRectElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name)) name = "Rectangle Stroke ";
            
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
    }
}
