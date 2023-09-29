using Sitecore.Shell.Framework.Commands;
using System;
using SitecoreExtension.RestoreDeletedUsers.Utilities;
using System.Linq;
using Sitecore.Web.UI.WebControls;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.XamlSharp.Continuations;
using Sitecore.Web.UI.Sheer;
using System.Collections.Specialized;
using Sitecore.Text;

namespace SitecoreExtension.RestoreDeletedUsers.Commands
{
    [Serializable]
    public class DeletedUsersCommand : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull((object)context, nameof(context));
            ContinuationManager.Current.Start((ISupportsContinuation)this, "Run", new ClientPipelineArgs(new NameValueCollection(){}));
        }

        protected static void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (args.IsPostBack)
            {
                AjaxScriptManager.Current.Dispatch("usermanager:refresh");
            }
            else
            {
                SheerResponse.ShowModalDialog(new ModalDialogOptions(new UrlString("/sitecore/shell/Override/Applications/Security/UserManager/DeletedUsers.aspx").ToString())
                {
                    Width = "1200",
                    Height = "700",
                    Response = true
                });

                args.WaitForPostBack();
            }
        }

        public override CommandState QueryState(CommandContext context) => !this.IsAdvancedClient() || Sitecore.Context.Database.GetItem("/sitecore/content/Applications/Security/Deleted Users") == null ? CommandState.Hidden : base.QueryState(context);

        private bool GetArchivesUsers()
        {
            var users = DatabaseFunctions.GetUserArchives();

            return users.Any() ? false : true;
        }
    }
}
