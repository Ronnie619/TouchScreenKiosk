// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace SVGImporter.Utils
{
    public enum SVGLengthType : ushort
    {
        Unknown = 0,
        Number = 1,
        Percentage = 2,
        EMs = 3,
        EXs = 4,
        PX = 5,
        CM = 6,
        MM = 7,
        IN = 8,
        PT = 9,
        PC = 10,
    }

    public struct SVGLength
    {
        private SVGLengthType _unitType;
        private float _valueInSpecifiedUnits, _value;

        public float value
        {
            get { return _value; }
        }

        public SVGLengthType unitType
        {
            get { return _unitType; }
        }

        public SVGLength(SVGLengthType unitType, float valueInSpecifiedUnits)
        {
            _unitType = unitType;
            _valueInSpecifiedUnits = valueInSpecifiedUnits;
            _value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
        }

        public SVGLength(float valueInSpecifiedUnits)
        {
            _unitType = SVGLengthType.Number;
            _valueInSpecifiedUnits = valueInSpecifiedUnits;
            _value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
        }

        public SVGLength(string valueText)
        {
            float t_value = 0.0f;
            SVGLengthType t_type = SVGLengthType.Unknown;
            SVGLengthConvertor.ExtractType(valueText, ref t_value, ref t_type);
            _unitType = t_type;
            _valueInSpecifiedUnits = t_value;
            _value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
        }

        public void NewValueSpecifiedUnits(float valueInSpecifiedUnits)
        {
            _unitType = (SVGLengthType)0;
            _valueInSpecifiedUnits = valueInSpecifiedUnits;
            _value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
        }

        public static float GetPXLength(string valueText)
        {
            float t_value = 0.0f;
            SVGLengthType t_type = SVGLengthType.Unknown;
            SVGLengthConvertor.ExtractType(valueText, ref t_value, ref t_type);
            return SVGLengthConvertor.ConvertToPX(t_value, t_type);
        }

        public SVGLength Multiply(SVGLength svglength)
        {
            if(unitType == SVGLengthType.Percentage && svglength.unitType == SVGLengthType.Percentage)
            {
                return new SVGLength(SVGLengthType.Percentage, this.value * svglength.value);                    
            } else {
                return new SVGLength(SVGLengthType.PX, this.value * svglength.value);
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
