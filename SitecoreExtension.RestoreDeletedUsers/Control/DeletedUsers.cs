using System;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Web.UI.Grids;
using Sitecore.Shell.Web;
using Sitecore.Extensions;
using UserManager = Sitecore.Shell.Applications.Security.UserManager.UserManager;
using Sitecore;
using SitecoreExtension.RestoreDeletedUsers.Utilities;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;

namespace SitecoreExtension.RestoreDeletedUsers.Control
{
    public class DeletedUsers : UserManager, IHasCommandContext
    {
        private bool RebindRequired => !this.Page.IsPostBack && this.Request.QueryString["Cart_Users_Callback"] != "yes" || this.Page.Request.Params["requireRebind"] == "true";

        CommandContext IHasCommandContext.GetCommandContext()
        {
            CommandContext commandContext = new CommandContext();
            var itemNotNull = Client.GetItemNotNull("/sitecore/content/Applications/Security/Deleted Users/Ribbon", Client.CoreDatabase);
            commandContext.RibbonSourceUri = itemNotNull.Uri;
            string selectedValue = GridUtil.GetSelectedValue("Users");
            string empty = string.Empty;
            ListString listString = new ListString(selectedValue);
            if (listString.Count > 0)
                empty = listString[0].Split('^')[0];
            commandContext.Parameters["username"] = selectedValue;
            commandContext.Parameters["domainname"] = Sitecore.Security.SecurityUtility.GetDomainName();
            commandContext.Parameters["accountname"] = empty;
            commandContext.Parameters["accounttype"] = AccountType.User.ToString();
            return commandContext;
        }


        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            ShellPage.IsLoggedIn(true);
            base.OnLoad(e);
            Assert.CanRunApplication("Security/Deleted Users");

            ComponentArtGridHandler<User>.Manage(this.Users, (IGridSource<User>)new GridSource<User>(DatabaseFunctions.GetDeletedUsers()), this.RebindRequired);

            this.Users.LocalizeGrid();
            this.WriteLanguageAndBrowserCssClass();
        }

        private void WriteLanguageAndBrowserCssClass() => this.Form.Attributes["class"] = string.Format("{0} {1}", (object)UIUtil.GetBrowserClassString(), (object)UIUtil.GetLanguageCssClassString());
    }
}
