using Bogus;
using IdentityService.Tests.Shared.Fixtures.Utils;
using IdentityService.Application.Dto;

namespace IdentityService.Tests.Shared.Fixtures.Dtos;

public class GuestUserFixtures : BaseFixtures<GuestUser>
{
    public GuestUserFixtures() : base() { }

    public static GuestUser GenerateUser()
    {
        var faker = Faker
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Password, f => f.Internet.Password(9) + "#" + 1);

        return faker.Generate();
    }

    public static GuestUser CreateAs_Base()
    {
        var user = GenerateUser();

        return user;
    }

    public static GuestUser CreateAs_InvalidName()
    {
        var user = CreateAs_Base();
        user.Name = string.Empty;

        return user;
    }

    public static GuestUser CreateAs_InvalidEmail()
    {
        var user = CreateAs_Base();
        user.Email = FakerDefault.Random.String2(2, 2);

        return user;
    }

    public static GuestUser CreateAs_InvalidPassword()
    {
        var user = CreateAs_Base();
        user.Password = FakerDefault.Random.String2(2, 2);

        return user;
    }
}
