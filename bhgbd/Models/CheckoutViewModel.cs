namespace bhgbd.Models
{
    public class CheckoutViewModel
    {
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "COD"; // "COD" hoặc "BANK"

        // Danh sách sản phẩm mua
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal SubTotal => CartItems.Sum(i => i.TotalPriceValue);
        public string SubTotalFormatted => SubTotal.ToString("N0") + "đ";

        public decimal DiscountValue { get; set; } = 0;
        public string DiscountFormatted => DiscountValue.ToString("N0") + "đ";

        public decimal GrandTotal => SubTotal - DiscountValue;
        public string GrandTotalFormatted => GrandTotal.ToString("N0") + "đ";
    }
}
