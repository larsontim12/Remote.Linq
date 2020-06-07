﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.DynamicQuery
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class RemoteQueryable<T> : RemoteQueryable, IOrderedRemoteQueryable<T>
    {
        internal RemoteQueryable(IRemoteQueryProvider provider, Expression? expression = null)
            : base(typeof(T), provider, expression)
        {
        }

        public IEnumerator<T> GetEnumerator()
            => Execute().GetEnumerator();

        public IEnumerable<T> Execute()
            => Provider.Execute<IEnumerable<T>>(Expression);
    }
}
