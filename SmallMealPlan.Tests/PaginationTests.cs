using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmallMealPlan.Tests
{
    [TestClass]
    public class PaginationTests
    {
        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(1, 2)]
        [DataRow(2, 2)]
        [DataRow(1, 3)]
        [DataRow(2, 3)]
        [DataRow(3, 3)]
        [DataRow(1, 10)]
        [DataRow(5, 10)]
        [DataRow(10, 10)]
        public void Up_to_10_pages_should_show_all_pages(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, Enumerable.Range(1, pageCount).ToArray()));

        [TestMethod]
        [DataRow(1, 11)]
        [DataRow(2, 11)]
        [DataRow(3, 11)]
        [DataRow(4, 11)]
        [DataRow(5, 11)]
        public void Given_11_pages_should_skip_pages_9_and_10_when_on_pages_1_to_5(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 2, 3, 4, 5, 6, 7, 8, 11));

        [TestMethod]
        [DataRow(6, 11)]
        [DataRow(7, 11)]
        [DataRow(8, 11)]
        [DataRow(9, 11)]
        [DataRow(10, 11)]
        [DataRow(11, 11)]
        public void Given_11_pages_should_skip_pages_2_and_3_when_on_pages_6_to_11(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 4, 5, 6, 7, 8, 9, 10, 11));

        [TestMethod]
        [DataRow(1, 12)]
        [DataRow(2, 12)]
        [DataRow(3, 12)]
        [DataRow(4, 12)]
        [DataRow(5, 12)]
        public void Given_12_pages_should_skip_pages_9_to_11_when_on_pages_1_to_5(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 2, 3, 4, 5, 6, 7, 8, 12));

        [TestMethod]
        [DataRow(6, 12)]
        public void Given_12_pages_should_skip_pages_2_3_10_11_when_on_page_6(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 4, 5, 6, 7, 8, 9, 12));

        [TestMethod]
        [DataRow(7, 12)]
        [DataRow(8, 12)]
        [DataRow(9, 12)]
        [DataRow(10, 12)]
        [DataRow(11, 12)]
        [DataRow(12, 12)]
        public void Given_12_pages_should_skip_pages_2_to_4_when_on_pages_7_to_12(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 5, 6, 7, 8, 9, 10, 11, 12));

        [TestMethod]
        [DataRow(1, 13)]
        [DataRow(2, 13)]
        [DataRow(3, 13)]
        [DataRow(4, 13)]
        [DataRow(5, 13)]
        public void Given_13_pages_should_skip_pages_9_to_12_when_on_pages_1_to_5(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 2, 3, 4, 5, 6, 7, 8, 13));

        [TestMethod]
        [DataRow(6, 13)]
        public void Given_13_pages_should_skip_pages_2_3_10_11_12_when_on_page_6(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 4, 5, 6, 7, 8, 9, 13));

        [TestMethod]
        [DataRow(7, 13)]
        public void Given_13_pages_should_skip_pages_2_3_4_11_12_when_on_page_6(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 5, 6, 7, 8, 9, 10, 13));

        [TestMethod]
        [DataRow(8, 13)]
        [DataRow(9, 13)]
        [DataRow(10, 13)]
        [DataRow(11, 13)]
        [DataRow(12, 13)]
        [DataRow(13, 13)]
        public void Given_13_pages_should_skip_pages_2_to_5_when_on_pages_8_to_13(int activePage, int pageCount) =>
            new Pagination(activePage, pageCount, Pagination.SortByName, "").Pages
                .Should().BeEquivalentTo(Pages(activePage, 1, 6, 7, 8, 9, 10, 11, 12, 13));

        private IEnumerable<Page> Pages(int activePage, params int[] pageNumbers)
            => pageNumbers.Select(p => new Page(p, activePage == p));
    }
}