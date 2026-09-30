namespace bhgbd.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public decimal SubTotal => Items.Sum(i => i.TotalPriceValue);
        public string SubTotalFormatted => SubTotal.ToString("N0") + "đ";

        public decimal DiscountValue { get; set; } = 0;
        public string DiscountFormatted => DiscountValue.ToString("N0") + "đ";

        public string VoucherCode { get; set; } = string.Empty;

        public decimal GrandTotal => SubTotal - DiscountValue;
        public string GrandTotalFormatted => GrandTotal.ToString("N0") + "đ";
    }
    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Sole { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        public decimal UnitPriceValue { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPriceValue => UnitPriceValue * Quantity;
        public string TotalPriceFormatted => TotalPriceValue.ToString("N0") + "đ";
    }
}
