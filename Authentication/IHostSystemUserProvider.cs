using IDS.Banking.Abstraction;

namespace AgileFusion.Banking.Services.Authentication
{
    public interface IHostSystemUserProvider
    {
        IAccountHolder GetUser(string loginName, string userId);
    }
}
