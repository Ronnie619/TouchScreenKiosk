// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter.Rendering
{
    using Document;
    using Utils;
    using ClipperLib;
    using Geometry;

    public class SVGSimplePath {

        public static StrokeLineCap GetStrokeLineCap(SVGStrokeLineCapMethod capMethod)
        {
            switch(capMethod)
            {
                case SVGStrokeLineCapMethod.Butt:
                    return StrokeLineCap.butt;
                case SVGStrokeLineCapMethod.Round:
                    return StrokeLineCap.round;
                case SVGStrokeLineCapMethod.Square:
                    return StrokeLineCap.square;
            }

            return StrokeLineCap.butt;
        }

        public static StrokeLineJoin GetStrokeLineJoin(SVGStrokeLineJoinMethod capMethod)
        {
            switch(capMethod)
            {
                case SVGStrokeLineJoinMethod.Miter:
                    return StrokeLineJoin.miter;
                case SVGStrokeLineJoinMethod.MiterClip:
                    return StrokeLineJoin.miterClip;
                case SVGStrokeLineJoinMethod.Round:
                    return StrokeLineJoin.round;
                case SVGStrokeLineJoinMethod.Bevel:
                    return StrokeLineJoin.bevel;
            }
            
            return StrokeLineJoin.bevel;
        }

        public static StrokeSegment[] GetSegments(List<Vector2> points)
        {
            if(points == null || points.Count < 2)
                return null;

            for(int i = 1; i < points.Count; i++)
            {
                if(points[i - 1] == points[i])
                {
                    points.RemoveAt(i - 1);
                    i--;
                }
            }

            List<StrokeSegment> segments = new List<StrokeSegment>();
            for(int i = 1; i < points.Count; i++)
            {
                segments.Add(new StrokeSegment(points[i - 1], points[i]));
            }

            return segments.ToArray();
        }      

        public static Color GetStrokeColor(SVGPaintable paintable)
        {   
            Color color = paintable.strokeColor.Value.color;
            color.a *= paintable.strokeOpacity * paintable.opacity;
            paintable.svgFill = new SVGFill(color, FILL_BLEND.OPAQUE, FILL_TYPE.SOLID);
            if(color.a != 1f) paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
            return color;
        }

        public static List<List<Vector2>> CreateStroke(List<Vector2> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
                return null;
            
            return CreateStroke(new List<List<Vector2>>(){inputShapes}, paintable, closePath);
        }
        
        public static List<List<Vector2>> CreateStroke(List<List<Vector2>> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
                return null;

            List<StrokeSegment[]> segments = new List<StrokeSegment[]>();
            for(int i = 0; i < inputShapes.Count; i++)
            {
                if(inputShapes[i] == null || inputShapes[i].Count < 2)
                    continue;
                
                segments.Add(GetSegments(inputShapes[i]));
            }

            return SVGLineUtils.StrokeShape(segments, paintable.strokeWidth, Color.black, GetStrokeLineJoin(paintable.strokeLineJoin), GetStrokeLineCap(paintable.strokeLineCap), paintable.miterLimit, paintable.dashArray, paintable.dashOffset, closePath, SVGGraphics.roundQuality);
        }

        public static Mesh CreateStrokeMesh(List<Vector2> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
                return null;

            return CreateStrokeMesh(new List<List<Vector2>>(){inputShapes}, paintable, closePath);
        }
        
        public static Mesh CreateStrokeMesh(List<List<Vector2>> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
                return null;

            List<StrokeSegment[]> segments = new List<StrokeSegment[]>();
            for(int i = 0; i < inputShapes.Count; i++)
            {
                if(inputShapes[i] == null || inputShapes[i].Count < 2)
                    continue;

                segments.Add(GetSegments(inputShapes[i]));
            }

            return SVGLineUtils.StrokeMesh(segments, paintable.strokeWidth, GetStrokeColor(paintable), GetStrokeLineJoin(paintable.strokeLineJoin), GetStrokeLineCap(paintable.strokeLineCap), paintable.miterLimit, paintable.dashArray, paintable.dashOffset, closePath, SVGGraphics.roundQuality);
        }

        public static Mesh CreateStrokeSimple(SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            return CreateStrokeSimple(new List<List<Vector2>>(){new List<Vector2>(SVGGraphics.position_buffer.ToArray())}, paintable, closePath);
        }

        public static Mesh CreateStrokeSimple(List<List<Vector2>> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
                return null;

            AddInputShape(inputShapes);

            Color color = GetStrokeColor(paintable);            

            float strokeWidth = paintable.strokeWidth;
            if(inputShapes.Count > 1)
            {
                CombineInstance[] combineInstances = new CombineInstance[inputShapes.Count];
                for(int i = 0; i < inputShapes.Count; i++)
                {
                    combineInstances[i] = new CombineInstance();
                    combineInstances[i].mesh = SVGMeshUtils.VectorLine(inputShapes[i].ToArray(), color, color, strokeWidth, 0f, closePath);
                }

                Mesh mesh = new Mesh();
                mesh.CombineMeshes(combineInstances, true, false);
                return mesh;
            } else {
                return SVGMeshUtils.VectorLine(inputShapes[0].ToArray(), color, color, strokeWidth, 0f, closePath);
            }
        }

        public static Mesh CreateAntialiasing(List<List<Vector2>> inputShapes, Color colorA, float width, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(inputShapes == null || inputShapes.Count == 0)
                return null;

            Color colorB = new Color(colorA.r, colorA.g, colorA.b, 0f);
            if(inputShapes.Count > 1)
            {
                CombineInstance[] combineInstances = new CombineInstance[inputShapes.Count];
                for(int i = 0; i < inputShapes.Count; i++)
                {         
                    combineInstances[i] = new CombineInstance();
                    combineInstances[i].mesh = SVGMeshUtils.VectorLine(inputShapes[i].ToArray(), colorA, colorB, width, width * 0.5f, closePath);
                }
                
                Mesh mesh = new Mesh();
                mesh.CombineMeshes(combineInstances, true, false);
                return mesh;
            } else {
                return SVGMeshUtils.VectorLine(inputShapes[0].ToArray(), colorA, colorB, width, width * 0.5f, closePath);
            }
        }

        public static Mesh CreatePolygon(List<Vector2> inputShapes, SVGPaintable paintable, SVGMatrix matrix, out Mesh antialiasingMesh)
        {        
            if(inputShapes == null || inputShapes.Count == 0)
            {
                antialiasingMesh = null;
                return null;
            }

            return CreatePolygon(new List<List<Vector2>>(){inputShapes}, paintable, matrix, out antialiasingMesh);
        }

        public static Mesh CreatePolygon(List<List<Vector2>> inputShapes, SVGPaintable paintable, SVGMatrix matrix, out Mesh antialiasingMesh)
        {   
            antialiasingMesh = null;
            if(inputShapes == null || inputShapes.Count == 0)
            {
                return null;
            }

            List<List<Vector2>> simplifiedShapes = new List<List<Vector2>>();

            PolyFillType fillType = PolyFillType.pftNonZero;
            if(paintable.fillRule == SVGFillRule.EvenOdd) { fillType = PolyFillType.pftEvenOdd; }

            simplifiedShapes = SVGGeom.SimplifyPolygons(inputShapes, fillType);
            if(simplifiedShapes == null || simplifiedShapes.Count == 0) return null;

            AddInputShape(simplifiedShapes);

            Rect bounds = GetRect(simplifiedShapes);

            switch (paintable.GetPaintType())
            {
                case SVGPaintMethod.SolidFill:
                {
                    Color color = Color.black;
                    SVGColorType colorType = paintable.fillColor.Value.colorType;
                    if(colorType == SVGColorType.Unknown || colorType == SVGColorType.None)
                    {
                        color.a *= paintable.fillOpacity;
                        paintable.svgFill = new SVGFill(color);
                    } else {
                        color = paintable.fillColor.Value.color;
                        color.a *= paintable.fillOpacity; 
                        paintable.svgFill = new SVGFill(color);
                    }

                    paintable.svgFill.fillType = FILL_TYPE.SOLID;
                    if(color.a == 1)
                    {
                        paintable.svgFill.blend = FILL_BLEND.OPAQUE;
                    } else {
                        paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
                    }
                }
                    break;
                case SVGPaintMethod.LinearGradientFill:      
                {
                    SVGLinearGradientBrush linearGradBrush = paintable.GetLinearGradientBrush(bounds, matrix);
                    paintable.svgFill = linearGradBrush.fill;
                }
                    break;
                case SVGPaintMethod.RadialGradientFill:
                {
                    SVGRadialGradientBrush radialGradBrush = paintable.GetRadialGradientBrush(bounds, matrix);
                    paintable.svgFill = radialGradBrush.fill;
                }
                    break;
                case SVGPaintMethod.ConicalGradientFill:
                {
                    SVGConicalGradientBrush conicalGradBrush = paintable.GetConicalGradientBrush(bounds, matrix);
                    paintable.svgFill = conicalGradBrush.fill;
                }
                    break;
                case SVGPaintMethod.PathDraw:  
                {
                    Color color = Color.black;
                    SVGColorType colorType = paintable.fillColor.Value.colorType;
                    if(colorType == SVGColorType.Unknown || colorType == SVGColorType.None)
                    {
                        color.a *= paintable.strokeOpacity;
                        paintable.svgFill = new SVGFill(color);
                    } else {
                        color = paintable.fillColor.Value.color;
                        color.a *= paintable.strokeOpacity;
                        paintable.svgFill = new SVGFill(color);
                    }
                    
                    paintable.svgFill.fillType = FILL_TYPE.SOLID;
                    if(color.a == 1)
                    {
                        paintable.svgFill.blend = FILL_BLEND.OPAQUE;
                    } else {
                        paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
                    }
                }
                    break;
                default:
                    break;
            }

            LibTessDotNet.Tess tesselation = new LibTessDotNet.Tess();
            LibTessDotNet.ContourVertex[] path;
            int pathLength;
            for(int i = 0; i < simplifiedShapes.Count; i++)
            {
                if(simplifiedShapes[i] == null)
                    continue;
                
                pathLength = simplifiedShapes[i].Count;
                path = new LibTessDotNet.ContourVertex[pathLength];
                Vector2 position;
                for(int j = 0; j < pathLength; j++)
                {
                    position = simplifiedShapes[i][j];
                    path[j].Position = new LibTessDotNet.Vec3{X = position.x, Y = position.y, Z = 0f};
                }
                tesselation.AddContour(path);
            }

            tesselation.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3);

            Mesh mesh = new Mesh();
            int meshVertexCount = tesselation.Vertices.Length;
            Vector3[] vertices = new Vector3[meshVertexCount];
            Vector2[] uv = null;
            Vector2[] uv2 = null;

            for(int i = 0; i < meshVertexCount; i++)
            {
                vertices[i] = new Vector3(tesselation.Vertices[i].Position.X, tesselation.Vertices[i].Position.Y, 0f);
            }

            int numTriangles = tesselation.ElementCount;
            int[] triangles = new int[numTriangles * 3];
            for (int i = 0; i < numTriangles; i++)
            {
                triangles[i * 3] = tesselation.Elements[i * 3];
                triangles[i * 3 + 1] = tesselation.Elements[i * 3 + 1];
                triangles[i * 3 + 2] = tesselation.Elements[i * 3 + 2];
            }

            SVGFill svgFill = paintable.svgFill;
            Color32 fillColor = Color.white;
            if (svgFill.fillType != FILL_TYPE.GRADIENT && svgFill.gradientColors == null)
                fillColor = svgFill.color;

            antialiasingMesh = CreateAntialiasing(simplifiedShapes, fillColor, -SVGAssetImport.antialiasingWidth, false, SVGImporter.Utils.ClosePathRule.ALWAYS);

            Color32[] colors32 = new Color32[meshVertexCount];

            for (int i = 0; i < meshVertexCount; i++) 
            {
                colors32 [i].r = fillColor.r;
                colors32 [i].g = fillColor.g;
                colors32 [i].b = fillColor.b;
                colors32 [i].a = fillColor.a;
            }

            if(antialiasingMesh != null)
            {
                Vector3[] antialiasingVertices = antialiasingMesh.vertices;
                Vector2[] antialiasingUV = antialiasingMesh.uv;
                Vector2[] antialiasingUV2 = antialiasingMesh.uv2;
                WriteUVGradientCoordinates(ref antialiasingUV, antialiasingVertices, paintable, bounds);
                WriteUVGradientIndexType(ref antialiasingUV2, antialiasingVertices.Length, paintable);
                antialiasingMesh.uv = antialiasingUV;
                antialiasingMesh.uv2 = antialiasingUV2;
            }

            WriteUVGradientCoordinates(ref uv, vertices, paintable, bounds);
            WriteUVGradientIndexType(ref uv2, meshVertexCount, paintable);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            if(colors32 != null) mesh.colors32 = colors32;
            if(uv != null) mesh.uv = uv;
            if(uv2 != null) mesh.uv2 = uv2;

            return mesh;
        }

        protected static void WriteUVGradientIndexType(ref Vector2[] uv, int meshVertexCount, SVGPaintable svgPaintable)
        {
            SVGFill svgFill = svgPaintable.svgFill;
            if (svgFill.fillType == FILL_TYPE.GRADIENT && svgFill.gradientColors != null)
            {
                Vector2 gradientUV = new Vector2(svgFill.gradientColors.index, (int)svgFill.gradientType);
                uv = new Vector2[meshVertexCount];
                for (int i = 0; i < meshVertexCount; i++) {
                    uv [i].x = gradientUV.x;
                    uv [i].y = gradientUV.y;
                }
            }
        }

        protected static void WriteUVGradientCoordinates(ref Vector2[] uv, Vector3[] vertices, SVGPaintable svgPaintable, Rect bounds)
        {
            SVGFill svgFill = svgPaintable.svgFill;
            if(svgFill.fillType == FILL_TYPE.GRADIENT)
            {
                int meshVertexCount = vertices.Length;

                uv = new Vector2[meshVertexCount];
                Vector2 uvPoint = Vector2.zero;
                SVGMatrix svgFillTransform = GetFillTransform(svgPaintable, bounds);                
                Rect viewport = svgPaintable.viewport;
                for (int i = 0; i < meshVertexCount; i++)
                {
                    uvPoint.x = vertices [i].x;
                    uvPoint.y = vertices [i].y;
                    uvPoint = svgFillTransform.Transform(uvPoint);
                    
                    uv [i].x = (uvPoint.x - viewport.x) / viewport.width;
                    uv [i].y = (uvPoint.y - viewport.y) / viewport.height;
                }
            }
        }

        public static Mesh CreateAntialiasing(List<List<Vector2>> paths, Color color, float antialiasingWidth, bool isStroke = false, ClosePathRule closePath = ClosePathRule.NEVER)
        {
            if(SVGAssetImport.antialiasingWidth <= 0f) return null;
            return SVGSimplePath.CreateAntialiasing(paths, color, antialiasingWidth, closePath);
        }

        private static void UpdateMesh(Mesh mesh, SVGFill svgFill)
        {
            if (svgFill.fillType == FILL_TYPE.GRADIENT && svgFill.gradientColors != null)
            {
                SVGMeshUtils.ChangeMeshUV2(mesh, new Vector2(svgFill.gradientColors.index, (int)svgFill.gradientType));
            } else {
                SVGMeshUtils.ChangeMeshColor(mesh, svgFill.color);
            }
        }

        private static Bounds GetBounds(List<Vector2> array)
        {
            if(array == null || array.Count == 0)
                return new Bounds();

            Bounds bounds = new Bounds();
            bounds.SetMinMax(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), new Vector3(float.MinValue, float.MinValue, float.MinValue));
            int arrayLength = array.Count;
            for(int i = 0; i < arrayLength; i++)
            {
                bounds.Encapsulate(array[i]);
            }

            return bounds;
        }
        
        private static Rect GetRect(List<Vector2> array)
        {
            if(array == null || array.Count == 0)
                return new Rect();

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            int arrayLength = array.Count;
            for(int i = 0; i < arrayLength; i++)
            {
                if(array[i].x < min.x)
                    min.x = array[i].x;
                if(array[i].y < min.y)
                    min.y = array[i].y;
                if(array[i].x > max.x)
                    max.x = array[i].x;
                if(array[i].y > max.y)
                    max.y = array[i].y;
            }
            
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private static Rect GetRect(List<List<Vector2>> array)
        {
            if(array == null || array.Count == 0)
                return new Rect();
            
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            int arrayLength = array.Count;
            int nestedArrayLength;
            for(int i = 0; i < arrayLength; i++)
            {
                if(array[i] == null)
                    continue;

                nestedArrayLength = array[i].Count;
                for(int j = 0; j < nestedArrayLength; j++)
                {
                    if(array[i][j].x < min.x)
                        min.x = array[i][j].x;
                    if(array[i][j].y < min.y)
                        min.y = array[i][j].y;
                    if(array[i][j].x > max.x)
                        max.x = array[i][j].x;
                    if(array[i][j].y > max.y)
                        max.y = array[i][j].y;
                }
            }
            
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private static void OffsetPositions(Bounds bounds, List<Vector2> array)
        {
            if(array == null || array.Count == 0)
                return;

            int arrayLength = array.Count;
            Vector2 boundsCenter = bounds.center;
            for(int i = 0; i < arrayLength; i++)
            {
                array[i] -= boundsCenter;
            }

        }

        protected static Vector2 GetGradientVector(SVGLength posX, SVGLength posY, Rect bounds)
        {
            Vector2 point = Vector2.zero;
            if(posX.unitType != SVGLengthType.Percentage)
            { 
                point.x = posX.value; 
            } else { 
                point.x = bounds.x + bounds.width * (posX.value / 100f); 
            }

            if(posY.unitType != SVGLengthType.Percentage) 
            { 
                point.y = posY.value; 
            } else 
            { 
                point.y = bounds.y + bounds.height * (posY.value / 100f); 
            }

            return point;
        }

        //const float defaultViewportScale = 800f;
        private static SVGMatrix GetFillTransform(SVGPaintable svgPaintable, Rect bounds)
        {
            SVGFill svgFill = svgPaintable.svgFill;
            SVGMatrix transform = new SVGMatrix();
            SVGMatrix gradientMatrix = svgFill.gradientTransform;

            Rect viewport = svgPaintable.viewport;

            if (svgFill.fillType == FILL_TYPE.GRADIENT)
            {
                switch (svgFill.gradientType)
                {
                    case GRADIENT_TYPE.LINEAR:
                    {
                        Vector2 startPoint = GetGradientVector(svgFill.gradientStartX, svgFill.gradientStartY, bounds);
                        Vector2 endPoint = GetGradientVector(svgFill.gradientEndX, svgFill.gradientEndY, bounds);

                        Vector2 gradientVector = endPoint - startPoint;        
                        Vector2 normalizedVector = Vector2.zero;

                        float angle = Mathf.Atan2(gradientVector.y, gradientVector.x) * Mathf.Rad2Deg;
                        Vector2 posDiff = Vector2.Lerp(startPoint, endPoint, 0.5f);

                        float magnitude = gradientVector.magnitude;

                        if(magnitude != 0f)
                        {
                            normalizedVector.x = viewport.width / magnitude;
                            normalizedVector.y = viewport.height / magnitude;
                        }

                        transform = transform.Translate(viewport.center);
                        transform = transform.ScaleNonUniform(normalizedVector.x, normalizedVector.y);
                        transform = transform.Rotate(-angle);
                        transform = transform.Translate(-posDiff);

                        transform = transform.Multiply(gradientMatrix.Inverse());
                        transform = transform.Multiply(svgFill.transform.Inverse());

                        break;
                    }
                    case GRADIENT_TYPE.RADIAL:
                    {
                        Vector2 point = GetGradientVector(svgFill.gradientStartX, svgFill.gradientStartY, bounds);
                        float radius = GetGradientVector(svgFill.gradientEndX, svgFill.gradientEndY, bounds).x;
                        if(svgFill.gradientEndX.unitType == SVGLengthType.Percentage) radius *= 0.5f;

                        float radiusTimesTwo = radius * 2f;

                        Vector2 normalizedVector = Vector2.zero;

                        if(radiusTimesTwo != 0f)
                        {
                            normalizedVector.x = viewport.width / radiusTimesTwo;
                            normalizedVector.y = viewport.height / radiusTimesTwo;
                        }

                        transform = transform.Translate(viewport.center);
                        transform = transform.ScaleNonUniform(normalizedVector.x, normalizedVector.y);
                        transform = transform.Translate(-point);
                        
                        transform = transform.Multiply(gradientMatrix.Inverse());
                        transform = transform.Multiply(svgFill.transform.Inverse());

                        break;
                    }
                    case GRADIENT_TYPE.CONICAL:
                    {
                        Vector2 point = GetGradientVector(svgFill.gradientStartX, svgFill.gradientStartY, bounds);
                        float radius = GetGradientVector(svgFill.gradientEndX, svgFill.gradientEndY, bounds).x;
                        if(svgFill.gradientEndX.unitType == SVGLengthType.Percentage) radius *= 0.5f;

                        float radiusTimesTwo = radius * 2f;
                        
                        Vector2 normalizedVector = Vector2.zero;
                        
                        if(radiusTimesTwo != 0f)
                        {
                            normalizedVector.x = viewport.width / radiusTimesTwo;
                            normalizedVector.y = viewport.height / radiusTimesTwo;
                        }
                        
                        transform = transform.Translate(viewport.center);
                        transform = transform.ScaleNonUniform(normalizedVector.x, normalizedVector.y);
                        transform = transform.Translate(-point);
                        
                        transform = transform.Multiply(gradientMatrix.Inverse());
                        transform = transform.Multiply(svgFill.transform.Inverse());
                        
                        break;
                    }
                }
            }
            
            return transform;
        }

        protected static void AddInputShape(List<List<Vector2>> inputShapes)
        {            
            if(inputShapes == null) return;
            for(int i = 0; i < inputShapes.Count; i++)
            {
                if(inputShapes[i] == null || inputShapes[i].Count == 0) continue;
                SVGGraphics.paths.Add(new SVGPath(inputShapes[i].ToArray()));
            }
            
        }
    }
}
