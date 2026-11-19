
namespace SalesManagementPlatform.Domain.Entities
{
    public sealed class Customer
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get;  }   
        public string Email { get;  }

        public Customer(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}