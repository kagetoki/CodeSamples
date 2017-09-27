using IDS.Banking.Abstraction;
using IDS.ComponentModel;
using IDS.Banking;
using IDS.Security;


namespace AgileFusion.Banking.Services.Authentication
{
    public class HostSystemUserProvider : IHostSystemUserProvider
    {
        [ComponentSetting("Host System", "The Host system to find cached account holder by login tokens", "Services", WellKnownComponent = true)]
        public HostSystem HostSystem { get; set; }
        public IAccountHolder GetUser(string loginName, string userId)
        {
            var cachingHostSystem = HostSystem as ICachingHostSystem;
            var credentials = new Credentials { LoginName = loginName };
            return cachingHostSystem?.GetCachedAccountHolder(credentials, userId) ?? HostSystem?.GetAccountHolder(credentials, userId);
        }
    }
}
