using nordelta.cobra.webapi.Models;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Repositories.Contracts;

public interface IValidacionClienteRepository
{
    IEnumerable<ValidacionCliente> Sync(IEnumerable<ValidacionCliente> entities);
    ValidacionCliente GetSingle(Expression<Func<ValidacionCliente, bool>> predicate);
}
