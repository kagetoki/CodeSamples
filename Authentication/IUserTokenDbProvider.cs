namespace AgileFusion.Banking.Services.Authentication
{
    public interface IUserTokenDbProvider
    {
        byte[] GetUserKey(string userId);
        void InsertUserKey(string userId, byte[] key);
        string GetLoginName(string userId);
        void DeleteUserKey(string userId);
    }
}
