using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexer.UnitTesting
{
    public class BaseUnitTest
    {
        protected IConfiguration Configuration { get; set; }
        public BaseUnitTest()
        {
            Configuration = new ConfigurationBuilder()
                                .SetBasePath(AppContext.BaseDirectory)
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();
        }
    }
}
