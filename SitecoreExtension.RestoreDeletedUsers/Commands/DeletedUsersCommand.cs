using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using System;
using SitecoreExtension.RestoreDeletedUsers.Utilities;
using System.Linq;

namespace SitecoreExtension.RestoreDeletedUsers.Commands
{
    [Serializable]
    public class DeletedUsersCommand : Command
    {
        public override void Execute(CommandContext context) => Windows.RunApplication("Security/Deleted Users");

        public override CommandState QueryState(CommandContext context) => !this.IsAdvancedClient() || Sitecore.Context.Database.GetItem("/sitecore/content/Applications/Security/Deleted Users") == null ? CommandState.Hidden : base.QueryState(context);

        private bool GetArchivesUsers()
        {
            var users = DatabaseFunctions.GetUserArchives();

            return users.Any() ? false : true;
        }
    }
}
