using System;

namespace AbiFramework.UnitsOfWork;

[AttributeUsage(AttributeTargets.Method)]
public sealed class UnitOfWorkAttribute : Attribute
{
}
