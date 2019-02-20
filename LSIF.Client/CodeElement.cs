// Derived from https://raw.githubusercontent.com/OmniSharp/omnisharp-roslyn/d3c7cdc50727c72cebb0f5260d6c860e1984ce46/src/OmniSharp.Abstractions/Models/v2/CodeStructure/CodeElement.cs
/*
The MIT License (MIT)

Copyright (c) 2015 OmniSharp

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Collections.Immutable;

namespace LSIF.Client
{
    public partial class CodeElement
    {
        public string Kind { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public IReadOnlyList<CodeElement> Children { get; }
        public IReadOnlyDictionary<string, Range> Ranges { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }

        private CodeElement(
            string kind, string name, string displayName,
            IReadOnlyList<CodeElement> children,
            IReadOnlyDictionary<string, Range> ranges,
            IReadOnlyDictionary<string, object> properties)
        {
            Kind = kind;
            Name = name;
            DisplayName = displayName;
            Children = children;
            Ranges = ranges;
            Properties = properties;
        }

        public override string ToString()
            => $"{Kind} {Name}";

        public class Builder
        {
            private ImmutableList<CodeElement>.Builder _childrenBuilder;
            private ImmutableDictionary<string, Range>.Builder _rangesBuilder;
            private ImmutableDictionary<string, object>.Builder _propertiesBuilder;

            public string Kind { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }

            public void AddChild(CodeElement element)
            {
                if (_childrenBuilder == null)
                {
                    _childrenBuilder = ImmutableList.CreateBuilder<CodeElement>();
                }

                _childrenBuilder.Add(element);
            }

            public void AddRange(string name, Range range)
            {
                if (_rangesBuilder == null)
                {
                    _rangesBuilder = ImmutableDictionary.CreateBuilder<string, Range>();
                }

                _rangesBuilder.Add(name, range);
            }

            public void AddProperty(string name, object value)
            {
                if (_propertiesBuilder == null)
                {
                    _propertiesBuilder = ImmutableDictionary.CreateBuilder<string, object>();
                }

                _propertiesBuilder.Add(name, value);
            }

            public CodeElement ToCodeElement()
            {
                return new CodeElement(
                    Kind, Name, DisplayName,
                    _childrenBuilder?.ToImmutable(),
                    _rangesBuilder?.ToImmutable(),
                    _propertiesBuilder?.ToImmutable());
            }
        }
    }
}