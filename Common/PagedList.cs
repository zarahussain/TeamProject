using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventureWorks
{
   public class Page
    {
        public int number { get; set; } = 1;
        public int size { get; set; } = 10;
    }
   public class PagedList<T>
   {
      public List<T> Items { get; }
      public int TotalItems { get; }
      public int TotalPages { get; }
      public int PageSize { get; }
      public int PageNumber { get; }
      public bool HasPreviousPage { get; }
      public bool HasNextPage { get; }
      public int PreviousPageNumber { get; }
      public int NextPageNumber { get; }

      public PagedList(IQueryable<T> source, int pageNumber, int pageSize)
      {
         Items = source.Skip(pageSize * (pageNumber - 1))
                     .Take(pageSize)
                     .ToList();
         TotalItems = source.Count();
         TotalPages = (int)Math.Ceiling(TotalItems / (double)pageSize);
         PageSize = pageSize;
         PageNumber = pageNumber;
         HasPreviousPage = PageNumber > 1;
         HasNextPage = PageNumber < TotalPages;
         PreviousPageNumber = HasPreviousPage ? PageNumber - 1 : 1;
         NextPageNumber = HasNextPage ? PageNumber + 1 : TotalPages;
      }
   }
}