using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Http;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    public interface IGoodreadsSearchProxy
    {
        public List<SearchJsonResource> Search(string query);
    }

    public class GoodreadsSearchProxy : IGoodreadsSearchProxy
    {
        private readonly ICachedHttpResponseService _cachedHttpClient;
        private readonly Logger _logger;
        private readonly IHttpRequestBuilderFactory _searchBuilder;

        public GoodreadsSearchProxy(ICachedHttpResponseService cachedHttpClient,
            Logger logger)
        {
            _cachedHttpClient = cachedHttpClient;
            _logger = logger;

            _searchBuilder = new HttpRequestBuilder("http://localhost:3000/bookinfo/v1/search")
                .KeepAlive()
                .CreateFactory();
        }

        public List<SearchJsonResource> Search(string query)
        {
            try
            {
                var httpRequest = _searchBuilder.Create()
                    .AddQueryParam("q", query)
                    .Build();

                var response = _cachedHttpClient.Get<List<SearchJsonResource>>(httpRequest, true, TimeSpan.FromDays(5));

                return response.Resource;
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new GoodreadsException("Search for '{0}' failed. Unable to communicate with Goodreads.", ex, query);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new GoodreadsException("Search for '{0}' failed. Unable to communicate with Goodreads.", ex, query, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new GoodreadsException("Search for '{0}' failed. Invalid response received from Goodreads.", ex, query);
            }
        }
    }
}
