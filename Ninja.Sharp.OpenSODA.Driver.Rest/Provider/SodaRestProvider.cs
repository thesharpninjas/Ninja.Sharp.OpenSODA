// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Microsoft.Extensions.Logging;
using Ninja.Sharp.OpenSODA.Driver.Rest.Services;
using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Models.Configuration;
using Ninja.Sharp.OpenSODA.Queries;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using InvalidDataException = Ninja.Sharp.OpenSODA.Exceptions.InvalidDataException;

namespace Ninja.Sharp.OpenSODA.Driver.Rest.Provider
{
    internal class SodaRestProvider(SodaRestConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IPaginationService paginationService,
        ILogger<SodaRestProvider> logger) : IDocumentDbProvider
    {
        private readonly SodaRestConfiguration sodaConfiguration = configuration;
        private readonly ILogger<SodaRestProvider> logger = logger;

        private const string COLLECTIONS_API_FORMAT = "/ords/{0}/soda/latest";
        private const string COLLECTION_API_FORMAT = "/ords/{0}/soda/latest/{1}";
        private const string OBJECT_API_FORMAT = "/ords/{0}/soda/latest/{1}/{2}";
        private const string RETRIEVE_API_FORMAT = "/ords/{0}/soda/latest/{1}/?limit=1&fromID={2}";
        private const string FILTER_API_FORMAT = "/ords/{0}/soda/latest/custom-actions/query/{1}/";
        private const string CLIENT_NAME = "soda";

        public async Task<ICollection<Item<T>>> ListAsync<T>(Page? pagination = null, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            StringBuilder urlBuilder = new(string.Format(COLLECTION_API_FORMAT, sodaConfiguration.Schema, myCollection));

            paginationService.ValidatePagination(pagination);
            paginationService.AddPagination(pagination, urlBuilder);

            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            string url = urlBuilder.ToString();
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
            logger.LogInformation("[SODA][GET][REQUEST]: {Url}", url);
            string responseString = await responseMessage.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][PUT][RESPONSE]: {StatusCode}", responseMessage.StatusCode);
            logger.LogInformation("[SODA][GET][RESPONSE][BODY]: {ResponseString}", responseString);
            if (responseMessage.IsSuccessStatusCode)
            {
                Result<T> sodaResult = responseString.Deserialize<Result<T>>();
                return sodaResult.Items;
            }

            throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {responseMessage.StatusCode}, message: \"{responseString}\".");

        }

        public async Task CreateCollectionIfNotExistsAsync(string collection)
        {
            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);

            string url = string.Format(COLLECTIONS_API_FORMAT, sodaConfiguration.Schema);
            using HttpResponseMessage getResponse = await httpClient.GetAsync(url);
            logger.LogInformation("[SODA][GET][REQUEST]: {Url}", url);
            string getResponseString = await getResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][GET][RESPONSE]: {StatusCode}", getResponse.StatusCode);
            logger.LogInformation("[SODA][GET][RESPONSE][BODY]: {GetResponseString}", getResponseString);

            if (getResponse.IsSuccessStatusCode)
            {
                string jsonResponseMessage = await getResponse.Content.ReadAsStringAsync();
                CollectionsResult sodaCollectionsResult = jsonResponseMessage.Deserialize<CollectionsResult>();
                if (!sodaCollectionsResult.Items.Any(c => c.Name == collection))
                {
                    url = string.Format(COLLECTION_API_FORMAT, sodaConfiguration.Schema, collection);

                    var stringContent = new StringContent(string.Empty, mediaType: "application/json", encoding: Encoding.UTF8);
                    using HttpResponseMessage putResponse = await httpClient.PutAsync(url, stringContent);

                    logger.LogInformation("[SODA][PUT][REQUEST]: {Url}", url);
                    logger.LogInformation("[SODA][PUT][REQUEST][BODY]: {StringContent}", stringContent);
                    string putResponseString = await putResponse.Content.ReadAsStringAsync();
                    logger.LogInformation("[SODA][PUT][RESPONSE]: {StatusCode}", putResponse.StatusCode);
                    logger.LogInformation("[SODA][PUT][RESPONSE][BODY]: {PutResponseString}", putResponseString);

                    if (!putResponse.IsSuccessStatusCode)
                    {
                        throw new InternalErrorException($"[SODA] HttpPut: Error calling \"{url}\"; status: {putResponse.StatusCode}, message: \"{putResponseString}\".");
                    }
                }
            }
            else
            {
                throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {getResponse.StatusCode}, message: \"{getResponseString}\".");
            }
        }

        public Task CreateCollectionIfNotExistsAsync<T>() where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(string.Empty);
            return CreateCollectionIfNotExistsAsync(myCollection);
        }

        public async Task<Item<T>> CreateAsync<T>(T item, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            StringBuilder urlBuilder = new(string.Format(COLLECTION_API_FORMAT, sodaConfiguration.Schema, myCollection));
            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            string url = urlBuilder.ToString();
            string serialized = item.Serialize();

            var stringContent = new StringContent(serialized, mediaType: "application/json", encoding: Encoding.UTF8);
            using HttpResponseMessage responseMessage = await httpClient.PostAsync(url, stringContent);

            logger.LogInformation("[SODA][POST][REQUEST]: {Url}", url);
            logger.LogInformation("[SODA][POST][REQUEST][BODY]: {StringContent}", stringContent);
            string responseString = await responseMessage.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][POST][RESPONSE]: {StatusCode}", responseMessage.StatusCode);
            logger.LogInformation("[SODA][POST][RESPONSE][BODY]: {ResponseString}", responseString);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new InternalErrorException($"[SODA] HttpPost: Error calling \"{url}\"; status: {responseMessage.StatusCode}, message: \"{responseString}\".");
            }

            Result<T> sodaResult = responseString.Deserialize<Result<T>>();
            Item<T> myResponse = sodaResult.Items.First();
            myResponse.Value = item;
            return myResponse;
        }

        private async Task<Item<T>> UpdateInternalAsync<T>(T item, HttpClient httpClient, string alphaId, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            StringBuilder urlBuilder = new(string.Format(OBJECT_API_FORMAT, sodaConfiguration.Schema, myCollection, alphaId));
            string url = urlBuilder.ToString();
            string serialized = item.Serialize();

            var stringContent = new StringContent(serialized, mediaType: "application/json", encoding: Encoding.UTF8);
            using HttpResponseMessage putResponse = await httpClient.PutAsync(url, stringContent);

            logger.LogInformation("[SODA][PUT][REQUEST]: {Url}", url);
            logger.LogInformation("[SODA][PUT][REQUEST][BODY]: {StringContent}", stringContent);
            string putResponseString = await putResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][PUT][RESPONSE]: {StatusCode}", putResponse.StatusCode);
            logger.LogInformation("[SODA][PUT][RESPONSE][BODY]: {PutResponseString}", putResponseString);

            if (!putResponse.IsSuccessStatusCode)
            {
                throw new InternalErrorException($"[SODA] HttpPut: Error calling \"{url}\"; status: {putResponse.StatusCode}, message: \"{putResponseString}\".");
            }

            // vs: magari si può capire se sia meglio rispondere solo col documento interno, oppure non rispondere affatto nel caso della update (come fanno del resto le REST API stesse)

            urlBuilder = new(string.Format(RETRIEVE_API_FORMAT, sodaConfiguration.Schema, myCollection, Guid.Parse(alphaId).Decrease().ToString("n").ToUpper()));
            url = urlBuilder.ToString();
            using HttpResponseMessage getResponseMessage = await httpClient.GetAsync(url);

            logger.LogInformation("[SODA][GET][REQUEST]: {Url}", url);
            string getResponseString = await getResponseMessage.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][GET][RESPONSE]: {StatusCode}", getResponseMessage.StatusCode);
            logger.LogInformation("[SODA][GET][RESPONSE][BODY]: {PutResponseString}", putResponseString);

            if (getResponseMessage.IsSuccessStatusCode)
            {
                Result<T> sodaResult = getResponseString.Deserialize<Result<T>>();
                Item<T> myResponse = sodaResult.Items.First(x => x.Id == alphaId);
                return myResponse;
            }

            throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {getResponseMessage.StatusCode}, message: \"{getResponseString}\".");
        }

        public async Task<Item<T>> UpdateAsync<T>(T item, string id, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            StringBuilder urlBuilder = new(string.Format(COLLECTION_API_FORMAT, sodaConfiguration.Schema, myCollection));
            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            urlBuilder.Append('/');
            string alphaId = Regex.Replace(id, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));
            if (string.IsNullOrWhiteSpace(alphaId))
            {
                throw new NotFoundException($"ID {alphaId} not found for collection {myCollection}.");
            }
            urlBuilder.Append(alphaId);
            string url = urlBuilder.ToString();
            using HttpResponseMessage getResponse = await httpClient.GetAsync(url);

            logger.LogInformation("[SODA][GET][REQUEST]: {Url}", url);
            string getResponseString = await getResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][GET][RESPONSE]: {StatusCode}", getResponse.StatusCode);
            logger.LogInformation("[SODA][GET][RESPONSE][BODY]: {GetResponseString}", getResponseString);

            if (getResponse.IsSuccessStatusCode)
            {
                return await UpdateInternalAsync(item, httpClient, alphaId);
            }

            if (getResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException($"[SODA] ID {alphaId} not found for collection {myCollection}.");
            }

            throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {getResponse.StatusCode}, message: \"{getResponseString}\".");
        }

        public async Task<Item<T>> UpsertAsync<T>(T item, string? id = null, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string alphaId = Regex.Replace(id ?? string.Empty, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));
            if (string.IsNullOrWhiteSpace(alphaId))
            {
                return await CreateAsync(item);
            }

            StringBuilder urlBuilder = new(string.Format(COLLECTION_API_FORMAT, sodaConfiguration.Schema, myCollection));
            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            urlBuilder.Append('/');
            urlBuilder.Append(alphaId);

            string url = urlBuilder.ToString();
            using HttpResponseMessage getResponse = await httpClient.GetAsync(url);

            logger.LogInformation("[SODA][GET][REQUEST]: {Url}", url);
            string getResponseString = await getResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][GET][RESPONSE]: {StatusCode}", getResponse.StatusCode);
            logger.LogInformation("[SODA][GET][RESPONSE][BODY]: {GetResponseString}", getResponseString);

            if (getResponse.IsSuccessStatusCode)
            {
                return await UpdateInternalAsync(item, httpClient, alphaId);
            }

            if (getResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return await CreateAsync(item);
            }

            throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {getResponse.StatusCode}, message: \"{getResponseString}\".");
        }

        public async Task<Item<T>> RetrieveAsync<T>(string id, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            StringBuilder urlBuilder = new(string.Format(RETRIEVE_API_FORMAT, sodaConfiguration.Schema, myCollection, Guid.Parse(id).Decrease().ToString("n").ToUpper()));

            string alphaId = Regex.Replace(id, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));
            string url = urlBuilder.ToString();

            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            using HttpResponseMessage getResponse = await httpClient.GetAsync(url);

            logger.LogInformation("[SODA][GET][REQUEST]: {Url}", url);
            string getResponseString = await getResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][GET][RESPONSE]: {StatusCode}", getResponse.StatusCode);
            logger.LogInformation("[SODA][GET][RESPONSE][BODY]: {GetResponseString}", getResponseString);

            if (getResponse.IsSuccessStatusCode)
            {
                Result<T> sodaResult = getResponseString.Deserialize<Result<T>>();
                Item<T>? myResponse = sodaResult.Items.FirstOrDefault(x => x.Id == alphaId);
                return myResponse ?? throw new NotFoundException($"ID {alphaId} not found for collection {myCollection}.");
            }

            throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {getResponse.StatusCode}, message: \"{getResponseString}\".");
        }

        public async Task DeleteAsync<T>(string id, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string alphaId = Regex.Replace(id, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));
            StringBuilder urlBuilder = new(string.Format(OBJECT_API_FORMAT, sodaConfiguration.Schema, myCollection, alphaId));

            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            string url = urlBuilder.ToString();

            using HttpResponseMessage deleteResponse = await httpClient.DeleteAsync(url);

            logger.LogInformation("[SODA][DELETE][REQUEST]: {Url}", url);
            string deleteResponseString = await deleteResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][DELETE][RESPONSE]: {StatusCode}", deleteResponse.StatusCode);
            logger.LogInformation("[SODA][DELETE][RESPONSE][BODY]: {DeleteResponseString}", deleteResponseString);

            if (deleteResponse.IsSuccessStatusCode)
            {
                return;
            }

            if (deleteResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException($"[SODA] HttpDelete: Error calling \"{url}\"; status: {deleteResponse.StatusCode}, message: \"{deleteResponseString}\".");
            }

            throw new InternalErrorException($"[SODA] HttpDelete: Error calling \"{url}\"; status: {deleteResponse.StatusCode}, message: \"{deleteResponseString}\".");
        }

        public async Task<ICollection<Item<T>>> FilterAsync<T>(string qbe, Page? pagination = null, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            StringBuilder urlBuilder = new(string.Format(FILTER_API_FORMAT, sodaConfiguration.Schema, myCollection));

            try
            {
                Qbe deserializedQbe = qbe.Deserialize<Qbe>();
                paginationService.ValidatePagination(pagination);
                if (deserializedQbe.OrderBy != null)
                {
                    paginationService.AddPagination(pagination, urlBuilder);
                }
                else
                {
                    deserializedQbe.Query ??= qbe.Deserialize<object>();
                    paginationService.AddPagination(pagination, urlBuilder, deserializedQbe, ref qbe);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("[SODA] Provided filter is invalid.", innerException: ex);
            }

            using HttpClient httpClient = httpClientFactory.CreateClient(CLIENT_NAME);
            string url = urlBuilder.ToString();

            var stringContent = new StringContent(qbe, mediaType: "application/json", encoding: Encoding.UTF8);
            using HttpResponseMessage postResponse = await httpClient.PostAsync(url, stringContent);

            logger.LogInformation("[SODA][POST][REQUEST]: {Url}", url);
            logger.LogInformation("[SODA][POST][REQUEST][BODY]: {StringContent}", stringContent);
            string postResponseString = await postResponse.Content.ReadAsStringAsync();
            logger.LogInformation("[SODA][POST][RESPONSE]: {StatusCode}", postResponse.StatusCode);
            logger.LogInformation("[SODA][POST][RESPONSE][BODY]: {PostResponseString}", postResponseString);

            if (postResponse.IsSuccessStatusCode)
            {
                Result<T> sodaResult = postResponseString.Deserialize<Result<T>>();
                ICollection<Item<T>> myResponse = sodaResult.Items;
                return myResponse;
            }

            throw new InternalErrorException($"[SODA] HttpGet: Error calling \"{url}\"; status: {postResponse.StatusCode}, message: \"{postResponseString}\".");
        }

        public Task<ICollection<Item<T>>> FilterAsync<T>(Query query, Page? pagination = null, string? collection = null) where T : class, new()
        {
            string qte = query.ToQbeString();
            return FilterAsync<T>(qte, pagination, collection);
        }
    }
}
