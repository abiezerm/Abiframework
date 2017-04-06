using System;

namespace AbiFramework.Entities
{
  public static class ResultExtensions
  {
    public static Result OnSuccess(this Result result, Func<Result> func)
    {
      return result.Failure ? result : func();
    }

    public static Result OnSuccess(this Result result, Action action)
    {
      if (result.Failure)
        return result;

      action();

      return Result.Ok();
    }

    public static Result OnSuccess<T>(this Result<T> result, Action<T> action)
    {
      if (result.Failure)
        return result;

      action(result.Value);

      return Result.Ok();
    }

    public static Result<T> OnSuccess<T>(this Result result, Func<T> func)
    {
      return result.Failure ? Result.Fail<T>(result.Messages) : Result.Ok(func());
    }

    public static Result<T> OnSuccess<T>(this Result result, Func<Result<T>> func)
    {
      return result.Failure ? Result.Fail<T>(result.Messages) : func();
    }

    public static Result OnSuccess<T>(this Result<T> result, Func<T, Result> func)
    {
      return result.Failure ? result : func(result.Value);
    }

    public static Result OnFailure(this Result result, Action action)
    {
      if (result.Failure)
      {
        action();
      }

      return result;
    }

    public static Result OnBoth(this Result result, Action<Result> action)
    {
      action(result);

      return result;
    }

    public static T OnBoth<T>(this Result result, Func<Result, T> func)
    {
      return func(result);
    }
  }
}