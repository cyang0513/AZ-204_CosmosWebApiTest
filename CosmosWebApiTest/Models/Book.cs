using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CosmosWebApiTest.Models
{
   public class Book
   {
      string bookId;
      public string id
      {
         get
         {
            if (bookId == null || bookId == string.Empty)
            { return Guid.NewGuid().ToString(); }
            return bookId;
         }

         set
         {
            if (value == null || value == string.Empty)
            {
               bookId = Guid.NewGuid().ToString();
            }
            else
            {
               bookId = value;
            }
         }
      }

      public string BookName { get; set; }

      public string AuthorName { get; set; }

      public string Category { get; set; }

      public override string ToString()
      {
         return JsonSerializer.Serialize<Book>(this);
      }

   }
}
