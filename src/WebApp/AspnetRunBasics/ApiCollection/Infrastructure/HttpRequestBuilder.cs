using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AspnetRunBasics.ApiCollection.Infrastructure
{
    public class HttpRequestBuilder
    {
        private readonly HttpRequestMessage _request;
        private string _baseAddress;
        private readonly ApiBuilder _apiBuilder;

        public HttpRequestBuilder(string uri) : this(new ApiBuilder(uri)) { }
        public HttpRequestBuilder(ApiBuilder apiBuilder)
        {
            _request = new HttpRequestMessage();
            _apiBuilder = apiBuilder;
            _baseAddress = _apiBuilder.GetLeftPart();
        }

        public HttpRequestBuilder AddToPath(string path)
        {
            _apiBuilder.AddToPath(path);
            _request.RequestUri = _apiBuilder.GetUri();

            return this;
        }

        public HttpRequestBuilder SetPath(string path)
        {
            _apiBuilder.SetPath(path);
            _request.RequestUri = _apiBuilder.GetUri();

            return this;
        }

        public HttpRequestBuilder HttpMethod(HttpMethod httpMethod)
        {
            _request.Method = httpMethod;
            return this;
        }

        public HttpRequestBuilder Headers(Action<HttpRequestHeaders> funcOfHeaders)
        {
            funcOfHeaders(_request.Headers);
            return this;
        }

        public HttpRequestBuilder Headers(NameValueCollection headers)
        {
            _request.Headers.Clear();
            foreach (var item in headers.AllKeys)
            {
                _request.Headers.Add(item, headers[item]);
            }

            return this;
        }

        public HttpRequestBuilder Content(HttpContent content)
        {
            _request.Content = content;
            return this;
        }

        public HttpRequestBuilder RequestUri(Uri uri)
        {
            _request.RequestUri = new ApiBuilder(uri.ToString())
                .GetUri();
            return this;
        }

        public HttpRequestBuilder RequestUri(string uri)
        {
            return RequestUri(new Uri(uri));
        }

        public HttpRequestBuilder BaseAddress(string address)
        {
            _baseAddress = address;
            return this;
        }

        public HttpRequestBuilder Subdomain(string subdomain)
        {
            _apiBuilder.SetSubdomain(subdomain);
            _request.RequestUri = _apiBuilder.GetUri();

            return this;
        }

        public HttpRequestBuilder AddQueryString(string name, string value)
        {
            _apiBuilder.AddQueryString(name, value);
            _request.RequestUri = _apiBuilder.GetUri();

            return this;
        }

        public HttpRequestBuilder SetQueryString(string qs)
        {
            _apiBuilder.QueryString(qs);
            _request.RequestUri = _apiBuilder.GetUri();

            return this;
        }

        public HttpRequestMessage GetHttpMessage()
        {
            return _request;
        }

        public ApiBuilder GetApiBuilder()
        {
            return new ApiBuilder(_request.RequestUri.ToString());
        }
    }
}
