using Bogus;
using IdentityService.Domain.Entities;

namespace IdentityService.Tests.Shared.Fixtures.Utils;

public class UserDataFixtures : BaseFixtures<UserData>
{
    public static UserData GenerateUserData()
    {
        var faker = new Faker<UserData>("pt_BR")
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Name, f => f.Person.FullName);

        return faker.Generate();
    }

    public static UserData CreateAs_Base()
    {
        var userData = GenerateUserData();

        userData.PrepareToInsert(Guid.NewGuid());

        return userData;
    }
}
