// Derived from https://github.com/OmniSharp/omnisharp-roslyn/blob/d3c7cdc50727c72cebb0f5260d6c860e1984ce46/src/OmniSharp.Roslyn.CSharp/Services/Structure/CodeStructureService.cs
/*
The MIT License (MIT)

Copyright (c) 2015 OmniSharp

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LSIF.Client
{
    public static class TextExtensions
    {
        /// <summary>
        /// Converts a zero-based position in a <see cref="SourceText"/> to an OmniSharp <see cref="Point"/>.
        /// </summary>
        public static Point GetPointFromPosition(this SourceText text, int position)
        {
            var line = text.Lines.GetLineFromPosition(position);

            return new Point
            {
                Line = line.LineNumber,
                Column = position - line.Start
            };
        }

        /// <summary>
        /// Converts a line number and offset to a zero-based position within a <see cref="SourceText"/>.
        /// </summary>
        public static int GetPositionFromLineAndOffset(this SourceText text, int lineNumber, int offset)
            => text.Lines[lineNumber].Start + offset;

        /// <summary>
        /// Converts an OmniSharp <see cref="Point"/> to a zero-based position within a <see cref="SourceText"/>.
        /// </summary>
        public static int GetPositionFromPoint(this SourceText text, Point point)
            => text.GetPositionFromLineAndOffset(point.Line, point.Column);

        /// <summary>
        /// Converts a <see cref="TextSpan"/> in a <see cref="SourceText"/> to an OmniSharp <see cref="Range"/>.
        /// </summary>
        public static Range GetRangeFromSpan(this SourceText text, TextSpan span)
            => new Range
            {
                Start = text.GetPointFromPosition(span.Start),
                End = text.GetPointFromPosition(span.End)
            };

        /// <summary>
        /// Converts an OmniSharp <see cref="Range"/> to a <see cref="TextSpan"/> within a <see cref="SourceText"/>.
        /// </summary>
        public static TextSpan GetSpanFromRange(this SourceText text, Range range)
            => TextSpan.FromBounds(
                start: text.GetPositionFromPoint(range.Start),
                end: text.GetPositionFromPoint(range.End));
    }

    internal static class SymbolExtensions
    {
        public static string GetKindString(this ISymbol symbol)
        {
            switch (symbol)
            {
                case INamespaceSymbol _:
                    return SymbolKinds.Namespace;
                case INamedTypeSymbol namedTypeSymbol:
                    return namedTypeSymbol.GetKindString();
                case IMethodSymbol methodSymbol:
                    return methodSymbol.GetKindString();
                case IFieldSymbol fieldSymbol:
                    return fieldSymbol.GetKindString();
                case IPropertySymbol propertySymbol:
                    return propertySymbol.GetKindString();
                case IEventSymbol _:
                    return SymbolKinds.Event;
                default:
                    return SymbolKinds.Unknown;
            }
        }

        public static string GetAccessibilityString(this ISymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    return SymbolAccessibilities.Public;
                case Accessibility.Internal:
                    return SymbolAccessibilities.Internal;
                case Accessibility.Private:
                    return SymbolAccessibilities.Private;
                case Accessibility.Protected:
                    return SymbolAccessibilities.Protected;
                case Accessibility.ProtectedOrInternal:
                    return SymbolAccessibilities.ProtectedInternal;
                case Accessibility.ProtectedAndInternal:
                    return SymbolAccessibilities.PrivateProtected;
                default:
                    return null;
            }
        }
    }

    public static class SymbolAccessibilities
    {
        public static readonly string Internal = nameof(Internal).ToLowerInvariant();
        public static readonly string Private = nameof(Private).ToLowerInvariant();
        public static readonly string PrivateProtected = $"{Private} {Protected}";
        public static readonly string Protected = nameof(Protected).ToLowerInvariant();
        public static readonly string ProtectedInternal = $"{Protected} {Internal}";
        public static readonly string Public = nameof(Public).ToLowerInvariant();
    }

    public static class SymbolKinds
    {
        // types
        public static readonly string Class = nameof(Class).ToLowerInvariant();
        public static readonly string Delegate = nameof(Delegate).ToLowerInvariant();
        public static readonly string Enum = nameof(Enum).ToLowerInvariant();
        public static readonly string Interface = nameof(Interface).ToLowerInvariant();
        public static readonly string Struct = nameof(Struct).ToLowerInvariant();

        // members
        public static readonly string Constant = nameof(Constant).ToLowerInvariant();
        public static readonly string Constructor = nameof(Constructor).ToLowerInvariant();
        public static readonly string Destructor = nameof(Destructor).ToLowerInvariant();
        public static readonly string EnumMember = nameof(EnumMember).ToLowerInvariant();
        public static readonly string Event = nameof(Event).ToLowerInvariant();
        public static readonly string Field = nameof(Field).ToLowerInvariant();
        public static readonly string Indexer = nameof(Indexer).ToLowerInvariant();
        public static readonly string Method = nameof(Method).ToLowerInvariant();
        public static readonly string Operator = nameof(Operator).ToLowerInvariant();
        public static readonly string Property = nameof(Property).ToLowerInvariant();

        // other
        public static readonly string Namespace = nameof(Namespace).ToLowerInvariant();
        public static readonly string Unknown = nameof(Unknown).ToLowerInvariant();
    }

    public static class SymbolDisplayFormats
    {
        public static readonly SymbolDisplayFormat ShortTypeFormat = new SymbolDisplayFormat(
         typeQualificationStyle:
             SymbolDisplayTypeQualificationStyle.NameOnly,
         genericsOptions:
             SymbolDisplayGenericsOptions.IncludeTypeParameters);

        public static readonly SymbolDisplayFormat TypeFormat = new SymbolDisplayFormat(
            typeQualificationStyle:
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters |
                SymbolDisplayGenericsOptions.IncludeVariance);

        public static readonly SymbolDisplayFormat ShortMemberFormat = new SymbolDisplayFormat(
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters);

        public static readonly SymbolDisplayFormat MemberFormat = new SymbolDisplayFormat(
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters |
                SymbolDisplayGenericsOptions.IncludeVariance,
            memberOptions:
                SymbolDisplayMemberOptions.IncludeParameters,
            parameterOptions:
                SymbolDisplayParameterOptions.IncludeDefaultValue |
                SymbolDisplayParameterOptions.IncludeExtensionThis |
                SymbolDisplayParameterOptions.IncludeName |
                SymbolDisplayParameterOptions.IncludeParamsRefOut |
                SymbolDisplayParameterOptions.IncludeType,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
    }

    public static class SymbolRangeNames
    {
        public static readonly string Attributes = nameof(Attributes).ToLowerInvariant();
        public static readonly string Full = nameof(Full).ToLowerInvariant();
        public static readonly string Name = nameof(Name).ToLowerInvariant();
    }

    public static class SymbolPropertyNames
    {
        public static readonly string Accessibility = nameof(Accessibility).ToLowerInvariant();
        public static readonly string Static = nameof(Static).ToLowerInvariant();
    }

    public class DocumentSymbolProvider
    {
        public async Task<IReadOnlyList<CodeElement>> GetCodeElementsAsync(Document document)
        {
            var text = await document.GetTextAsync();
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            var results = ImmutableList.CreateBuilder<CodeElement>();

            foreach (var node in ((CompilationUnitSyntax)syntaxRoot).Members)
            {
                foreach (var element in CreateCodeElements(node, text, semanticModel))
                {
                    if (element != null)
                    {
                        results.Add(element);
                    }
                }
            }

            return results.ToImmutable();
        }

        private IEnumerable<CodeElement> CreateCodeElements(SyntaxNode node, SourceText text, SemanticModel semanticModel)
        {
            switch (node)
            {
                case TypeDeclarationSyntax typeDeclaration:
                    yield return CreateCodeElement(typeDeclaration, text, semanticModel);
                    break;
                case DelegateDeclarationSyntax delegateDeclaration:
                    yield return CreateCodeElement(delegateDeclaration, text, semanticModel);
                    break;
                case EnumDeclarationSyntax enumDeclaration:
                    yield return CreateCodeElement(enumDeclaration, text, semanticModel);
                    break;
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    yield return CreateCodeElement(namespaceDeclaration, text, semanticModel);
                    break;
                case BaseMethodDeclarationSyntax baseMethodDeclaration:
                    yield return CreateCodeElement(baseMethodDeclaration, text, semanticModel);
                    break;
                case BasePropertyDeclarationSyntax basePropertyDeclaration:
                    yield return CreateCodeElement(basePropertyDeclaration, text, semanticModel);
                    break;
                case BaseFieldDeclarationSyntax baseFieldDeclaration:
                    foreach (var variableDeclarator in baseFieldDeclaration.Declaration.Variables)
                    {
                        yield return CreateCodeElement(variableDeclarator, baseFieldDeclaration, text, semanticModel);
                    }

                    break;
                case EnumMemberDeclarationSyntax enumMemberDeclarationSyntax:
                    yield return CreateCodeElement(enumMemberDeclarationSyntax, text, semanticModel);
                    break;
            }
        }

        private CodeElement CreateCodeElement(TypeDeclarationSyntax typeDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortTypeFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.TypeFormat)
            };

            AddRanges(builder, typeDeclaration.AttributeLists.Span, typeDeclaration.Span, typeDeclaration.Identifier.Span, text);
            AddSymbolProperties(symbol, builder);

            foreach (var member in typeDeclaration.Members)
            {
                foreach (var childElement in CreateCodeElements(member, text, semanticModel))
                {
                    builder.AddChild(childElement);
                }
            }

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(DelegateDeclarationSyntax delegateDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(delegateDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortTypeFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.TypeFormat),
            };

            AddRanges(builder, delegateDeclaration.AttributeLists.Span, delegateDeclaration.Span, delegateDeclaration.Identifier.Span, text);
            AddSymbolProperties(symbol, builder);

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(EnumDeclarationSyntax enumDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(enumDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortTypeFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.TypeFormat),
            };

            AddRanges(builder, enumDeclaration.AttributeLists.Span, enumDeclaration.Span, enumDeclaration.Identifier.Span, text);
            AddSymbolProperties(symbol, builder);

            foreach (var member in enumDeclaration.Members)
            {
                foreach (var childElement in CreateCodeElements(member, text, semanticModel))
                {
                    builder.AddChild(childElement);
                }
            }

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(NamespaceDeclarationSyntax namespaceDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(namespaceDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortTypeFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.TypeFormat),
            };

            AddRanges(builder, attributesSpan: default, namespaceDeclaration.Span, namespaceDeclaration.Name.Span, text);

            foreach (var member in namespaceDeclaration.Members)
            {
                foreach (var childElement in CreateCodeElements(member, text, semanticModel))
                {
                    builder.AddChild(childElement);
                }
            }

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(BaseMethodDeclarationSyntax baseMethodDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(baseMethodDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortMemberFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.MemberFormat),
            };

            AddRanges(builder, baseMethodDeclaration.AttributeLists.Span, baseMethodDeclaration.Span, GetNameSpan(baseMethodDeclaration), text);
            AddSymbolProperties(symbol, builder);

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(BasePropertyDeclarationSyntax basePropertyDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(basePropertyDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortMemberFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.MemberFormat),
            };

            AddRanges(builder, basePropertyDeclaration.AttributeLists.Span, basePropertyDeclaration.Span, GetNameSpan(basePropertyDeclaration), text);
            AddSymbolProperties(symbol, builder);

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(VariableDeclaratorSyntax variableDeclarator, BaseFieldDeclarationSyntax baseFieldDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(variableDeclarator);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortMemberFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.MemberFormat),
            };

            AddRanges(builder, baseFieldDeclaration.AttributeLists.Span, variableDeclarator.Span, variableDeclarator.Identifier.Span, text);
            AddSymbolProperties(symbol, builder);

            return builder.ToCodeElement();
        }

        private CodeElement CreateCodeElement(EnumMemberDeclarationSyntax enumMemberDeclaration, SourceText text, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(enumMemberDeclaration);
            if (symbol == null)
            {
                return null;
            }

            var builder = new CodeElement.Builder
            {
                Kind = symbol.GetKindString(),
                Name = symbol.ToDisplayString(SymbolDisplayFormats.ShortMemberFormat),
                DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.MemberFormat),
            };

            AddRanges(builder, enumMemberDeclaration.AttributeLists.Span, enumMemberDeclaration.Span, enumMemberDeclaration.Identifier.Span, text);
            AddSymbolProperties(symbol, builder);

            return builder.ToCodeElement();
        }

        private static TextSpan GetNameSpan(BaseMethodDeclarationSyntax baseMethodDeclaration)
        {
            switch (baseMethodDeclaration)
            {
                case MethodDeclarationSyntax methodDeclaration:
                    return methodDeclaration.Identifier.Span;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    return constructorDeclaration.Identifier.Span;
                case DestructorDeclarationSyntax destructorDeclaration:
                    return destructorDeclaration.Identifier.Span;
                case OperatorDeclarationSyntax operatorDeclaration:
                    return operatorDeclaration.OperatorToken.Span;
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    return conversionOperatorDeclaration.Type.Span;
                default:
                    return default;
            }
        }

        private static TextSpan GetNameSpan(BasePropertyDeclarationSyntax basePropertyDeclaration)
        {
            switch (basePropertyDeclaration)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                    return propertyDeclaration.Identifier.Span;
                case EventDeclarationSyntax eventDeclaration:
                    return eventDeclaration.Identifier.Span;
                case IndexerDeclarationSyntax indexerDeclaration:
                    return indexerDeclaration.ThisKeyword.Span;
                default:
                    return default;
            }
        }

        private static void AddRanges(CodeElement.Builder builder, TextSpan attributesSpan, TextSpan fullSpan, TextSpan nameSpan, SourceText text)
        {
            if (attributesSpan != default)
            {
                builder.AddRange(SymbolRangeNames.Attributes, text.GetRangeFromSpan(attributesSpan));
            }

            if (fullSpan != default)
            {
                builder.AddRange(SymbolRangeNames.Full, text.GetRangeFromSpan(fullSpan));
            }

            if (nameSpan != default)
            {
                builder.AddRange(SymbolRangeNames.Name, text.GetRangeFromSpan(nameSpan));
            }
        }

        private void AddSymbolProperties(ISymbol symbol, CodeElement.Builder builder)
        {
            var accessibility = symbol.GetAccessibilityString();
            if (accessibility != null)
            {
                builder.AddProperty(SymbolPropertyNames.Accessibility, accessibility);
            }

            builder.AddProperty(SymbolPropertyNames.Static, symbol.IsStatic);
        }
    }
}