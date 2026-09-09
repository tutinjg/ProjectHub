using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Common; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace ProjectHub.Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyGlobalFilters<TInterface>(
        this ModelBuilder modelBuilder,
        Expression<Func<TInterface, bool>> expression)
    {
        var entities = modelBuilder.Model
            .GetEntityTypes()
            .Where(e => typeof(TInterface).IsAssignableFrom(e.ClrType));

        foreach (var entity in entities)
        {
            var newParam = Expression.Parameter(entity.ClrType);
            var newBody = ReplacingExpressionVisitor.Replace(
                expression.Parameters.Single(),
                newParam,
                expression.Body);

            var lambda = Expression.Lambda(newBody, newParam);
            modelBuilder.Entity(entity.ClrType).HasQueryFilter(lambda);
        }
    }
}