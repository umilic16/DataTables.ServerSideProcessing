using System.Linq.Expressions;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal record PropertyExpressionParts(ParameterExpression Parameter, MemberExpression MemberAccess, Type PropertyType);
