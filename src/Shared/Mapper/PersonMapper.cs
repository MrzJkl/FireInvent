using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class PersonMapper : BaseMapper
{
    public partial PersonModel MapPersonToPersonModel(Person person);

    [MapValue(nameof(Person.Id), Use = nameof(NewGuid))]
    public partial Person MapCreateOrUpdatePersonModelToPerson(CreateOrUpdatePersonModel createPersonModel);

    public partial List<PersonModel> MapPersonsToPersonModels(List<Person> persons);

    public partial void MapCreateOrUpdatePersonModelToPerson(CreateOrUpdatePersonModel source, Person target, Guid id);
}
