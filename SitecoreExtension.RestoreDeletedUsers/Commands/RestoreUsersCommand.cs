using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;
using System.Collections.Specialized;
using System.Web;
using Sitecore.Shell.Framework.Commands.UserManager;
using Sitecore;
using Sitecore.Shell.Framework.Commands;
using SitecoreExtension.RestoreDeletedUsers.Utilities;

namespace SitecoreExtension.RestoreDeletedUsers.Commands
{
    public class RestoreUsersCommand : ManageUserCommand, ISupportsContinuation
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

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            ListString listString = new ListString(args.Parameters["username"]);
            if (args.IsPostBack)
            {
                if (!(args.Result == "yes"))
                    return;

                var stringList = DatabaseFunctions.RestoreUsers(listString);

                if (stringList.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (string str in stringList)
                    {
                        stringBuilder.Append('\n');
                        stringBuilder.Append(str);
                    }
                    SheerResponse.Alert("The following users could not be restored:\n\n{0}", stringBuilder.ToString());
                }

                //AjaxScriptManager.Current.Dispatch("usermanager:restore");

                if (listString.Count != 1 || !(listString[0] == Context.User.Name))
                {
                    return;
                }

                HttpContext.Current.Response.Redirect(Client.Site.LoginPage);
            }
            else
            {
                if (listString.Count == 1)
                {
                    string str = listString[0];
                    SheerResponse.Confirm(Translate.Text("Are you sure you want to restore \"{0}\"?", (object)str));
                }
                else
                    SheerResponse.Confirm(Translate.Text("Are you sure you want to restore these {0} users?", (object)listString.Count));
                args.WaitForPostBack();
            }
        }
    }
}
