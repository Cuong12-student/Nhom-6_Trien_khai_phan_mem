namespace bhgbd.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Sole { get; set; } = string.Empty; // TF, FG, IC, AG
        public decimal PriceValue { get; set; }

        // Giá hiển thị định dạng sẵn: 2.690.000đ
        public string Price => PriceValue.ToString("N0") + "đ";

        public decimal? OldPriceValue { get; set; }
        public string Old => OldPriceValue.HasValue ? OldPriceValue.Value.ToString("N0") + "đ" : "";

        public string Image { get; set; } = string.Empty;
        public string Sizes { get; set; } = string.Empty; // Ví dụ: "39 40 41 42 43"
        public string Badge { get; set; } = string.Empty; // "BÁN CHẠY", "MỚI", "SALE", "HOT"
    }
}
