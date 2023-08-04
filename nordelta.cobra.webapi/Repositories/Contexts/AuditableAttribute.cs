using System;

namespace nordelta.cobra.webapi.Repositories.Contexts
{/// <summary>
/// Attribute used to indicate what entity models and database tables, need to be audited (i.e. track last modified time)
/// IMPORTANT: LAST MIGRATION NEED TO CREATE TRIGGERS. THIS LINE SHOULD BE ADDED MANUALLY
/// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditableAttribute : Attribute
    {
    }
}