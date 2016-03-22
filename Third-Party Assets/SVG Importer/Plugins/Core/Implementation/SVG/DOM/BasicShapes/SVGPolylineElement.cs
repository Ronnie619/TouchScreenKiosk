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

    public class SVGPolylineElement : SVGParentable, ISVGDrawable
    {
        private List<Vector2> _listPoints;

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

        public List<Vector2> listPoints
        {
            get { return this._listPoints; }
        }

        public SVGPolylineElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null) : base(inheritTransformList)
        {
            this._attrList = node.attributes;
            this._paintable = new SVGPaintable(inheritPaintable, node);
            this._listPoints = ExtractPoints(this._attrList.GetValue("points"));
            this.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));
            
            Rect viewport = _paintable.viewport;
            this.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(_attrList, ref viewport, false)));
            paintable.SetViewport(viewport);
        }

        private List<Vector2> ExtractPoints(string inputText)
        {
            List<Vector2> _return = new List<Vector2>();
            string[] _lstStr = SVGStringExtractor.ExtractTransformValue(inputText);

            int len = _lstStr.Length;
            for (int i = 0; i < len -1; i++)
            {
                string value1, value2;
                value1 = _lstStr [i];
                value2 = _lstStr [i + 1];
                SVGLength _length1 = new SVGLength(value1);
                SVGLength _length2 = new SVGLength(value2);
                Vector2 _point = new Vector2(_length1.value, _length2.value);
                _return.Add(_point);
                i++;
            }
            return _return;
        }

        public void BeforeRender(SVGTransformList transformList)
        {
            this.inheritTransformList = transformList;
        }

        public static List<Vector2> GetPath(SVGPolylineElement svgElement)
        {
            return GetPath(SVGMatrix.Identity(), svgElement);
        }
        
        public static List<Vector2> GetPath(SVGMatrix matrix, SVGPolylineElement svgElement)
        {
            List<Vector2> output = new List<Vector2>(svgElement.listPoints.Count + 1);
            List<Vector2> listPoints = svgElement.listPoints;
            for (int i = 0; i < listPoints.Count; i++)
            {
                output.Add(matrix.Transform(listPoints[i]));
            }

            // Douglas Peucker Reduction
            return SVGBezier.Optimise(output, SVGGraphics.vpm);
        }
        
        public static List<List<Vector2>> GetClipPath(SVGMatrix matrix, SVGPolylineElement svgElement)
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
                List<List<Vector2>> strokePath = SVGLineUtils.StrokeShape(segments, svgElement.paintable.strokeWidth, Color.black, SVGSimplePath.GetStrokeLineJoin(svgElement.paintable.strokeLineJoin), SVGSimplePath.GetStrokeLineCap(svgElement.paintable.strokeLineCap), svgElement.paintable.miterLimit, svgElement.paintable.dashArray, svgElement.paintable.dashOffset, ClosePathRule.NEVER, SVGGraphics.roundQuality);
                if(strokePath != null && strokePath.Count > 0) clipPath.AddRange(strokePath);
            }
            
            return clipPath;
        }

        public void Render()
        {
            Create(this);
        }
        public static void Create(SVGPolylineElement svgElement)
        {
            if(svgElement.paintable.visibility != SVGVisibility.Visible || svgElement.paintable.display == SVGDisplay.None)
                return;

            SVGGraphics.position_buffer = GetPath(svgElement.transformMatrix, svgElement);
            
            if(svgElement.paintable.IsFill())
                CreateFill(svgElement);
            if(svgElement.paintable.IsStroke())
                CreateStroke(svgElement);
        }
        
        static void CreateFill(SVGPolylineElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name))
                name = "Polyline Fill";
            
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
        
        static void CreateStroke(SVGPolylineElement svgElement)
        {
            string name = svgElement.attrList.GetValue("id");
            if (string.IsNullOrEmpty(name))
                name = "Polyline Stroke ";

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
