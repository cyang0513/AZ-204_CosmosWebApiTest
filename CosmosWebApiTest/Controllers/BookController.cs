using CosmosWebApiTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosWebApiTest.Controllers
{
   public class BookController : Controller
   {
      ILogger m_Logger;
      static Container m_Container;

      public BookController(ILogger<Book> logger, IOptionsSnapshot<CosmosConn> option)
      {
         m_Logger = logger;

         if (m_Container == null)
         {
            var client = new CosmosClient(option.Value.URI, option.Value.KEY);
            var db = client.GetDatabase("ChyaTestDB");

            var containerTask = db.CreateContainerIfNotExistsAsync(new ContainerProperties("Book", "/Category"));
            containerTask.Wait();
            m_Container = containerTask.Result.Container;
         }
      }

      [HttpGet, Route("Book/All")]
      public IList<Book> GetAllBook()
      {
         var res = m_Container.GetItemLinqQueryable<Book>(true).Select(x=>x).ToList();
         return res;
      }

      [HttpGet, Route("Book/{name}")]
      public IList<Book> Details(string name)
      {
         return m_Container.GetItemLinqQueryable<Book>(true).Where(x => x.BookName == name).ToList();
      }

      [HttpPost, Route("Book/Add")]
      public async Task<string> Create(Book book)
      {
         if (m_Container.GetItemLinqQueryable<Book>(true).Count(x => x.BookName == book.BookName) == 0)
         {
            var bookCreate = await m_Container.CreateItemAsync<Book>(book, new PartitionKey(book.Category));

            return bookCreate.Resource.id;
         }

         return "Book with same name has been added.";
      }

      [HttpDelete, Route("Book/Delete")]
      public async Task<string> Delete(string id, string catrgory)
      {
         try
         {
            await m_Container.DeleteItemAsync<Book>(id, new PartitionKey(catrgory));
            return $"Document {id} set to delete";
         } 
         catch (Exception ex)
         {
            return ex.Message;
         }
                
      }
   }
}
