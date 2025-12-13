# AbiFramework

[![NuGet](https://img.shields.io/nuget/v/AbiFramework.svg)](https://www.nuget.org/packages/AbiFramework)
[![NuGet](https://img.shields.io/nuget/dt/AbiFramework.svg)](https://www.nuget.org/packages/AbiFramework)

A .NET framework library designed to help developers implement **Clean Architecture** and enforce **SOLID principles** in their applications. AbiFramework provides foundational abstractions and base classes that promote separation of concerns, dependency inversion, and maintainable code structure.

## Purpose

AbiFramework was created to provide a solid foundation for building enterprise applications following Clean Architecture principles. The library enforces SOLID principles through:

- **Single Responsibility Principle (SRP)**: Clear separation between entities, repositories, and unit of work patterns
- **Open/Closed Principle (OCP)**: Extensible base classes and interfaces that can be extended without modification
- **Liskov Substitution Principle (LSP)**: Well-defined interfaces that can be substituted with concrete implementations
- **Interface Segregation Principle (ISP)**: Focused interfaces like `IReadOnlyRepository` and `IWriteRepository` that segregate read and write operations
- **Dependency Inversion Principle (DIP)**: Repository and Unit of Work patterns that depend on abstractions, not concrete implementations
- **CQRS Pattern**: Command Query Responsibility Segregation through `ICommand`/`ICommandHandler` and `IQuery`/`IQueryHandler` interfaces

## Features

### Core Entities

- **Base Entity Classes**: `AEntity<TPrimaryKey>` with built-in domain event support
- **Result Pattern**: `Result<T>` and `Result` classes for functional error handling
- **Operation Results**: `OperationResult<T>` for operation status tracking
- **Paged Results**: `PagedListResult<T>` for paginated data queries
- **Auditable Entities**: Support for tracking creation and modification dates
- **Activatable Entities**: Support for soft-delete patterns

### Repository Pattern

- **Read-Only Repository**: `IReadOnlyRepository<TEntity, TPrimaryKey>` for query operations
- **Async Read-Only Repository**: `IReadOnlyRepositoryAsync<TEntity, TPrimaryKey>` for asynchronous queries
- **Write Repository**: `IWriteRepository<TEntity, TPrimaryKey>` for data modification operations
- **Paged Search**: Built-in support for paginated queries with sorting

### Unit of Work

- **Transaction Management**: `IUnitOfWork` interface for managing database transactions
- **Async Support**: Both synchronous and asynchronous commit operations

### Domain Events

- **Event-Driven Architecture**: Built-in domain event support in base entities
- **Event Raising**: Simple API for raising and managing domain events

### CQRS / Messaging Pattern

- **Commands**: `ICommand` and `ICommand<TResponse>` marker interfaces for command operations
- **Command Handlers**: `ICommandHandler<TCommand>` and `ICommandHandler<TCommand, TResponse>` for processing commands with Result pattern integration
- **Queries**: `IQuery<TResponse>` marker interface for query operations
- **Query Handlers**: `IQueryHandler<TQuery, TResponse>` for processing queries with Result pattern integration
- **Result Integration**: All handlers return `Result` or `Result<TResponse>` for consistent error handling

### Helpers

- **Predicate Builder**: Dynamic LINQ expression building for complex queries
- **Cryptography**: Hash helper utilities

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package AbiFramework
```

Or via Package Manager Console:

```powershell
Install-Package AbiFramework
```

Or add directly to your `.csproj` file:

```xml
<PackageReference Include="AbiFramework" Version="4.0.0" />
```

## Usage Examples

### Entity with Domain Events

```csharp
using AbiFramework.Entities;

public class User : AEntity<int>
{
    public string Name { get; set; }
    public string Email { get; set; }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
        Raise(new EmailChangedEvent(Id, newEmail));
    }
}

public record EmailChangedEvent(int UserId, string NewEmail) : IDomainEvent;
```

### Result Pattern

```csharp
using AbiFramework.Entities;

// Success case
var result = Result.Success();
var valueResult = Result.Success(user);

// Failure case
var error = new Error("User.NotFound", "User not found");
var failure = Result.Failure(error);
var failureWithValue = Result<User>.Failure(error);

// Usage
if (result.IsSuccess)
{
    var user = valueResult.Value; // Safe access
}
```

### Using the Repository Pattern

```csharp
using AbiFramework.Repositories;
using AbiFramework.Entities;

public interface IUserRepository : IReadOnlyRepository<User, int>, IWriteRepository<User, int>
{
}

// In your service
public class UserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository; // Dependency injection
    }
    
    public async Task<User?> GetUserAsync(int id)
    {
        return await _repository.FindAsync(id);
    }
    
    public PagedListResult<User> SearchUsers(string searchTerm, int page, int pageSize)
    {
        var expression = PredicateBuilder.True<User>()
            .AndIf(!string.IsNullOrEmpty(searchTerm), 
                u => u.Name.Contains(searchTerm));
        
        return _repository.PagedSearch(expression, pageSize, page * pageSize);
    }
}
```

### Unit of Work Pattern

```csharp
using AbiFramework.UnitsOfWork;

public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    
    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }
    
    public async Task CreateUserAsync(User user)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            _userRepository.Add(user);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }
    }
}
```

### CQRS Pattern (Commands and Queries)

```csharp
using AbiFramework.Messaging;
using AbiFramework.Entities;
using AbiFramework.Repositories;
using AbiFramework.UnitsOfWork;

// Define a command
public record CreateUserCommand(string Name, string Email) : ICommand<int>;

// Define a command handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, int>
{
    private readonly IRepository<User, int> _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateUserCommandHandler(IRepository<User, int> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<int>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User { Name = command.Name, Email = command.Email };
        _repository.Add(user);
        
        _unitOfWork.BeginTransaction();
        await _unitOfWork.CommitAsync();
        
        return Result.Success(user.Id);
    }
}

// Define a query
public record GetUserByIdQuery(int UserId) : IQuery<User>;

// Define a query handler
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, User>
{
    private readonly IReadOnlyRepositoryAsync<User, int> _repository;
    
    public GetUserByIdQueryHandler(IReadOnlyRepositoryAsync<User, int> repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _repository.Find(query.UserId);
        
        if (user == null)
        {
            return Result<User>.Failure(new Error("User.NotFound", "User not found"));
        }
        
        return Result.Success(user);
    }
}
```

### Predicate Builder

```csharp
using AbiFramework.Helpers;

var predicate = PredicateBuilder.True<User>()
    .AndIf(!string.IsNullOrEmpty(name), u => u.Name.Contains(name))
    .AndIf(age.HasValue, u => u.Age == age.Value)
    .AndIf(isActive.HasValue, u => u.IsActive == isActive.Value);

var users = _repository.FilterBy(predicate);
```

## Target Framework

This library targets **.NET 10.0**, providing access to the latest C# features and performance improvements.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Links

- [NuGet Package](https://www.nuget.org/packages/AbiFramework)
- [GitHub Repository](https://github.com/abiezerm/Abiframework)
