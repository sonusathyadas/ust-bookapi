namespace BookWebApi.Models
{
	// Customer model with id, name, email, mobile, address, username and password hash
	public class Customer
	{
		public int Id { get; set; }
	public string? Name { get; set; }
	public string? Email { get; set; }
	public string? Mobile { get; set; }
	public string? Address { get; set; }

	// Authentication fields
	public string? UserName { get; set; }
	public string? PasswordHash { get; set; }
	}
}
