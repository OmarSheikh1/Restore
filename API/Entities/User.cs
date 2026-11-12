using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class User : IdentityUser<int>
{
    public UserAddress? Address { get; set; }
}

public class UserAddress : Address
{
    public int Id { get; set; }
}

public class Address
{
    public string FullName { get; set; } = string.Empty;
    
    public string Address1 { get; set; }= string.Empty;

    public string Address2 { get; set; }= string.Empty;

    public string City { get; set; }= string.Empty;

    public string State { get; set; }= string.Empty;

    public string Zip { get; set; }= string.Empty;

    public string Country { get; set; }= string.Empty;

}