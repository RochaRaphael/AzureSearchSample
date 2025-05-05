using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SearchIA.Application;
using SearchIA.Application.Models;
using SearchIA.Application.Services;

namespace SearchIA.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchAIController : ControllerBase
    {
        private SearchAIService _service;

        public SearchAIController(IOptions<ApiSettings> apiSettings, SearchAIService service)
        {
            _service = service;
        }

        [HttpPost("AddProductToAzureSearchIndex")]
        public async Task<BaseReturn> AddProductToAzureSearchIndex(InProduct dados)
        {
            return await _service.AddProductToAzureSearchIndex(dados);
        }

        [HttpPost("SearchByVectorSimilarity")]
        public async Task<IReadOnlyList<SearchResult<OutProduct>>> SearchByVectorSimilarity(string textoConsulta)
        {
            return await _service.SearchByVectorSimilarity(textoConsulta);
        }
    }
}
