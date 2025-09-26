using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Ref
{
    public enum LicenseType
    {
        /// <summary>
        /// Islamic financial license
        /// </summary>
        Islamic = 1,

        /// <summary>
        /// Commercial financial license
        /// </summary>
        Commercial = 2,

        /// <summary>
        /// Banking license
        /// </summary>
        Banking = 3,

        /// <summary>
        /// Exchange license
        /// </summary>
        Exchange = 4,

        /// <summary>
        /// FinTech / Technology-based financial license
        /// </summary>
        FinTech = 5
    }
}
