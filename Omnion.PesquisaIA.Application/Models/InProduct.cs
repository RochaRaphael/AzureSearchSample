
namespace SearchIA.Application.Models
{
    public class InProduct
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public List<string> ProductSKUs { get; set; }
    }
}
