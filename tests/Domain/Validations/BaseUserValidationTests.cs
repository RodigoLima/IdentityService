using IdentityService.Tests.Shared.Fixtures.Dtos;
using IdentityService.Application.Dto;

namespace IdentityService.Tests.Domain.Validations;

public class BaseUserValidationTests : BaseValidationTests
{
    [Fact]
    public void BasicUser_AllAttributesValid_ShouldHaveRequiredAttribute_ResultValid()
    {
        // Arrange
        var guestUser = GuestUserFixtures.CreateAs_Base();

        // Act
        var validationResults = ValidateModel(guestUser);

        // Assert
        Assert.Empty(validationResults);
    }

    /// <summary>
    /// Data for testing invalid BasicUser
    /// </summary>
    public static IEnumerable<object[]> GetBasicUserInvalidData()
    {
        yield return new object[] { GuestUserFixtures.CreateAs_InvalidName() };
        yield return new object[] { GuestUserFixtures.CreateAs_InvalidEmail() };
        yield return new object[] { GuestUserFixtures.CreateAs_InvalidPassword() };
    }

    [Theory]
    [MemberData(nameof(GetBasicUserInvalidData))]
    public void BasicUser_ShouldHaveRequiredAttribute_AllResultsInvalid(GuestUser guestUser)
    {
        // Act
        var validationResults = ValidateModel(guestUser);

        // Assert
        Assert.NotEmpty(validationResults);
    }
}
