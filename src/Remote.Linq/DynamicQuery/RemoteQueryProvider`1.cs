﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.DynamicQuery
{
    using Aqua.TypeSystem;
    using Remote.Linq.ExpressionVisitors;
    using System;
    using System.Linq;
    using Expression = System.Linq.Expressions.Expression;

    internal sealed class RemoteQueryProvider<TSource> : IRemoteQueryProvider
    {
        private readonly Func<Expressions.Expression, TSource> _dataProvider;
        private readonly IQueryResultMapper<TSource> _resultMapper;
        private readonly IQueryableResourceDescriptorProvider _queryableResourceProvider;
        private readonly Func<Expression, bool> _canBeEvaluatedLocally;

        internal RemoteQueryProvider(Func<Expressions.Expression, TSource> dataProvider, IQueryableResourceDescriptorProvider queryableResourceProvider, IQueryResultMapper<TSource> resultMapper, Func<Expression, bool> canBeEvaluatedLocally)
        {
            if (ReferenceEquals(null, dataProvider))
            {
                throw new ArgumentNullException(nameof(dataProvider));
            }

            _dataProvider = dataProvider;
            _resultMapper = resultMapper;
            _queryableResourceProvider = queryableResourceProvider;
            _canBeEvaluatedLocally = canBeEvaluatedLocally;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new RemoteQueryable<TElement>(this, expression);

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeHelper.GetElementType(expression.Type);
            return new RemoteQueryable(elementType, this, expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var rlinq = TranslateExpression(expression, _queryableResourceProvider, _canBeEvaluatedLocally);
            var dataRecords = _dataProvider(rlinq);
            var result = object.Equals(default(TSource), dataRecords)
                ? default(TResult)
                : _resultMapper.MapResult<TResult>(dataRecords, expression);
            return result;
        }

        public object Execute(Expression expression)
            => throw new NotImplementedException();

        internal static Expressions.Expression TranslateExpression(Expression expression, IQueryableResourceDescriptorProvider queryableResourceProvider, Func<Expression, bool> canBeEvaluatedLocally)
        {
            var slinq1 = expression.SimplifyIncorporationOfRemoteQueryables();
            var rlinq1 = slinq1.ToRemoteLinqExpression(canBeEvaluatedLocally);
            var rlinq2 = rlinq1.ReplaceQueryableByResourceDescriptors(queryableResourceProvider);
            var rlinq3 = rlinq2.ReplaceGenericQueryArgumentsByNonGenericArguments();
            return rlinq3;
        }
    }
}
