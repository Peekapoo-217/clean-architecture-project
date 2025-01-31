﻿using Entities;
using Infrastructure.Models;

namespace Infrastructure.Services
{
    public class BookProcessingService
    {
        public IEnumerable<Book> ApplySorting(IEnumerable<Book> books, BookSortCriteria bookSortCriteria)
        {
            var booksAfterSorting = books;
            if (bookSortCriteria.PriceSortOrder.HasValue)
            {
                booksAfterSorting = bookSortCriteria.PriceSortOrder switch
                {
                    PriceSortOrders.LowestToHighest => books.OrderByDescending(b => (b.Price * (1 - b.DiscountPercentage / 100m))),
                    PriceSortOrders.HighestToLowest => books.OrderBy(b => (b.Price * (1 - b.DiscountPercentage / 100m))),
                    _ => books
                };
            }

            if (bookSortCriteria.DateSortOrder.HasValue)
            {
                booksAfterSorting = bookSortCriteria.DateSortOrder switch
                {
                    DateSortOrders.NewestToOldest => books.OrderByDescending(b => b.CreatedAt),
                    DateSortOrders.OldestToNewest => books.OrderBy(b => b.CreatedAt),
                    _ => books
                };
            }
            return booksAfterSorting;
        }

        public IEnumerable<Book> ApplyFiltering(IEnumerable<Book> books, BookFilterCriteria bookFilterCriteria)
        {
            var booksAfterFiltering = books.Where(_ =>
                (bookFilterCriteria.CategoryId == null || _.CategoryId == bookFilterCriteria.CategoryId) &&
                (bookFilterCriteria.PublisherId == null || _.PublisherId == bookFilterCriteria.PublisherId)
            );
            return booksAfterFiltering;
        }
    }
}
