namespace UglyToad.PdfPig.Tests.Integration
{
    using System.Linq;
    using Xunit;

    /// <summary>
    /// The last object in this document is its cross reference stream and it is not terminated by
    /// an "endobj", the file simply continues into "startxref 849106 %%EOF". Reading the object
    /// therefore ran past its end and collected the trailing "startxref" and offset tokens, and
    /// because the last token read was taken as the object's value the cross reference stream was
    /// seen as a number. The cross reference could then not be read and opening the document threw
    /// a PdfDocumentFormatException carrying no message at all.
    /// </summary>
    public class StreamObjectWithoutEndObjectTests
    {
        private static string GetPath() => IntegrationHelpers.GetDocumentPath("GFAR China Crew 14 Mar 2026.pdf");

        [Fact]
        public void CanOpenDocumentAndReadPages()
        {
            using (var document = PdfDocument.Open(GetPath()))
            {
                Assert.Equal(1, document.NumberOfPages);
            }
        }

        [Fact]
        public void CanOpenDocumentWithLenientParsingDisabled()
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.Equal(1, document.NumberOfPages);
            }
        }

        [Fact]
        public void PageIsRetrievable()
        {
            using (var document = PdfDocument.Open(GetPath()))
            {
                var pages = document.GetPages().ToList();

                Assert.Single(pages);
                Assert.NotNull(pages[0].Content);
            }
        }

        /// <summary>
        /// A batch of documents a customer uploaded together, produced by the same tool and all
        /// missing the closing "endobj" on their cross reference stream. None of them could be
        /// opened before the fix, all of them open now. Strict parsing is used deliberately: the
        /// documents are well formed apart from the missing keyword, so recovering from it must
        /// not depend on lenient parsing.
        /// </summary>
        [Theory]
        [InlineData("GFAR New Zealand ETA 17 Nov 2027.pdf", 1)]
        [InlineData("LTIO Aus ETA Sept 11 2030.pdf", 1)]
        [InlineData("LTIO New Zealand ETA 11 Sept 2026.pdf", 1)]
        [InlineData("MENA New Zealand ETA Nov 28 2027.pdf", 1)]
        [InlineData("RLAC Austrailia ETA 17 Nov 2026.pdf", 1)]
        public void DocumentsMissingEndObjectAreOpenedUnderStrictParsing(string fileName, int expectedPageCount)
        {
            var path = IntegrationHelpers.GetDocumentPath(fileName);

            using (var document = PdfDocument.Open(path, new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.Equal(expectedPageCount, document.NumberOfPages);
                Assert.Equal(expectedPageCount, document.GetPages().Count());
            }
        }
    }
}
