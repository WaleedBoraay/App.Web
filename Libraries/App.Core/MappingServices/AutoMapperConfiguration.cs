using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.MappingServices
{
    /// <summary>
    /// AutoMApper configuration
    /// </summary>
    public static class AutoMapperConfiguration
    {
        /// <summary>
        /// MApper
        /// </summary>
        public static IMapper Mapper { get; private set; }

        /// <summary>
        /// MApper configuration
        /// </summary>
        public static MapperConfiguration MapperConfiguration { get; private set; }

        /// <summary>
        /// Initialize mApper
        /// </summary>
        /// <param name="config">MApper configuration</param>
        public static void Init(MapperConfiguration config)
        {
            MapperConfiguration = config;
            Mapper = config.CreateMapper();
        }
    }
}
