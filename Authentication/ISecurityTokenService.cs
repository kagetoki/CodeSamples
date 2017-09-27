using IDS.Banking.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Authentication
{
    public interface ISecurityTokenService
    {
        string GenerateToken(string userId);
        IAccountHolder GetUser(string token);
        void DeleteKeyForUser(string userId);
    }
}
