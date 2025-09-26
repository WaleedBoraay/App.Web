using System.Threading.Tasks;

namespace App.Services.Installation
{
    public interface IInstallationService
    {
        /// <summary>
        /// Run schema migrations (create all tables)
        /// </summary>
        ///Task InstallSchemaAsync();

        /// <summary>
        /// Install required system data (languages, countries, admin user, roles, etc.)
        /// </summary>
        Task InstallRequiredDataAsync();

        /// <summary>
        /// Install sample/test data
        /// </summary>
        Task InstallSampleDataAsync();
    }
}
