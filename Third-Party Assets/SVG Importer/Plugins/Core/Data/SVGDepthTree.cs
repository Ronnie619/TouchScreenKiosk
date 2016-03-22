// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SVGImporter.Data
{
    using Geometry;
    using Utils;

    public class SVGDepthTree : System.Object {

        protected QuadTree<SVGMesh> quadTree;

        public SVGDepthTree(SVGBounds bounds)
        {
            quadTree = new QuadTree<SVGMesh>(new SVGBounds(bounds.center, bounds.size * 10f));
        }

        public SVGDepthTree(Rect bounds)
        {
            quadTree = new QuadTree<SVGMesh>(new SVGBounds(bounds.center, bounds.size));
        }

        public SVGMesh[] TestDepthAdd(SVGMesh node, SVGBounds bounds)
        {
            List<QuadTreeNode<SVGMesh>> overlapNodes = quadTree.Intersects(bounds);
            SVGMesh[] output = null;
            if(overlapNodes != null && overlapNodes.Count > 0)
            {
                output = new SVGMesh[overlapNodes.Count];
                for(int i = 0 ; i < output.Length; i++)
                {
                    output[i] = overlapNodes[i].data;
                }
            }

            quadTree.Add(node, bounds);
            return output;
        }

        public void Clear()
        {
            quadTree.Clear();
        }
    }
}