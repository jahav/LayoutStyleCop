// ----------------------------------------------------------------------------
// <copyright file="LayoutAnalyzer.cs" company="https://github.com/jahav">
//   ISC License
// </copyright>
// <license>
//   Copyright (c) 2015, Jan Havlíček (havlicek.honza@gmail.com)
//
//   Permission to use, copy, modify, and/or distribute this software for any
//   purpose with or without fee is hereby granted, provided that the above
//   copyright notice and this permission notice appear in all copies.
//
//   THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
//   WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF 
//   MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE
//   FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY 
//   DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, 
//   WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION,
//   ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS
//   SOFTWARE.
// </license>
// <summary>
//   Checks the layout rules.
// </summary>
// ----------------------------------------------------------------------------
namespace LayoutStyleCop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using StyleCop;
    using StyleCop.CSharp;

    /// <summary>
    /// Layout analyzer of C# files.
    /// </summary>
    [SourceAnalyzer(typeof(CsParser))]
    public class LayoutAnalyzer : SourceAnalyzer
    {
        /// <summary>
        /// Checks the blank lines between using statements within the given document.
        /// </summary>
        /// <param name="document">Document to check.</param>
        public override void AnalyzeDocument(CodeDocument document)
        {
            CsDocument doc = (CsDocument)document;

            // skipping wrong or auto-generated documents
            if (doc.RootElement == null || doc.RootElement.Generated)
            {
                return;
            }

            this.CheckUsingDirectiveOrder(doc.RootElement);
        }

        /// <summary>
        /// Checks the order of using directives within the document.
        /// </summary>
        /// <param name="rootElement">
        /// The root element containing the using directives.
        /// </param>
        private void CheckUsingDirectiveOrder(CsElement rootElement)
        {
            Param.AssertNotNull(rootElement, "rootElement");

            if (!rootElement.Generated)
            {
                this.CheckOrderOfUsingDirectivesUnderElement(rootElement);

                // Find any namespace elements within this element.
                foreach (CsElement childElement in rootElement.ChildElements)
                {
                    if (childElement.ElementType == ElementType.Namespace)
                    {
                        this.CheckUsingDirectiveOrder(childElement);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the order of any using directives found under this element.
        /// </summary>
        /// <param name="element">
        /// The element containing the using directives.
        /// </param>
        private void CheckOrderOfUsingDirectivesUnderElement(CsElement element)
        {
            Param.AssertNotNull(element, "element");

            CsElement previousUsing = null;
            foreach (CsElement childElement in element.ChildElements)
            {
                if (childElement.ElementType == ElementType.UsingDirective
                    || childElement.ElementType == ElementType.ExternAliasDirective)
                {
                    if (previousUsing != null)
                    {
                        // check that two don't have space between them
                        var blankLinesCount = childElement.Location.StartPoint.LineNumber - previousUsing.Location.EndPoint.LineNumber;
                        if (blankLinesCount > 1)
                        {
                            this.AddViolation(childElement, childElement.Location.StartPoint.LineNumber - 1, "UsingMustNotBeSeparatedByBlankLine");
                        }
                    }

                    previousUsing = childElement;
                }
                else
                {
                    // some other element between groups of using element
                    previousUsing = null;
                }
            }
        }
    }
}
