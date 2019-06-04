﻿using Avalon.Model;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IHelperMethods
    {
        Task<Profile> GetCurrentUserProfile(ClaimsPrincipal user);
    }
}