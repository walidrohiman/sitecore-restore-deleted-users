using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Sitecore.Diagnostics;
using Sitecore.Text;
using SitecoreExtension.RestoreDeletedUsers.Models;
using Dapper;
using System.Data;
using System.Web.Security;
using Sitecore.Security.Accounts;
using System.Linq;

namespace SitecoreExtension.RestoreDeletedUsers.Utilities
{
    public static class DatabaseFunctions
    {
        public static IEnumerable<User> GetUsers()
        {
            var users = UserManager.GetUsers().ToList();

            var archiveUsers = GetUserArchives();

            if (archiveUsers.Any())
            {
                users = users.Where(x => !archiveUsers.Contains(x.Name)).ToList();
            }

            return users;
        }

        public static IEnumerable<User> GetDeletedUsers()
        {
            var users = UserManager.GetUsers().ToList();

            var archiveUsers = GetUserArchives();

            if (archiveUsers.Any())
            {
                return users.Where(x => archiveUsers.Contains(x.Name)).ToList();
            }

            return new List<User>();
        }

        public static List<string> GetUserArchives()
        {
            var listUserName = new List<string>();

            using (IDbConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SitecoreExtensions"].ConnectionString))
            {
                try
                {
                    var query = $"SELECT * FROM UserArchives";
                    var result = connection.Query<SitecoreUserModel>(query);

                    foreach (var user in result)
                    {
                        listUserName.Add(user.UserName);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error in fetching users from UserArchives | {ex.Message}", nameof(GetUserArchives));
                }

                return listUserName;
            }
        }

        public static List<string> GetUserInformation(ListString users)
        {
            var notFoundList = new List<string>();

            using (IDbConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["core"].ConnectionString))
            {
                foreach (var username in users)
                {
                    try
                    {
                        MembershipUser user = Membership.GetUser(username);

                        if (user != null)
                        {
                            user.IsApproved = false;
                            Membership.UpdateUser(user);

                            var query = $"SELECT * FROM aspnet_Users WHERE UserName = @UserName";
                            var result = connection.QueryFirst<SitecoreUserModel>(query, new { UserName = username });

                            if (result != null)
                            {
                                InsertUserIntoTable(result);
                            }
                            else
                            {
                                notFoundList.Add(username);

                                user.IsApproved = true;
                                Membership.UpdateUser(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in processing username {ex.Message}", nameof(GetUserInformation));
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
                catch (Exception ex)
                {
                    Log.Error($"Error in creating user {ex.Message}", nameof(GetUserArchives));
                }
            }
        }

        public static List<string> RestoreUsers(ListString users)
        {
            var notRestoreList = new List<string>();

            using (IDbConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SitecoreExtensions"].ConnectionString))
            {
                foreach (var username in users)
                {
                    MembershipUser user = Membership.GetUser(username);

                    if (user != null)
                    {
                        user.IsApproved = true;
                        Membership.UpdateUser(user);

                        try
                        {
                            var query = $"DELETE FROM UserArchives WHERE UserName = @UserName";
                            connection.Execute(query, new { UserName = username });
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error in restoring user {ex.Message}", nameof(GetUserArchives));
                            notRestoreList.Add(username);
                        }
                    }
                    else
                    {
                        notRestoreList.Add(username);
                    }
                }
            }

            return notRestoreList;
        }

        public static void DeleteUsersPermanently(List<string> users)
        {
            using (IDbConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SitecoreExtensions"].ConnectionString))
            {
                foreach (var username in users)
                {
                    try
                    {
                        var query = $"DELETE FROM UserArchives WHERE UserName = @UserName";
                        connection.Execute(query, new { UserName = username });
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in processing username {ex.Message}", nameof(GetUserInformation));
                    }
                }
            }
        }
    }
}
