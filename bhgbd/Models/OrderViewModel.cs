namespace bhgbd.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty; // Ví dụ: "BH26080601"
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();

        public int TotalQuantity => Items.Sum(i => i.Quantity);
        public string SummaryText => Items.Count > 1
            ? $"{Items.FirstOrDefault()?.ProductName} và {Items.Count - 1} sản phẩm khác"
            : $"{Items.FirstOrDefault()?.ProductName} · Size {Items.FirstOrDefault()?.Size}";

        public decimal GrandTotal { get; set; }
        public string GrandTotalFormatted => GrandTotal.ToString("N0") + "đ";
    }
    public class OrderItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
