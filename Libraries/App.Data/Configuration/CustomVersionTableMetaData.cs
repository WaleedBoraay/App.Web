using FluentMigrator.Runner.VersionTableInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Configuration
{
    public sealed class CustomVersionTableMetaData : IVersionTableMetaData
    {
        public string SchemaName => "dbo";
        public string TableName => "App_VersionInfo";
        public string ColumnName => "Version";
        public string AppliedOnColumnName => "AppliedOn";
        public string DescriptionColumnName => "Description";
        public string UniqueIndexName => "UC_App_VersionInfo";
        public bool OwnsSchema => false;

        public bool CreateWithPrimaryKey => false;
    }
}
