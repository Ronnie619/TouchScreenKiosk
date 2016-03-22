// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System;
using System.Collections.Generic;

namespace SVGImporter.Rendering
{
    using Data;
    using Geometry;

    public class SVGGraphics
    {
        public static SVGDepthTree depthTree;
        public static List<SVGPath> paths;

        public static void AddMesh(SVGMesh mesh)
        {
            meshes.Add(mesh);
        }

        public static List<Vector2> position_buffer;
        public static void DebugPositionBuffer()
        {
            if(position_buffer == null || position_buffer.Count == 0)
                return;
            
            string output = "";
            for(int i = 0; i < position_buffer.Count; i++)
            {
                output += position_buffer[i].ToString()+", ";
            }
            
            UnityEngine.Debug.Log(output);
        }

        public static List<SVGMesh> meshes;
        protected static float _vpm;
        public static float vpm
        {
            get {
                return _vpm;
            }
        }

        public static float _roundQuality = 0f;
        public static float roundQuality
        {
            get {
                return _roundQuality;
            }
        }

        private float _vertexPerMeter = 1000f;
        public float vertexPerMeter
        {
            get {           
                return _vertexPerMeter;
            }
        }

        public static void Clear()
        {
            if(position_buffer != null)
            {
                position_buffer.Clear();
                position_buffer = null;
            }
            if(meshes != null)
            {
                meshes.Clear();
                meshes = null;
            }
            
            if(depthTree != null)
            {
                depthTree.Clear();
                depthTree = null;
            }

            if(paths != null)
            {
                paths.Clear();
                paths = null;
            }
        }

        public static void Init()
        {
            if(position_buffer == null)
                position_buffer = new List<Vector2>();
            if(meshes == null)
                meshes = new List<SVGMesh>();
            if(paths == null)
                paths = new List<SVGPath>();
        }

        private SVGStrokeLineCapMethod   _strokeLineCap = SVGStrokeLineCapMethod.Unknown;
        private SVGStrokeLineJoinMethod  _strokeLineJoin = SVGStrokeLineJoinMethod.Unknown;

        public SVGStrokeLineCapMethod strokeLineCap
        {
            get { return this._strokeLineCap; }
        }

        public SVGStrokeLineJoinMethod strokeLineJoin
        {
            get { return this._strokeLineJoin; }
        }

        public SVGGraphics(float vertexPerMeter = 1000f)
        {
            _vpm = 1f;
            if(vertexPerMeter > 0f)
			{
                _vpm = 1000f / vertexPerMeter;
			} else {
				_vpm = 1000f;
			}

            if(_vpm != 0f)
            {
                _roundQuality = (1f / _vpm) * 0.5f;
            } else {
                _roundQuality = 0f;
            }

            this._vertexPerMeter = vertexPerMeter;
        }

        public void SetStrokeLineCap(SVGStrokeLineCapMethod strokeLineCap)
        {
            this._strokeLineCap = strokeLineCap;
        }

        public void SetStrokeLineJoin(SVGStrokeLineJoinMethod strokeLineJoin)
        {
            this._strokeLineJoin = strokeLineJoin;
        }

        public bool GetThickLine(Vector2 p1, Vector2 p2, float width,
                ref Vector2 rp1, ref Vector2 rp2, ref Vector2 rp3, ref Vector2 rp4)
        {

            float cx1, cy1, cx2, cy2, cx3, cy3, cx4, cy4;
            float dtx, dty, temp, _half;
            int _ihalf1, _ihalf2;

            _half = width / 2f;
            _ihalf1 = (int)_half;
            _ihalf2 = (int)(width - _ihalf1 + 0.5f);

            dtx = p2.x - p1.x;
            dty = p2.y - p1.y;
            temp = dtx * dtx + dty * dty;
            if (temp == 0f)
            {
                rp1.x = p1.x - _ihalf2;
                rp1.y = p1.y + _ihalf2;

                rp2.x = p1.x - _ihalf2;
                rp2.y = p1.y - _ihalf2;

                rp3.x = p1.x + _ihalf1;
                rp3.y = p1.y + _ihalf1;

                rp4.x = p1.x + _ihalf1;
                rp4.y = p1.y - _ihalf1;
                return false;
            }

            cy1 = _ihalf1 * dtx / (float)Math.Sqrt(temp) + p1.y;
            if (dtx == 0)
            {
                if (dty > 0)
                {
                    cx1 = p1.x - _ihalf1;
                } else
                {
                    cx1 = p1.x + _ihalf1;
                }
            } else
            {
                cx1 = (-(cy1 - p1.y) * dty) / dtx + p1.x;
            }

            cy2 = -(_ihalf2 * dtx / (float)Math.Sqrt(temp)) + p1.y;
            if (dtx == 0)
            {
                if (dty > 0)
                {
                    cx2 = p1.x + _ihalf2;
                } else
                {
                    cx2 = p1.x - _ihalf2;
                }
            } else
            {
                cx2 = (-(cy2 - p1.y) * dty) / dtx + p1.x;
            }

            dtx = p1.x - p2.x;
            dty = p1.y - p2.y;
            temp = dtx * dtx + dty * dty;

            cy3 = _ihalf1 * dtx / (float)Math.Sqrt(temp) + p2.y;
            if (dtx == 0)
            {
                if (dty > 0)
                {
                    cx3 = p2.x - _ihalf1;
                } else
                {
                    cx3 = p2.x + _ihalf1;
                }
            } else
            {
                cx3 = (-(cy3 - p2.y) * dty) / dtx + p2.x;
            }

            cy4 = -(_ihalf2 * dtx / (float)Math.Sqrt(temp)) + p2.y;

            if (dtx == 0)
            {
                if (dty > 0)
                {
                    cx4 = p2.x + _ihalf2;
                } else
                {
                    cx4 = p2.x - _ihalf2;
                }
            } else
            {
                cx4 = (-(cy4 - p2.y) * dty) / dtx + p2.x;
            }

            rp1.x = cx1;
            rp1.y = cy1;

            rp2.x = cx2;
            rp2.y = cy2;

            float t1, t2;
            t1 = ((p1.y - cy1) * (p2.x - p1.x)) - ((p1.x - cx1) * (p2.y - p1.y));
            t2 = ((p1.y - cy4) * (p2.x - p1.x)) - ((p1.x - cx4) * (p2.y - p1.y));
            if (t1 * t2 > 0)
            {
                //bi lech
                if (_ihalf1 != _ihalf2)
                {
                    cy3 = _ihalf2 * dtx / (float)Math.Sqrt(temp) + p2.y;
                    if (dtx == 0)
                    {
                        if (dty > 0)
                        {
                            cx3 = p2.x - _ihalf2;
                        } else
                        {
                            cx3 = p2.x + _ihalf2;
                        }
                    } else
                    {
                        cx3 = (-(cy3 - p2.y) * dty) / dtx + p2.x;
                    }

                    cy4 = -(_ihalf1 * dtx / (float)Math.Sqrt(temp)) + p2.y;

                    if (dtx == 0)
                    {
                        if (dty > 0)
                        {
                            cx4 = p2.x + _ihalf1;
                        } else
                        {
                            cx4 = p2.x - _ihalf1;
                        }
                    } else
                    {
                        cx4 = (-(cy4 - p2.y) * dty) / dtx + p2.x;
                    }
                }

                rp3.x = cx4;
                rp3.y = cy4;
                rp4.x = cx3;
                rp4.y = cy3;
            } else
            {
                rp3.x = cx3;
                rp3.y = cy3;
                rp4.x = cx4;
                rp4.y = cy4;
            }
            return true;
        }

