namespace LYLApiV1.Models
{
    public class Routes
    {
        public int Id { get; set; }
        public int Customer_Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public decimal Distance_Km { get; set; }
        public decimal Avg_fuel_consumption { get; set; }
        public decimal Customer_price { get; set; }
        public decimal Driver_profit { get; set; }
    }
}
