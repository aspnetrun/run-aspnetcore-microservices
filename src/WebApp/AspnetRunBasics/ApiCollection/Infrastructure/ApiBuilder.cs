using System;
using System.Web;

namespace AspnetRunBasics.ApiCollection.Infrastructure
{
    public class ApiBuilder
    {
        private readonly string _fullUrl;
        private UriBuilder _builder;

        public ApiBuilder(string url)
        {
            _fullUrl = url;
            _builder = new UriBuilder(url);
        }

        public Uri GetUri() => _builder.Uri;

        public ApiBuilder Scheme(string scheme)
        {
            _builder.Scheme = scheme;
            return this;
        }

        public ApiBuilder Host(string host)
        {
            _builder.Host = host;
            return this;
        }

        public ApiBuilder Port(int port)
        {
            _builder.Port = port;
            return this;
        }

        public ApiBuilder AddToPath(string path)
        {
            IncludePath(path);
            return this;
        }

        public ApiBuilder SetPath(string path)
        {
            _builder.Path = path;
            return this;
        }

        public void IncludePath(string path)
        {
            if (string.IsNullOrEmpty(_builder.Path)
                || _builder.Path == "/")
            {
                _builder.Path = path;
            }
            else
            {
                var newPath = $"{_builder.Path}/{path}";
                _builder.Path = newPath.Replace("//", "/");
            }
        }

        public ApiBuilder Fragment(string fragment)
        {
            _builder.Fragment = fragment;
            return this;
        }

        public ApiBuilder SetSubdomain(string subdomain)
        {
            _builder.Host = string.Concat(subdomain, ".", new Uri(_fullUrl).Host);
            return this;
        }

        public bool HasSubdomain()
        {
            return _builder.Uri.HostNameType == UriHostNameType.Dns
                   && _builder.Uri.Host.Split('.').Length > 2;
        }

        public ApiBuilder AddQueryString(string name, string value)
        {
            var qsNv = HttpUtility.ParseQueryString(_builder.Query);
            qsNv[name] = string.IsNullOrEmpty(qsNv[name])
                ? value
                : string.Concat(qsNv[name], ",", value);

            _builder.Query = qsNv.ToString();

            return this;
        }

        public ApiBuilder QueryString(string queryString)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                _builder.Query = queryString;
            }
            return this;
        }

        public ApiBuilder UserName(string username)
        {
            _builder.UserName = username;
            return this;
        }

        public ApiBuilder Password(string password)
        {
            _builder.Password = password;
            return this;
        }

        public string GetLeftPart()
        {
            return _builder.Uri.GetLeftPart(UriPartial.Path);
        }
    }
}
