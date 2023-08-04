using System;

namespace nordelta.cobra.webapi.Repositories.Contexts
{
    /// <summary>
    /// Attribute used to indicate what entity models shouldn't be deleted physically from database and use a 'deletion bit' instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SoftDeleteAttribute : Attribute
    {
    }
}
