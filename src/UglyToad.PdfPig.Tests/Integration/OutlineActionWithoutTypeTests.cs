namespace UglyToad.PdfPig.Tests.Integration
{
    using System.Linq;
    using Outline;
    using Xunit;

    /// <summary>
    /// This document's outline resolves correctly only when indirect objects held in object streams
    /// are read by their recorded byte offsets. Reading them sequentially instead returned a
    /// neighbouring object's content, so outline nodes appeared to point at /Link annotations
    /// rather than actions, and destinations that did resolve landed on the wrong page: object
    /// 3272 was read as 3251's content, putting "REVISION TRANSMITAL" on page 212 instead of 8.
    /// Every node in this document has a real destination - none of them should fall back.
    /// </summary>
    public class OutlineActionWithoutTypeTests
    {
        // Kept out of Integration\Documents deliberately: the theories in IntegrationDocumentTests
        // read the annotations of every document there, and this one also carries an annotation
        // with no /Subtype, which AnnotationProvider rejects. That is a separate defect and is not
        // what these tests are about - the service only ever reads the outline.
        private static string GetPath() => IntegrationHelpers.GetSpecificTestDocumentPath("20251219 - N646AK MIP Rev 6.pdf");

        [Fact]
        public void BookmarksAreRead()
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
        public void EveryBookmarkResolvesToARealPage()
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.True(document.TryGetBookmarks(out var bookmarks));

                var documentNodes = bookmarks.GetNodes().OfType<DocumentBookmarkNode>().ToList();

                Assert.NotEmpty(documentNodes);
                Assert.DoesNotContain(documentNodes, n => n.PageNumber <= 0);
                Assert.DoesNotContain(documentNodes, n => n.PageNumber > document.NumberOfPages);
            }
        }

        [Theory]
        [InlineData("TITLE PAGE", 7)]
        [InlineData("REVISION TRANSMITAL", 8)]
        [InlineData("REVISION RECORD", 9)]
        [InlineData("REASON FOR REVISION", 10)]
        [InlineData("TABLE OF CONTENTS", 12)]
        public void FrontMatterBookmarksPointAtTheirOwnPages(string title, int expectedPageNumber)
        {
            using (var document = PdfDocument.Open(GetPath(), new ParsingOptions { UseLenientParsing = false }))
            {
                Assert.True(document.TryGetBookmarks(out var bookmarks));

                var node = bookmarks.GetNodes()
                    .OfType<DocumentBookmarkNode>()
                    .Single(n => n.Title == title);

                Assert.Equal(expectedPageNumber, node.PageNumber);
            }
        }
    }
}
