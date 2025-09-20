using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class UserMapper
{
    public partial UserModel MapUserToUserModel(User user);

    public partial List<UserModel> MapUsersToUserModels(List<User> users);
}
