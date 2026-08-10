using System.Collections.Frozen;
using System.Linq.Expressions;
using BookingSystem.Application.Persistence;
using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BookingSystem.Infrastructure.Services;

public sealed class ConstraintErrorRegistry : ConstraintErrorRegistryBase
{
    private List<ConstraintErrorDescriptor> _descriptors = [];
    private readonly Lazy<FrozenDictionary<(string, string), DomainError>> _resolved;

    public ConstraintErrorRegistry(IModel model)
    {
        _resolved = new Lazy<FrozenDictionary<(string, string), DomainError>>(() => Build(model),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
    public ConstraintErrorRegistry()
    {
        _resolved = new(() => FrozenDictionary.Create<(string, string), DomainError>());
    }

    public override ConstraintErrorRegistryBase Register<TEntity>(Expression<Func<TEntity, object?>> selector,
        ConstraintViolationType constraintViolationType, DomainError error)
    {
        if (constraintViolationType is not ConstraintViolationType.Unique
            and not ConstraintViolationType.ForeignKey)
            throw new ArgumentException("Invalid constraintViolationType", nameof(constraintViolationType));

        var propertyNames = ExtractProperties(selector);
        _descriptors.Add(new ConstraintErrorDescriptor(typeof(TEntity), propertyNames, error, constraintViolationType));
        return this;
    }

    public override DomainError? TryResolve(string tableName, string constraintName)
        => _resolved.Value.GetValueOrDefault((tableName, constraintName));

    private FrozenDictionary<(string, string), DomainError> Build(IModel model)
    {
        var result = new Dictionary<(string, string), DomainError>();
        foreach (var descriptor in _descriptors)
        {
            var entityType = model.FindEntityType(descriptor.ClrEntityType)
                             ?? throw new InvalidConstraintErrorDescriptorException(
                                 $"{descriptor.ClrEntityType.Name} is not a valid entity type");
            var constraintName = descriptor.ConstraintViolationType switch
            {
                ConstraintViolationType.Unique =>
                    entityType
                        .GetIndexes()
                        .SingleOrDefault(index =>
                            index.IsUnique &&
                            index.Properties
                                .Select(p => p.Name)
                                .SequenceEqual(
                                    descriptor.PropertyNames))
                        ?.GetDatabaseName()
                    ?? throw new
                        InvalidConstraintErrorDescriptorException(
                            $"No unique index found on {descriptor.ClrEntityType.Name} " +
                            $"for properties: {string.Join(", ", descriptor.PropertyNames)}"),
                ConstraintViolationType.ForeignKey =>
                    entityType
                        .GetForeignKeys()
                        .SingleOrDefault(fk => fk.Properties
                            .Select(p => p.Name)
                            .SequenceEqual(descriptor.PropertyNames))
                        ?.GetConstraintName()
                    ?? throw new InvalidConstraintErrorDescriptorException(
                        $"No foreign key found on {descriptor.ClrEntityType.Name} " +
                        $"for properties: {string.Join(", ", descriptor.PropertyNames)}"),
                _ => throw new InvalidOperationException(
                    $"Unsupported constraint violation type: {descriptor.ConstraintViolationType}")
            };

            result[(entityType.GetTableName()!, constraintName)] = descriptor.Error;
        }

        _descriptors.Clear();
        _descriptors = null!;

        AddPrimaryKeys(result, model);

        return result.ToFrozenDictionary();
    }

    private static string[] ExtractProperties<TEntity>(Expression<Func<TEntity, object?>> expression)
        => expression.Body switch
        {
            MemberExpression memberExpression => [memberExpression.Member.Name],
            UnaryExpression { Operand: MemberExpression memberExpression } => [memberExpression.Member.Name],
            NewExpression { Members: not null } newExpression =>
                [.. newExpression.Members.Select(m => m.Name)],
            _ => throw new ArgumentException(
                "Invalid expression format. Expected a member access or an anonymous object creation.",
                nameof(expression))
        };

    private static void AddPrimaryKeys(Dictionary<(string, string), DomainError> dict, IModel model)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var pk = entityType.FindPrimaryKey()?.GetName();
            if (pk is null) continue;
            dict[(entityType.GetTableName()!, pk)] = new ConflictError(
                $"{entityType.ClrType.Name}.PrimaryKey",
                $"A record with the same primary key already exists in {entityType.GetTableName()}");
        }
    }
}
