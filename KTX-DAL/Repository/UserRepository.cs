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
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IConfiguration configuration, ILogger<User> logger) : base(configuration, logger)
        {
        }
    }
}
