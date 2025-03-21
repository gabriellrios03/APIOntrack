namespace LYLApiV1.Models
{
    public class DeliveriesHdr
    {
        public int Id { get; set; }
        public int Customer_id { get; set; }    
        public int Truck_id { get; set; }   
        public int Route_id { get; set; }
        public DateTime Appoinment_date { get; set; }
        public int Status_id { get; set; }
    }
}
