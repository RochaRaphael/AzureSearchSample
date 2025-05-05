using Newtonsoft.Json;

namespace SearchIA.Application.Models
{
    public class Product
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public List<string> ProductSKUs { get; set; }
        public ReadOnlyMemory<float> ProductNameVector { get; set; }
        public ReadOnlyMemory<float> ProductDescriptionVector { get; set; }
        
    }
}
