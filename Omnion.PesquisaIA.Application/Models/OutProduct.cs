using System.Text.Json.Serialization;

namespace SearchIA.Application.Models
{
    public class OutProduct
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("productSKUs")]
        public List<int> ProductSKUs { get; set; }
    }
}
