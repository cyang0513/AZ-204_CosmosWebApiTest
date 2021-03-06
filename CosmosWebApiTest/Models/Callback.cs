using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CosmosWebApiTest
{
   public class Callback
   {
      public Callback()
      {
         this.Id = Guid.NewGuid();
      }

      public Guid Id { get; }

      public Uri Uri { set; get; }

      public HttpResponseMessage InvokeAsync<TOutput>(TOutput triggerOutput)
      {
         HttpClient httpClient = new HttpClient();

         httpClient.DefaultRequestHeaders.Accept.Clear();
         httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));

         return Task.Run(async () => await httpClient.PostAsJsonAsync(Uri, triggerOutput)).Result;
      }
   }
}
