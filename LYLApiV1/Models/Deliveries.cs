namespace LYLApiV1.Models
{
    public class Deliveries
    {
        public int Id { get; set; }
        public int Header_id { get; set; }
        public int Customer_id { get; set; }
        public int Truck_id { get; set; }
        public int Route_id { get; set; }
        public int Driver_Iid { get; set; }
        public int Dryvan_id { get; set; }
        public int Free_dryvan_id { get; set;}
        public DateTime Appoinment_date { get; set; }
        public int Status_id { get; set; }
        public string Documents_status { get; set; }
        public string Remision_number { get; set; }
        public bool Is_invoiced { get; set; }
        public bool Is_paid { get; set; }
        public string Notes { get; set; }

    }
}
