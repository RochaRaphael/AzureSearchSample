using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using SearchIA.Application.Models;
using OpenAI;
using OpenAI.Embeddings;
using System.Net;

namespace SearchIA.Application.Services
{
    public class SearchAIService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly SearchClient _searchClient;

        public SearchAIService(OpenAIClient clientFactory, SearchClient searchClient)
        {
            _openAIClient = clientFactory;
            _searchClient = searchClient;
        }

        public async Task<BaseReturn> AddProductToAzureSearchIndex(InProduct product)
        {
            try
            {
                // Configura o cliente de embeddings e define o vetor com 1024 dimensões para busca vetorial
                EmbeddingClient embeddingClient = _openAIClient.GetEmbeddingClient("text-embedding-3-large");
                var embeddingOptions = new EmbeddingGenerationOptions { Dimensions = 1024 };

                // Gera os embeddings para o nome e descrição do produto
                OpenAIEmbedding nameEmbedding = await embeddingClient.GenerateEmbeddingAsync(product.Name, embeddingOptions);
                OpenAIEmbedding descricaoEmbedding = await embeddingClient.GenerateEmbeddingAsync(product.Description, embeddingOptions);

                var newProduct = new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Image = product.Image,
                    Price = product.Price,
                    ProductSKUs = product.ProductSKUs,
                    ProductNameVector = nameEmbedding.ToFloats(),
                    ProductDescriptionVector = descricaoEmbedding.ToFloats(),
                };

                // Adiciona o produto ao índice de busca
                var response = await _searchClient.UploadDocumentsAsync(new[] { newProduct });

                bool success = response.GetRawResponse().Status == (int)HttpStatusCode.OK || response.GetRawResponse().Status == (int)HttpStatusCode.Created;

                return new BaseReturn
                {
                    Success = success,
                    Message = success ? "Product inserted successfully." : "Error inserting product."
                };
            }
            catch (Exception ex)
            {
                return new BaseReturn
                {
                    Success = false,
                    Message = $"Error inserting product: {ex.Message}"
                };
            }
        }
        public async Task<IReadOnlyList<SearchResult<OutProduct>>> SearchByVectorSimilarity(string queryText)
        {
            var resultList = new List<SearchResult<OutProduct>>();
            try
            {
                //Aqui configuramos da mesma forma que inserimos para que o texto pesquisado esteja de acordo com os vetores que foram salvos no index
                EmbeddingClient embeddingClient = _openAIClient.GetEmbeddingClient("text-embedding-3-large");
                var embeddingOptions = new EmbeddingGenerationOptions { Dimensions = 1024 };

                // Converte o embedding para um vetor de floats, que será usado na busca vetorial
                OpenAIEmbedding embedding = await embeddingClient.GenerateEmbeddingAsync(queryText, embeddingOptions);
                ReadOnlyMemory<float> queryVector = embedding.ToFloats().ToArray();

                // Executa a busca vetorial no Azure Cognitive Search usando o vetor da consulta
                SearchResults<OutProduct> results = await _searchClient.SearchAsync<OutProduct>(
                new SearchOptions
                {
                    VectorSearch = new()
                    {
                        // Retorna os 2 itens mais semelhantes e os campos vetoriais a serem comparados
                        Queries = { new VectorizedQuery(queryVector) { KNearestNeighborsCount = 2, Fields = { "productDescriptionVector ", "productNameVector" } } }
                    }
                });

                // Itera sobre os resultados da busca e adiciona à lista de retorno
                await foreach (var result in results.GetResultsAsync())
                {
                    resultList.Add(result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return resultList;
        }
    }
}