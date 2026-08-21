namespace UglyToad.PdfPig.Tests.Integration
{
    using System.Linq;
    using Xunit;

    /// <summary>
    /// An object stream whose contents are annotated with comments, e.g.
    /// <code>
    /// % 4 0
    /// &lt;&lt; /Type /Page /Parent 3 0 R ... &gt;&gt;
    /// % endobj
    /// % 11 0
    /// </code>
    /// A comment is equivalent to whitespace, so it must be ignored while reading the objects out
    /// of the stream. Reading one token per object without skipping them stored the comment as the
    /// value of an object and shifted every following object onto its neighbour's value, so
    /// resolving the page tree failed with "Could not find the object number 3 0 with type
    /// DictionaryToken instead, it was found with type CommentToken.".
    /// </summary>
    public class ObjectStreamWithCommentsTests
    {
        private static string GetPath() => IntegrationHelpers.GetDocumentPath("RAZ-BM700-208_-C_MDS.pdf");

        [Fact]
        public void CanOpenDocumentAndReadPages()
        {
            using (var document = PdfDocument.Open(GetPath()))
            {
                Assert.Equal(5, document.NumberOfPages);
            }
        }

        [Fact]
        public void EveryPageIsRetrievedFromTheObjectStream()
        {
            using (var document = PdfDocument.Open(GetPath()))
            {
                var pages = document.GetPages().ToList();

                Assert.Equal(5, pages.Count);
                Assert.All(pages, page => Assert.NotNull(page.Content));
            }
        }

        [Fact]
        public void CanOpenDocumentWithLenientParsingDisabled()
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.Equal(5, document.NumberOfPages);
            }
        }
    }
}
