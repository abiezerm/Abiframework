using System;
using System.Collections.Generic;

namespace AbiFramework.Entities
{
  public class Result
  {
    public bool Success { get; private set; }
    public List<string> Messages { get; private set; }



    public bool Failure => !Success;

      protected Result(bool success, string message)
    {
      Success = success;
      Messages = new List<string>();
      Messages.Add(message);
    }

    protected Result(bool success, List<string> message)
    {
      Success = success;
      Messages = message;
    }

    public static Result Fail(string message)
    {
      return new Result(false, message);
    }

    public static Result Fail(List<string> messages)
    {
      return new Result(false, messages);
    }


    public static Result<T> Fail<T>(string message)
    {
      return new Result<T>(default(T), false, message);
    }

    public static Result<T> Fail<T>(List<string> messages)
    {
      return new Result<T>(default(T), false, messages);
    }


    public static Result Ok()
    {
      return new Result(true, String.Empty);
    }

    public static Result<T> Ok<T>(T value, string message = "")
    {
      return new Result<T>(value, true, message);
    }

    public static Result Combine(params Result[] results)
    {
      foreach (Result result in results)
      {
        if (result.Failure)
          return result;
      }

      return Ok();
    }
  }


  public class Result<T> : Result
  {
    public T Value { get; private set; }

    protected internal Result(T value, bool success, string message)
      : base(success, message)
    {
      Value = value;
    }

    protected internal Result(T value, bool success, List<string> messages)
      : base(success, messages)
    {
      Value = value;
    }

    public Result<T> AddMessage(string message)
    {
      Messages.Add(message);
      return this;
    }
  }
}