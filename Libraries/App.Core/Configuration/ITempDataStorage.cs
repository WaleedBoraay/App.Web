using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Configuration
{
    /// <summary>
    /// Represents temporary storage
    /// </summary>
    /// <typeparam name="T">Storage record mapping class</typeparam>
    public partial interface ITempDataStorage<T> : IQueryable<T>, IDisposable, IAsyncDisposable where T : class
    {
    }
}
