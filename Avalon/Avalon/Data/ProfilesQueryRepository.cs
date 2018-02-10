using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class ProfilesQueryRepository : IProfilesQueryRepository
    {
        private readonly ProfileContext _context = null;

        public ProfilesQueryRepository(IOptions<Settings> settings)
        {
            _context = new ProfileContext(settings);
        }

        public async Task<IEnumerable<Profile>> GetLatestProfiles()
        {
            try
            {
                return _context.Profiles.AsQueryable().Take(1);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}
