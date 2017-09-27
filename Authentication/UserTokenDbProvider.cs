using IDS.ComponentModel;
using IDS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Authentication
{
    public class UserTokenDbProvider : IUserTokenDbProvider
    {
        private const string INSERT_KEY_FMT = @"insert into AFAPP_SecurityToken (UserId, [Key], Dataspace) select @userId, @key, @dataspace";
        private const string GET_USER_KEY_FMT = @"select [Key] from AFAPP_SecurityToken where UserId = @userId and Dataspace = @dataspace";
        private const string DELETE_KEY_FMT = @"delete from AFAPP_SecurityToken where UserId = @userId and Dataspace = @dataspace";
        private const string GET_LOGIN_FMT = @"select [LoginName] from IDS_UserSecurity where UserId = @userId and Dataspace = @dataspace";

        [ComponentSetting("Sql Connection Service", "Services", WellKnownComponent = true)]
        public SqlConnectionService SqlConnection { get; set; }
        public void DeleteUserKey(string userId)
        {
            ExecuteSql(DELETE_KEY_FMT, new SqlParameter("@" + nameof(userId), userId),
                                       new SqlParameter("@dataspace", Dataspace.Current.Name));
        }

        public string GetLoginName(string userId)
        {
            return ExecuteSql<string>(GET_LOGIN_FMT, new SqlParameter("@" + nameof(userId), userId),
                                                     new SqlParameter("@dataspace", Dataspace.Current.Name));
        }

        public byte[] GetUserKey(string userId)
        {
            return ExecuteSql<byte[]>(GET_USER_KEY_FMT, new SqlParameter("@" + nameof(userId), userId),
                                                        new SqlParameter("@dataspace", Dataspace.Current.Name));
        }

        public void InsertUserKey(string userId, byte[] key)
        {
            ExecuteSql(INSERT_KEY_FMT, new SqlParameter("@" + nameof(userId), userId), 
                                       new SqlParameter("@" + nameof(key), key), 
                                       new SqlParameter("@dataspace", Dataspace.Current.Name));
        }

        private T ExecuteSql<T>(string sql, params SqlParameter[] parameters)
        {
            T result = default(T);
            using (var connection = SqlConnection.GetOpenConnection())
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = (T)reader[0];
                    }
                }
            }
            return result;
        }
        private void ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            using (var connection = SqlConnection.GetOpenConnection())
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }
    }
    
}
