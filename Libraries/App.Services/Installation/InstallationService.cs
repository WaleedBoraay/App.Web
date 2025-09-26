using App.Data.Migrations.Installation;
using App.Data.MigratorServices;
using System.Threading.Tasks;

namespace App.Services.Installation
{
    public class InstallationService : IInstallationService
    {
        private readonly InstallRequiredData _installRequiredData;
        private readonly InstallSampleData _installSampleData;
        private readonly IMigrationManager _migrationManager;

        public InstallationService(InstallRequiredData installRequiredData, InstallSampleData installSampleData)
        {
            _installRequiredData = installRequiredData;
            _installSampleData = installSampleData;
        }

        /// <summary>
        /// Run schema migrations (create all tables)
        /// </summary>
        //public async Task InstallSchemaAsync()
        //{
        //    // Apply all schema migrations (tables creation)
        //    _migrationManager.ApplyUpMigrations(typeof(SchemaMigration).Assembly, MigrationProcessType.Installation, false);

        //    await Task.CompletedTask;
        //}

        public async Task InstallRequiredDataAsync()
        {
            await _installRequiredData.InstallAsync();
        }

        public async Task InstallSampleDataAsync()
        {
            await _installSampleData.InstallAsync();
        }
    }
}
