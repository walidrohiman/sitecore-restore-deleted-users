using System;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Web.UI.Grids;
using Sitecore.Shell.Web;
using Sitecore.Extensions;
using UserManager = Sitecore.Shell.Applications.Security.UserManager.UserManager;
using Sitecore;
using SitecoreExtension.RestoreDeletedUsers.Utilities;

namespace SitecoreExtension.RestoreDeletedUsers
{
    public class CustomUserManager : UserManager
    {
        private bool RebindRequired => !this.Page.IsPostBack && this.Request.QueryString["Cart_Users_Callback"] != "yes" || this.Page.Request.Params["requireRebind"] == "true";

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            ShellPage.IsLoggedIn(true);
            base.OnLoad(e);
            Assert.CanRunApplication("Security/User Manager");
            ComponentArtGridHandler<User>.Manage(this.Users, (IGridSource<User>)new GridSource<User>(DatabaseFunctions.GetCustomUsers()), this.RebindRequired);

            this.Users.LocalizeGrid();
            this.WriteLanguageAndBrowserCssClass();
        }

        private void WriteLanguageAndBrowserCssClass() => this.Form.Attributes["class"] = string.Format("{0} {1}", (object)UIUtil.GetBrowserClassString(), (object)UIUtil.GetLanguageCssClassString());
    }
}
