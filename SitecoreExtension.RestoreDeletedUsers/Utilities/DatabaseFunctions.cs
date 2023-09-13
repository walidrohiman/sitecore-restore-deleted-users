using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Sitecore.Diagnostics;
using Sitecore.Text;
using SitecoreExtension.RestoreDeletedUsers.Models;
using Dapper;
using System.Data;

namespace SitecoreExtension.RestoreDeletedUsers.Utilities
{
    public static class DatabaseFunctions
    {
        public static List<string> GetUserInformation(ListString users)
        {
            var notFoundList = new List<string>();

            using (IDbConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["core"].ConnectionString))
            {
                foreach (var username in users)
                {
                    try
                    {
                        var query = $@"SELECT * FROM aspnet_Users WHERE UserName = '{username}'";

                        var result = connection.QueryFirst<SitecoreUserModel>(query);

                        if (result != null)
                        {
                            //create user on new table
                            InsertUserIntoTable(result);

                            //delete user from sitecore
                            var deleteQuery = $"DELETE FROM aspnet_Users WHERE UserName = '{username}'";

                            var deleteResult = connection.Execute(deleteQuery, new { UserName = username });

                            if(deleteResult == 0)
                            {
                                notFoundList.Add(username);
                            }
                        }
                        else
                        {
                            notFoundList.Add(username);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in processing username: {username} | {ex.Message}", nameof(GetUserInformation));
                    }
                }
            }

            return notFoundList;
        }

        public static void InsertUserIntoTable(SitecoreUserModel user)
        {
            using (IDbConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SitecoreExtensions"].ConnectionString))
            {
                try
                {
                    var query = @"INSERT INTO UserArchives 
                            (ApplicationId, UserId, UserName, LoweredUserName, MobileAlias, IsAnonymous, LastActivityDate) 
                            VALUES 
                            (@ApplicationId, @UserId, @UserName, @LoweredUserName, @MobileAlias, @IsAnonymous, @LastActivityDate)";

                    connection.Execute(query, user);
                }
                catch(Exception ex)
                {
                    Log.Error($"Error in creating user: {user.UserName} | {ex.Message}", nameof(InsertUserIntoTable));
                }         
            }
        }
    }
}
