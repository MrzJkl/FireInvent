using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class PersonMapper : BaseMapper
{
    public partial PersonModel MapPersonToPersonModel(Person person);

    [MapValue(nameof(Person.Id), Use = nameof(NewGuid))]
    public partial Person MapCreatePersonModelToPerson(CreatePersonModel createPersonModel);

    public partial List<PersonModel> MapPersonsToPersonModels(List<Person> persons);

    [MapperIgnoreTarget(nameof(Person.Id))]
    public partial void MapPersonModelToPerson(PersonModel source, Person target);
}
