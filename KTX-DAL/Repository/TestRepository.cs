using KTX_DAL.IRepository;
using KTX_DAL.Models.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Repository
{
    public class TestRepository : Repository<Test>, ITestRepository
    {
        public TestRepository(IConfiguration configuration, ILogger<Test> logger) : base(configuration, logger)
        {
        }
    }
}
