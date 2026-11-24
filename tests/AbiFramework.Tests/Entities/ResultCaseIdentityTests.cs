using AbiFramework.Entities;
using AbiFramework.Extensions;
using AbiFramework.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AbiFramework.Tests.Entities;

public class ResultCaseIdentityTests
{
    private static readonly DomainError Conflict = DomainError.Conflict("TEST.CONFLICT", "Conflict");

    [Fact]
    public void ObjectResult_Failure_IsFailureNotSuccess()
    {
        var result = Result<object>.Failure(Conflict);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(Conflict);
        Func<object> act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ObjectResult_SuccessCarryingDomainError_IsSuccess()
    {
        var result = Result<object>.Success(Conflict);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(Conflict);
        result.Error.Should().Be(DomainError.None);
    }

    [Fact]
    public void DomainErrorResult_SuccessAndFailure_StayDistinct()
    {
        var success = Result<DomainError>.Success(Conflict);
        var failure = Result<DomainError>.Failure(Conflict);

        success.IsSuccess.Should().BeTrue();
        success.Value.Should().BeSameAs(Conflict);
        success.Error.Should().Be(DomainError.None);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().BeSameAs(Conflict);
        success.Equals(failure).Should().BeFalse();
        (success == failure).Should().BeFalse();
        success.GetHashCode().Should().NotBe(failure.GetHashCode());
    }

    [Fact]
    public void ValidationErrorResult_SuccessAndFailure_StayDistinct()
    {
        var validation = new ValidationError([Conflict]);
        var success = Result<ValidationError>.Success(validation);
        var failure = Result<ValidationError>.Failure(validation);

        success.IsSuccess.Should().BeTrue();
        success.Value.Should().BeSameAs(validation);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().BeSameAs(validation);
        success.Should().NotBe(failure);
    }

    [Fact]
    public void ObjectResult_SuccessAndFailure_AreNeverEqual()
    {
        var success = Result<object>.Success(Conflict);
        var failure = Result<object>.Failure(Conflict);

        success.Equals((object)failure).Should().BeFalse();
        (success != failure).Should().BeTrue();
    }

    [Fact]
    public void ObjectResult_ImplicitConversions_UseExactlyTheDeclaredCase()
    {
        Result<object> fromError = Conflict;
        Result<object> fromValue = "text";

        fromError.IsFailure.Should().BeTrue();
        fromValue.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ObjectResult_SwitchWithDomainErrorArmFirst_RespectsCase()
    {
        Describe(Result<object>.Failure(Conflict)).Should().Be("error");
        Describe(Result<object>.Success("x")).Should().Be("value");
        Describe(default).Should().Be("error");

        static string Describe(Result<object> result) => result switch
        {
            DomainError => "error",
            object => "value",
            null => "error"
        };
    }

    [Fact]
    public void Match_ObjectAndDomainErrorResults_BranchOnCase()
    {
        Result<object>.Failure(Conflict)
            .Match(_ => "success", _ => "failure").Should().Be("failure");
        Result<object>.Success(Conflict)
            .Match(_ => "success", _ => "failure").Should().Be("success");
        Result<DomainError>.Failure(Conflict)
            .Match(_ => "success", _ => "failure").Should().Be("failure");
        Result<DomainError>.Success(Conflict)
            .Match(_ => "success", _ => "failure").Should().Be("success");
    }

    [Fact]
    public void CustomResultsProblem_OverlappingTypes_MapsByCase()
    {
        CustomResults.Problem(Result<object>.Failure(Conflict)).Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        CustomResults.Problem(Result<DomainError>.Failure(Conflict)).Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        Ok<object> okObject = CustomResults.Problem(Result<object>.Success(Conflict))
            .Should().BeOfType<Ok<object>>().Subject;
        okObject.Value.Should().BeSameAs(Conflict);
        Ok<DomainError> okError = CustomResults.Problem(Result<DomainError>.Success(Conflict))
            .Should().BeOfType<Ok<DomainError>>().Subject;
        okError.Value.Should().BeSameAs(Conflict);
    }

    [Fact]
    public void ToHttpResult_OverlappingTypes_MapsByCase()
    {
        Result<object>.Failure(Conflict).ToHttpResult().Result
            .Should().BeOfType<DomainProblemResult>().Which.StatusCode.Should().Be(409);
        Result<DomainError>.Failure(Conflict).ToHttpResult().Result
            .Should().BeOfType<DomainProblemResult>().Which.StatusCode.Should().Be(409);
        Result<object>.Success(Conflict).ToHttpResult().Result
            .Should().BeOfType<Ok<object>>().Which.Value.Should().BeSameAs(Conflict);
        Result<DomainError>.Success(Conflict).ToHttpResult().Result
            .Should().BeOfType<Ok<DomainError>>().Which.Value.Should().BeSameAs(Conflict);

        Result<object>.Failure(Conflict).ToHttpResult(v => TypedResults.Ok(v)).Result
            .Should().BeOfType<DomainProblemResult>();
        Result<DomainError>.Success(Conflict).ToHttpResult(v => TypedResults.Ok(v)).Result
            .Should().BeOfType<Ok<DomainError>>();
    }

    [Fact]
    public async Task ToHttpResult_TaskOverlappingTypes_MapsByCase()
    {
        Results<Ok<object>, DomainProblemResult> failure = await Task.FromResult(Result<object>.Failure(Conflict)).ToHttpResult();
        Results<Ok<DomainError>, DomainProblemResult> success = await Task.FromResult(Result<DomainError>.Success(Conflict)).ToHttpResult();
        Results<Ok<DomainError>, DomainProblemResult> failureMapped = await Task.FromResult(Result<DomainError>.Failure(Conflict))
            .ToHttpResult(v => TypedResults.Ok(v));
        Results<Ok<object>, DomainProblemResult> successMapped = await Task.FromResult(Result<object>.Success(Conflict))
            .ToHttpResult(v => TypedResults.Ok(v));

        failure.Result.Should().BeOfType<DomainProblemResult>().Which.StatusCode.Should().Be(409);
        success.Result.Should().BeOfType<Ok<DomainError>>();
        failureMapped.Result.Should().BeOfType<DomainProblemResult>();
        successMapped.Result.Should().BeOfType<Ok<object>>();
    }

    [Fact]
    public void ResultOfT_FailureWithNoneOrNull_Throws()
    {
        AssertInvalid(() => Result<int>.Failure(DomainError.None));
        AssertInvalid(() => Result<int>.ValidationFailure(DomainError.None));
        AssertInvalid(() => Result.Failure<int>(DomainError.None));
        AssertInvalid(() => _ = ConvertToResult(DomainError.None));
        AssertInvalid(() => Result<object>.Failure(DomainError.None));

        Action nullError = () => Result<int>.Failure(null!);
        nullError.Should().Throw<ArgumentNullException>();

        static Result<int> ConvertToResult(DomainError error) => error;
    }

    [Fact]
    public void Result_FailureWithNoneOrNull_Throws()
    {
        AssertInvalid(() => Result.Failure(DomainError.None));
        AssertInvalid(() => _ = ConvertToResult(DomainError.None));
        AssertInvalid(() => _ = new Result(DomainError.None));

        Action nullError = () => Result.Failure(null!);
        nullError.Should().Throw<ArgumentNullException>();

        static Result ConvertToResult(DomainError error) => error;
    }

    [Fact]
    public void Success_Error_RemainsNone()
    {
        Result.Success().Error.Should().Be(DomainError.None);
        Result.Success(1).Error.Should().Be(DomainError.None);
    }

    private static void AssertInvalid(Action action) =>
        action.Should().Throw<ArgumentException>()
            .WithMessage("Invalid error*")
            .Which.ParamName.Should().Be("error");
}
