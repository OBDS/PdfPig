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
    }
}
