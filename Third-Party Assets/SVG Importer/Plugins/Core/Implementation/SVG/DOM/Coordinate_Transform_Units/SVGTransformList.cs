// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;

namespace SVGImporter.Rendering
{
    using Document;
    using Utils;

    public class SVGTransformList
    {
        private List<SVGTransform> _listTransform;

        public int Count
        {
            get { return _listTransform.Count; }
        }

        private SVGMatrix _totalMatrix = null;

        public SVGMatrix totalMatrix
        {
            get
            {
                if (_totalMatrix == null)
                {
                    if (_listTransform.Count == 0)
                    {
                        _totalMatrix = new SVGMatrix(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
                    } else
                    {
                        SVGMatrix matrix = _listTransform [0].matrix;
                        for (int i = 1; i < _listTransform.Count; i++)
                            matrix = matrix.Multiply(_listTransform [i].matrix);
                        _totalMatrix = matrix;
                    }
                }
                return _totalMatrix;
            }
        }

        public SVGTransformList()
        {
            _listTransform = new List<SVGTransform>();
        }

        public SVGTransformList(int capacity)
        {
            _listTransform = new List<SVGTransform>(capacity);
        }

        public SVGTransformList(string listString)
        {
            _listTransform = SVGStringExtractor.ExtractTransformList(listString);
        }

        public void Clear()
        {
            _listTransform.Clear();
            _totalMatrix = null;
        }

        public void AppendItem(SVGTransform newItem)
        {
            _listTransform.Add(newItem);
            _totalMatrix = null;
        }

        
        public void AppendItemAt(SVGTransform newItem, int index)
        {
            _listTransform.Insert(index, newItem);
            _totalMatrix = null;
        }

        public void AppendItems(SVGTransformList newListItem)
        {
            _listTransform.AddRange(newListItem._listTransform);
            _totalMatrix = null;
        }

        public void AppendItemsAt(SVGTransformList newListItem, int index)
        {
            _listTransform.InsertRange(index, newListItem._listTransform);
            _totalMatrix = null;
        }

        public SVGTransform this [int index]
        {
            get
            {
                if ((index < 0) || (index >= _listTransform.Count))
                    throw new DOMException(DOMExceptionType.IndexSizeErr);
                return _listTransform [index];
            }
        }

        public SVGTransform Consolidate()
        {
            return new SVGTransform(totalMatrix);
        }
    }
}
