namespace bhgbd.Models
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Sole { get; set; } = string.Empty;
        public string SoleName { get; set; } = string.Empty; // "Cỏ nhân tạo", "Cỏ tự nhiên",...

        public decimal PriceValue { get; set; }
        public string Price => PriceValue.ToString("N0") + "đ";

        public decimal? OldPriceValue { get; set; }
        public string OldPrice => OldPriceValue.HasValue ? OldPriceValue.Value.ToString("N0") + "đ" : "";

        public string Discount { get; set; } = string.Empty; // "-13%"
        public string Badge { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string StoryTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Fit { get; set; } = string.Empty;

        public List<string> Sizes { get; set; } = new List<string>();
        public List<string> Images { get; set; } = new List<string>();
    }
}
