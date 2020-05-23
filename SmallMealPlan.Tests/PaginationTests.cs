using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace SmallMealPlan.Tests
{
    public class PaginationTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [InlineData(1, 3)]
        [InlineData(2, 3)]
        [InlineData(3, 3)]
        [InlineData(1, 10)]
        [InlineData(5, 10)]
        [InlineData(10, 10)]
        public void Up_to_10_pages_should_show_all_pages(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, Enumerable.Range(1, pageCount).ToArray()));

        [Theory]
        [InlineData(1, 11)]
        [InlineData(2, 11)]
        [InlineData(3, 11)]
        [InlineData(4, 11)]
        [InlineData(5, 11)]
        public void Given_11_pages_should_skip_pages_9_and_10_when_on_pages_1_to_5(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 2, 3, 4, 5, 6, 7, 8, 11));

        [Theory]
        [InlineData(6, 11)]
        [InlineData(7, 11)]
        [InlineData(8, 11)]
        [InlineData(9, 11)]
        [InlineData(10, 11)]
        [InlineData(11, 11)]
        public void Given_11_pages_should_skip_pages_2_and_3_when_on_pages_6_to_11(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 4, 5, 6, 7, 8, 9, 10, 11));

        [Theory]
        [InlineData(1, 12)]
        [InlineData(2, 12)]
        [InlineData(3, 12)]
        [InlineData(4, 12)]
        [InlineData(5, 12)]
        public void Given_12_pages_should_skip_pages_9_to_11_when_on_pages_1_to_5(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 2, 3, 4, 5, 6, 7, 8, 12));

        [Theory]
        [InlineData(6, 12)]
        public void Given_12_pages_should_skip_pages_2_3_10_11_when_on_page_6(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 4, 5, 6, 7, 8, 9, 12));

        [Theory]
        [InlineData(7, 12)]
        [InlineData(8, 12)]
        [InlineData(9, 12)]
        [InlineData(10, 12)]
        [InlineData(11, 12)]
        [InlineData(12, 12)]
        public void Given_12_pages_should_skip_pages_2_to_4_when_on_pages_7_to_12(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 5, 6, 7, 8, 9, 10, 11, 12));

        [Theory]
        [InlineData(1, 13)]
        [InlineData(2, 13)]
        [InlineData(3, 13)]
        [InlineData(4, 13)]
        [InlineData(5, 13)]
        public void Given_13_pages_should_skip_pages_9_to_12_when_on_pages_1_to_5(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 2, 3, 4, 5, 6, 7, 8, 13));

        [Theory]
        [InlineData(6, 13)]
        public void Given_13_pages_should_skip_pages_2_3_10_11_12_when_on_page_6(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 4, 5, 6, 7, 8, 9, 13));

        [Theory]
        [InlineData(7, 13)]
        public void Given_13_pages_should_skip_pages_2_3_4_11_12_when_on_page_6(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 5, 6, 7, 8, 9, 10, 13));

        [Theory]
        [InlineData(8, 13)]
        [InlineData(9, 13)]
        [InlineData(10, 13)]
        [InlineData(11, 13)]
        [InlineData(12, 13)]
        [InlineData(13, 13)]
        public void Given_13_pages_should_skip_pages_2_to_5_when_on_pages_8_to_13(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName).Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 6, 7, 8, 9, 10, 11, 12, 13));

        private IEnumerable<Page> Pages(int activePage, params int[] pageNumbers)
            => pageNumbers.Select(p => new Page(p, activePage == p));
    }
}