using System;

namespace GeomPad
{
    public class GeomPadException : Exception
    {
        public GeomPadException() : base() { }
        public GeomPadException(string str) : base(str) { }
    }
}