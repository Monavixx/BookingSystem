using System.Linq.Expressions;
using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Application.Persistence.Abstractions;

public abstract class ConstraintErrorRegistryBase
{
    public abstract DomainError? TryResolve(string tableName, string constraintName);
    public abstract ConstraintErrorRegistryBase Register<TEntity>(Expression<Func<TEntity, object?>> selector,
        ConstraintViolationType constraintViolationType, DomainError error);

    public ConstraintErrorRegistryBase Register<TEntity, TDomainError>(Expression<Func<TEntity, object?>> selector,
        ConstraintViolationType constraintViolationType, string code, string message)
        where TDomainError : DomainError
    {
        var error = (TDomainError)Activator.CreateInstance(typeof(TDomainError), code, message)!;
        return Register(selector, constraintViolationType, error);
    }
    public ConstraintErrorRegistryBase Register<TEntity, TDomainError>(Expression<Func<TEntity, object?>> selector,
        ConstraintViolationType constraintViolationType, params object[] parameters)
        where TDomainError : DomainError
    {
        var error = (TDomainError)Activator.CreateInstance(typeof(TDomainError), parameters)!;
        return Register(selector, constraintViolationType, error);
    }

    public ConstraintErrorRegistryBase RegisterUnique<TEntity, TDomainError>(
        Expression<Func<TEntity, object?>> selector, string code, string message)
        where TDomainError : DomainError
        => Register<TEntity, TDomainError>(selector, ConstraintViolationType.Unique, code, message);
    public ConstraintErrorRegistryBase RegisterUnique<TEntity>(
        Expression<Func<TEntity, object?>> selector, DomainError error)
        => Register(selector, ConstraintViolationType.Unique, error);
    public ConstraintErrorRegistryBase RegisterUnique<TEntity, TDomainError>(
        Expression<Func<TEntity, object?>> selector, params object[] parameters)
        where TDomainError : DomainError
        => Register<TEntity, TDomainError>(selector, ConstraintViolationType.Unique, parameters);
    
    public ConstraintErrorRegistryBase RegisterForeignKey<TEntity, TDomainError>(
        Expression<Func<TEntity, object?>> selector, string code, string message)
        where TDomainError : DomainError
        => Register<TEntity, TDomainError>(selector, ConstraintViolationType.ForeignKey, code, message);
    public ConstraintErrorRegistryBase RegisterForeignKey<TEntity>(
        Expression<Func<TEntity, object?>> selector, DomainError error)
        => Register(selector, ConstraintViolationType.ForeignKey, error);
    public ConstraintErrorRegistryBase RegisterForeignKey<TEntity, TDomainError>(
        Expression<Func<TEntity, object?>> selector, params object[] parameters)
        where TDomainError : DomainError
        => Register<TEntity, TDomainError>(selector, ConstraintViolationType.ForeignKey, parameters);
}