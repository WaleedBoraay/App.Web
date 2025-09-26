using App.Core.Configuration;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.DataProviders
{
    /// <summary>
    /// Represents temporary storage
    /// </summary>
    /// <typeparam name="T">Storage record mapping class</typeparam>
    public partial class TempSqlDataStorage<T> : TempTable<T>, ITempDataStorage<T> where T : class
    {
        #region Ctor

        public TempSqlDataStorage(string storageName, IQueryable<T> query, IDataContext dataConnection)
            : base(dataConnection, storageName, query, tableOptions: TableOptions.NotSet | TableOptions.DropIfExists)
        {
            dataConnection.CloseAfterUse = true;
        }

        #endregion
    }
}
