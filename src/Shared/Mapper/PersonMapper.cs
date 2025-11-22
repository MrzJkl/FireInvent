using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class PersonMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(Person.AssignedItems))]
    [MapperIgnoreSource(nameof(Person.Departments))]
    public partial PersonModel MapPersonToPersonModel(Person person);

    [MapValue(nameof(Person.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(Person.AssignedItems))]
    [MapperIgnoreTarget(nameof(Person.Departments))]
    [MapperIgnoreSource(nameof(CreateOrUpdatePersonModel.DepartmentIds))]
    public partial Person MapCreateOrUpdatePersonModelToPerson(CreateOrUpdatePersonModel createPersonModel);

    public partial List<PersonModel> MapPersonsToPersonModels(List<Person> persons);

    [MapperIgnoreTarget(nameof(Person.Id))]
    [MapperIgnoreTarget(nameof(Person.AssignedItems))]
    [MapperIgnoreTarget(nameof(Person.Departments))]
    [MapperIgnoreSource(nameof(CreateOrUpdatePersonModel.DepartmentIds))]
    public partial void MapCreateOrUpdatePersonModelToPerson(CreateOrUpdatePersonModel source, Person target, Guid id);
}