        public Vector2 GetCrossPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {

            Vector2 _return = new Vector2(0f, 0f);
            float a1 = 0f, b1 = 0f, a2 = 0f, b2 = 0f;

            float dx1, dy1, dx2, dy2;
            dx1 = p1.x - p2.x;
            dy1 = p1.y - p2.y;
            dx2 = p3.x - p4.x;
            dy2 = p3.y - p4.y;

            if (dx1 != 0f)
            {
                a1 = dy1 / dx1;
                b1 = p1.y - a1 * p1.x;
            }

            if (dx2 != 0)
            {
                a2 = dy2 / dx2;
                b2 = p3.y - a2 * p3.x;
            }

            float tx = 0f, ty = 0f;

            if ((a1 == a2) && (b1 == b2))
            {
                Vector2 t_p1 = p1;
                Vector2 t_p2 = p1;
                if (dx1 == 0f)
                {
                    if (p2.y < t_p1.y)
                        t_p1 = p2;
                    if (p3.y < t_p1.y)
                        t_p1 = p3;
                    if (p4.y < t_p1.y)
                        t_p1 = p4;

                    if (p2.y > t_p2.y)
                        t_p2 = p2;
                    if (p3.y > t_p2.y)
                        t_p2 = p3;
                    if (p4.y > t_p2.y)
                        t_p2 = p4;
                } else
                {
                    if (p2.x < t_p1.x)
                        t_p1 = p2;
                    if (p3.x < t_p1.x)
                        t_p1 = p3;
                    if (p4.x < t_p1.x)
                        t_p1 = p4;

                    if (p2.x > t_p2.x)
                        t_p2 = p2;
                    if (p3.x > t_p2.x)
                        t_p2 = p3;
                    if (p4.x > t_p2.x)
                        t_p2 = p4;
                }

                tx = (t_p1.x - t_p2.x) / 2f;
                tx = t_p2.x + tx;

                ty = (t_p1.y - t_p2.y) / 2f;
                ty = t_p2.y + ty;

                _return.x = tx;
                _return.y = ty;
                return _return;
            }



            if ((dx1 != 0) && (dx2 != 0))
            {
                tx = -(b1 - b2) / (a1 - a2);
                ty = a1 * tx + b1;
            } else if ((dx1 == 0) && (dx2 != 0))
            {
                tx = p1.x;
                ty = a2 * tx + b2;
            } else if ((dx1 != 0) && (dx2 == 0))
            {
                tx = p3.x;
                ty = a1 * tx + b1;
            }

            _return.x = tx;
            _return.y = ty;
            return _return;
        }

        public float AngleBetween2Vector(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Vector2 vt1, vt2;
            vt1 = new Vector2(p2.x - p1.x, p2.y - p1.y);
            vt2 = new Vector2(p4.x - p3.x, p4.y - p3.y);
            float t1 = vt1.x * vt2.x + vt1.y * vt2.y;
            float gtvt1 = (float)Math.Sqrt(vt1.x * vt1.x + vt1.y * vt1.y);
            float gtvt2 = (float)Math.Sqrt(vt2.x * vt2.x + vt2.y * vt2.y);
            float t2 = gtvt1 * gtvt2;
            float cosAngle = t1 / t2;

            return((float)Math.Acos(cosAngle));
        }

    }
}
