using CosmosWebApiTest.Models;
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
      static readonly Dictionary<string, Callback> m_Callbacks = new Dictionary<string, Callback>();

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

      [HttpGet, Route("Book/{name}", Name = "GetBook")]
      public IList<Book> Details(string name)
      {
         return m_Container.GetItemLinqQueryable<Book>(true).Where(x => x.BookName == name).ToList();
      }

      [HttpGet, Route("Book/Sys")]
      public string GetSys()
      {
         return Environment.MachineName + " " + Environment.OSVersion;
      }

      [HttpPost, Route("Book/Add")]
      public async Task<Book> Create(Book book)
      {
         if (m_Container.GetItemLinqQueryable<Book>(true).Count(x => x.BookName == book.BookName) == 0)
         {
            var bookCreate = await m_Container.CreateItemAsync<Book>(book, new PartitionKey(book.Category));

            m_Logger.LogInformation($"Call back count {m_Callbacks.Count}");
            foreach (var call in m_Callbacks.Values)
            {
               m_Logger.LogInformation($"Call back Id: {call.Id}, Uri {call.Uri}");
               if (call.Uri == null || call.Uri.ToString() == string.Empty)
               {
                  m_Logger.LogInformation($"Call back Uri invalid");
                  continue;
               }
               call.InvokeAsync<Book>(bookCreate.Resource);
            }

            return bookCreate.Resource;
         }

         return null;
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

      // Subscribe to newly created books
      [HttpPost, Route("Book/Subscribe")]
      public IActionResult Subscribe(Callback callback)
      {
         if (m_Callbacks.ContainsKey(callback.Id))
         {
            return BadRequest(); 
         }
         if (m_Callbacks.Values.Any(x=>x.Uri.ToString() == callback.Uri.ToString()))
         {
            return BadRequest(); 
         }
         m_Callbacks.Add(callback.Id, callback);

         m_Logger.LogInformation($"Call back added: {callback.Id} - {callback.Uri}");

         var value = CreatedAtRoute(nameof(Unsubscribe), new {callbackId = callback.Id}, string.Empty);
         return value;
      }

      [HttpDelete, Route("Book/Subscribe/{callbackId}", Name = nameof(Unsubscribe))]
      public IActionResult Unsubscribe(string callbackId)
      {
         if (m_Callbacks.ContainsKey(callbackId))
         {
            m_Callbacks.Remove(callbackId);
            return Ok();
         }
         return BadRequest();
      }
   }
}
