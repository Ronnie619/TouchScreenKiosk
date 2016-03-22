// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SVGImporter.Utils
{        
    public static class SVGDebug
    {
        public static void DebugPoints(List<List<Vector2>> path)
        {
            GameObject goRoot = new GameObject("Debug Points");
            for(int i = 0; i < path.Count; i++)
            {
                GameObject go = new GameObject("Path");
                go.transform.SetParent(goRoot.transform);
                go.AddComponent<SVGDebugPoints>();
                for(int j = 0; j < path[i].Count; j++)
                {
                    GameObject childGo = new GameObject("Point");
                    childGo.transform.SetParent(go.transform);
                    Vector3 position = path[i][j];
                    position.y *= -1f;
                    childGo.transform.localPosition = position;
                }
            }
        }

        public static void DebugPoints(List<List<Vector3>> path)
        {
            GameObject goRoot = new GameObject("Debug Points");
            for(int i = 0; i < path.Count; i++)
            {
                GameObject go = new GameObject("Path");
                go.transform.SetParent(goRoot.transform);
                go.AddComponent<SVGDebugPoints>();
                for(int j = 0; j < path[i].Count; j++)
                {
                    GameObject childGo = new GameObject("Point");
                    childGo.transform.SetParent(go.transform);
                    Vector3 position = path[i][j];
                    position.y *= -1f;
                    childGo.transform.localPosition = position;
                }
            }
        }

        public static void DebugPoints(List<Vector2> path)
        {
            DebugPoints(new List<List<Vector2>>(){path});
        }

        public static void DebugPoints(List<Vector3> path)
        {
            DebugPoints(new List<List<Vector3>>(){path});
        }

        public static void DebugSegments(StrokeSegment[] segments)
        {
            GameObject goRoot = new GameObject("Debug Segments");
            for(int i = 0; i < segments.Length; i++)
            {
                GameObject go = new GameObject("Segment");
                go.transform.SetParent(goRoot.transform);
                go.AddComponent<SVGDebugPoints>();

                GameObject childGo1 = new GameObject("StartPoint");
                childGo1.transform.SetParent(go.transform);
                Vector3 startPoint = segments[i].startPoint;
                startPoint.y *= -1f;
                childGo1.transform.localPosition = startPoint;

                GameObject childGo2 = new GameObject("EndPoint");
                childGo2.transform.SetParent(go.transform);
                Vector3 endPoint = segments[i].endPoint;
                endPoint.y *= -1f;
                childGo2.transform.localPosition = endPoint;
            }
        }
    }
}
