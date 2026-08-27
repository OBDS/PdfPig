namespace UglyToad.PdfPig.Tests.Integration
{
    using System.Linq;
    using Outline;
    using Xunit;

    /// <summary>
    /// Outline nodes in this document point at a /Link annotation instead of an action:
    /// <code>
    /// node   &lt;A, 3270 0&gt;, &lt;Title, (TITLE PAGE)&gt; ...
    /// 3270 0 &lt;A, 3242 0&gt;, &lt;Subtype, /Link&gt;, &lt;Type, /Annot&gt; ...
    /// 3242 0 &lt;A, 3242 0&gt;, &lt;Subtype, /Link&gt;, &lt;Type, /Annot&gt; ...   (references itself)
    /// </code>
    /// There is no action type (/S) to be found down that chain - the last hop points at itself -
    /// so following it further is not an option. Throwing for the missing /S cost us the entire
    /// outline of a 221 page document because of four bad nodes, so a missing action type is now
    /// reported as "no action" and the node falls back to a bookmark without a destination.
    /// </summary>
    public class OutlineActionWithoutTypeTests
    {
        // Kept out of Integration\Documents deliberately: the theories in IntegrationDocumentTests
        // read the annotations of every document there, and this one also carries an annotation
        // with no /Subtype, which AnnotationProvider rejects. That is a separate defect and is not
        // what these tests are about - the service only ever reads the outline.
        private static string GetPath() => IntegrationHelpers.GetSpecificTestDocumentPath("20251219 - N646AK MIP Rev 6.pdf");

        [Fact]
        public void BookmarksAreReadDespiteActionsWithoutType()
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.Equal(221, document.NumberOfPages);

                Assert.True(document.TryGetBookmarks(out var bookmarks));
                Assert.Equal(5, bookmarks.Roots.Count);
                Assert.Equal(47, bookmarks.GetNodes().Count());
            }
        }

        [Fact]
        public void OnlyTheMalformedNodesLoseTheirDestination()
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.True(document.TryGetBookmarks(out var bookmarks));

                var documentNodes = bookmarks.GetNodes().OfType<DocumentBookmarkNode>().ToList();

                // The four nodes whose action cannot be resolved fall back to page 0, the rest of
                // the outline still resolves to real pages.
                Assert.Equal(4, documentNodes.Count(n => n.PageNumber == 0));
                Assert.True(documentNodes.Count(n => n.PageNumber > 0) >= 40);
            }
        }

        [Fact]
        public void TitlesOfTheMalformedNodesAreStillAvailable()
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.True(document.TryGetBookmarks(out var bookmarks));

                var titles = bookmarks.GetNodes().Select(n => n.Title).ToList();

                Assert.Contains("TITLE PAGE", titles);
                Assert.Contains("REVISION RECORD", titles);
                Assert.Contains("TABLE OF CONTENTS", titles);
            }
        }
    }
}
