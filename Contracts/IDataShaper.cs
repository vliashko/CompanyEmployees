using System.Collections.Generic;
using System.Dynamic;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<ExpandoObject> SpapeData(IEnumerable<T> entities, string fieldsString);
        ExpandoObject SpapeData(T entity, string fieldsString);
    }
}
