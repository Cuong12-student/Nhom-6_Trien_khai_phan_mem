namespace bhgbd.Models
{
    public class StaffDashboardViewModel
    {
        public int PendingCount { get; set; }
        public int CancelRequestedCount { get; set; }
        public int ConfirmedCount { get; set; }
        public List<OrderStaffViewModel> RecentOrders { get; set; } = new List<OrderStaffViewModel>();
    }
    public class OrderStaffViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode => $"#ORD-{OrderId}";
        public DateTime OrderDate { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
