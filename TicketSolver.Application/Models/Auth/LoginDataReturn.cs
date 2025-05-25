using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Application.Models.Auth;

public class LoginDataReturn
{
    public UsersReturn User { get; set; }
    public string Token { get; set; }

    public override string ToString()
    {
        if (User == null) return $"{{ \"user\": null, \"token\": \"{Token}\" }}";
        return $$"""
                  {
                      "user": {
                          "id": "{{User.Id}}",
                          "name": "{{User.Name}}",
                          "email": "{{User.Email}}",
                          "tenantId": {{User.TenantId}},
                          "defUserType": {{(int)User.DefUserType}},
                          "role": "{{User.Role}}"
                      },
                      "token": "{{Token}}"
                  }
                 """;
    }

}

public class UsersReturn
    {
        public UsersReturn(Users user)
        {
            Email = user.Email ?? string.Empty;
            Id = user.Id;
            Name = user.FullName;
            if (user.Email != null) Email = user.Email;
            TenantId = user.TenantId;
            DefUserType = (eDefUserTypes)user.DefUserTypeId;
            Role = DefUserType.ToString();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int TenantId { get; set; }
        public eDefUserTypes DefUserType { get; set; }
        private readonly string _role;
        public string Role
        {
            get => _role;
            private init
            {
                _role = value switch
                {
                    "Admin" => "admin",
                    "Technician" => "technician",
                    "Client" => "user",
                    _ => "user"
                };
            }
        }
    }
