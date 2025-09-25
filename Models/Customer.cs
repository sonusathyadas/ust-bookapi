namespace BookWebApi.Models
{
	// Customer model with id, name, email, mobile and address
	public class Customer
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Mobile { get; set; }
		public string Address { get; set; }
	}
}
