// Derived from https://github.com/OmniSharp/omnisharp-roslyn/blob/d3c7cdc50727c72cebb0f5260d6c860e1984ce46/src/OmniSharp.Abstractions/Models/v2/Range.cs
/*
The MIT License (MIT)

Copyright (c) 2015 OmniSharp

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;

namespace LSIF.Client
{
    public class Range : IEquatable<Range>
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public bool Contains(int line, int column)
        {
            if (Start.Line > line || End.Line < line)
            {
                return false;
            }

            if (Start.Line == line && Start.Column > column)
            {
                return false;
            }

            if (End.Line == line && End.Column < column)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
            => Equals(obj as Range);

        public bool Equals(Range other)
            => other != null
                && EqualityComparer<Point>.Default.Equals(Start, other.Start)
                && EqualityComparer<Point>.Default.Equals(End, other.End);

        public override int GetHashCode()
        {
            var hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + EqualityComparer<Point>.Default.GetHashCode(Start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Point>.Default.GetHashCode(End);
            return hashCode;
        }

        public override string ToString()
            => $"Start = {{{Start}}}, End = {{{End}}}";

        public static bool operator ==(Range range1, Range range2)
            => EqualityComparer<Range>.Default.Equals(range1, range2);

        public static bool operator !=(Range range1, Range range2)
            => !(range1 == range2);
    }
}