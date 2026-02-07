namespace AbiFramework.Entities;

/// <summary>
/// Enumeration for different types of errors
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// General failure error
    /// </summary>
    Failure = 0,
    
    /// <summary>
    /// Validation error
    /// </summary>
    Validation = 1,
    
    /// <summary>
    /// Problem error
    /// </summary>
    Problem = 2,
    
    /// <summary>
    /// Not found error
    /// </summary>
    NotFound = 3,
    
    /// <summary>
    /// Conflict error
    /// </summary>
    Conflict = 4
}