// Derived from https://github.com/OmniSharp/omnisharp-roslyn/blob/d3c7cdc50727c72cebb0f5260d6c860e1984ce46/src/OmniSharp.Abstractions/Models/v2/Point.cs
/*
The MIT License (MIT)

Copyright (c) 2015 OmniSharp

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LSIF.Client
{
    public class Point : IEquatable<Point>
    {
        public int Line { get; set; }
        public int Column { get; set; }

        public override bool Equals(object obj)
            => Equals(obj as Point);

        public bool Equals(Point other)
            => other != null
                && Line == other.Line
                && Column == other.Column;

        public override int GetHashCode()
        {
            var hashCode = -1456208474;
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            return hashCode;
        }

        public override string ToString()
            => $"Line = {Line}, Column = {Column}";

        public static bool operator ==(Point point1, Point point2)
            => EqualityComparer<Point>.Default.Equals(point1, point2);

        public static bool operator !=(Point point1, Point point2)
            => !(point1 == point2);
    }
}