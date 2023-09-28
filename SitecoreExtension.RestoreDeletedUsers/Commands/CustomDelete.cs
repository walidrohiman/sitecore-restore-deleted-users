using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Framework.Commands.UserManager;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;
using System.Collections.Specialized;
using Delete = Sitecore.Shell.Framework.Commands.UserManager.Delete;
using Sitecore.Text;
using System.Collections.Generic;
using Sitecore.Security;
using Sitecore.Globalization;
using Sitecore.Web.Authentication;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Linq;
using SitecoreExtension.RestoreDeletedUsers.Utilities;

namespace SitecoreExtension.RestoreDeletedUsers.Commands
{
    public class CustomDelete : Delete
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull((object)context, nameof(context));
            string parameter = context.Parameters["username"];
            if (!ValidationHelper.ValidateUserWithMessage(parameter) || !this.CanCurrentUserManageSelected(parameter))
                return;
            ContinuationManager.Current.Start((ISupportsContinuation)this, "Run", new ClientPipelineArgs(new NameValueCollection()
            {
                ["username"] = parameter
            }));
        }

        protected new void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            ListString listString = new ListString(args.Parameters["username"]);
            if (args.IsPostBack)
            {
                if (!(args.Result == "yes"))
                    return;
                List<string> stringList = new List<string>();
                foreach (string str in listString)
                {
                    SecurityAudit.LogDeleteUser((object)this, str);
                    if (!Membership.DeleteUser(str))
                    {
                        Log.Audit((object)this, "Failed to delete user: {0}", str);
                        stringList.Add(str);
                    }
                }

                //remove stringList from listString
                var usersList = listString.ToString().Split('|').ToList();
                usersList = usersList.Where(user => !stringList.Contains(user)).ToList();

                DatabaseFunctions.DeleteUsersPermanently(usersList);

                if (stringList.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (string str in stringList)
                    {
                        stringBuilder.Append('\n');
                        stringBuilder.Append(str);
                    }
                    SheerResponse.Alert("The following users could not be deleted:\n\n{0}", stringBuilder.ToString());
                }
                AjaxScriptManager.Current.Dispatch("usermanager:userdeleted");
                if (listString.Count != 1 || !(listString[0] == Sitecore.Context.User.Name))
                    return;
                Sitecore.Shell.Framework.Security.Abandon();
                string currentTicketId = TicketManager.GetCurrentTicketId();
                if (!string.IsNullOrEmpty(currentTicketId))
                    TicketManager.RemoveTicket(currentTicketId);
                HttpContext.Current.Response.Redirect(Sitecore.Client.Site.LoginPage);
            }
            else
            {
                if (listString.Count == 1)
                {
                    string str = listString[0];
                    if (str == Sitecore.Context.User.Name)
                        SheerResponse.Confirm(Translate.Text("Are you sure you want to delete yourself?\n\nThis will log you off the system."));
                    else
                        SheerResponse.Confirm(Translate.Text("Are you sure you want to delete \"{0}\"?", (object)str));
                }
                else
                    SheerResponse.Confirm(Translate.Text("Are you sure you want to delete these {0} users?", (object)listString.Count));
                args.WaitForPostBack();
            }
        }
    }
}
