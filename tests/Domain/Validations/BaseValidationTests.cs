using System.ComponentModel.DataAnnotations;

namespace IdentityService.Tests.Domain.Validations;

public class BaseValidationTests
{
    public static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }

    protected void AssertValidationError(string expectedErrorMessage, Action action)
    {
        var exception = Assert.Throws<ValidationException>(action);
        Assert.Contains(expectedErrorMessage, exception.Message);
    }

    protected void AssertNoValidationError(Action action)
    {
        var exception = Record.Exception(action);
        Assert.Null(exception);
    }
}
