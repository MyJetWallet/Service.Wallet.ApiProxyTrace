using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Service.Wallet.ApiProxyTrace.Services
{
    public class ProxyMiddleware
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;

        public ProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext context)
        {
            var targetUri = BuildTargetUri(context.Request);
            
            Console.WriteLine($"{context.Request.Method}:  {context.Request.Path} -->> {targetUri.ToString()}: ...");

            if (targetUri != null)
            {
                var targetRequestMessage = CreateTargetMessage(context, targetUri);

                using var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(context, responseMessage);
                await responseMessage.Content.CopyToAsync(context.Response.Body);
                
                Console.WriteLine($"{context.Request.Method}:  {context.Request.Path} -->> {targetUri.ToString()}: {responseMessage.StatusCode}");

                // Console.WriteLine("-----------------------");
                //
                // foreach (var header in targetRequestMessage.Headers)
                // {
                //     Console.WriteLine($"  {header.Key}: {header.Value.Aggregate((s,s1) => $"{s},{s1}")}");
                // }
                
                return;
            }
            Console.WriteLine($"Cannot handle CALL {context.Request.Method}:  {context.Request.Path}");
            await context.Response.WriteAsync($"PROXY: Cannot handle CALL {context.Request.Method}:  {context.Request.Path}");
            context.Response.StatusCode = 500;
        }
        
        private Uri BuildTargetUri(HttpRequest request)
        {
            var targetUri = new Uri(Program.Settings.ProxyHost + request.Path);
            return targetUri;
        }
        
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }
        
        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
        
        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                if (!string.IsNullOrEmpty(context.Request.ContentType))
                {
                    streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
                }
                Console.WriteLine($"--->>> {streamContent.Headers.ContentType}");
                requestMessage.Content = streamContent;
            }
            
            

            foreach (var header in context.Request.Headers)
            {
                if (header.Key == "Content-Type")
                {
                    //Console.WriteLine($"!!CT: {header.Value.FirstOrDefault()}");
                }
                else if(header.Key == "Content-Length")
                {
                    //Console.WriteLine($"!!!CL: {header.Value.FirstOrDefault()}");
                }
                else
                {
                    requestMessage.Headers.Remove(header.Key);
                    var res = requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    //Console.WriteLine($" --> {header.Key}: {header.Value.FirstOrDefault().Substring(0,10)} :: {res}");
                }

            }
        }
        
        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }
    }
}