using System;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Web.UI.Grids;
using Sitecore.Shell.Web;
using Sitecore.Extensions;
using Sitecore;
using SitecoreExtension.RestoreDeletedUsers.Utilities;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using System.Web.UI;
using ComponentArt.Web.UI;
using Sitecore.Web.UI.WebControls.Ribbons;
using System.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System.Web;
using System.Web.UI.WebControls;
using Sitecore.Web;
using Sitecore.Resources;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Ajax;

namespace SitecoreExtension.RestoreDeletedUsers.Control
{
    public class DeletedUsers : Page, IHasCommandContext
    {
        protected HtmlGenericControl PageBody;

        protected HtmlForm DeletedUsersForm;

        protected Ribbon Ribbon;

        protected Grid Users;

        protected GridServerTemplate CommentTemplate;

        protected GridServerTemplate FullNameTemplate;

        protected ClientTemplate LocalNameTemplate;

        protected ClientTemplate LoadingFeedbackTemplate;

        protected ClientTemplate SliderTemplate;

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

        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnInit(e);
            Sitecore.Context.State.DataBind = false;
            this.Users.ItemDataBound += new Grid.ItemDataBoundEventHandler(this.Users_ItemDataBound);
            this.Users.ItemContentCreated += new Grid.ItemContentCreatedEventHandler(this.UsersItemContentCreated);
            Client.AjaxScriptManager.OnExecute += new AjaxScriptManager.ExecuteDelegate(Current_OnExecute);
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

        private static void Current_OnExecute(object sender, AjaxCommandEventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (args.Name == "usermanager:userdeleted")
            {
                SheerResponse.Eval("refresh()");
            }
            else
            {
                if (!(args.Name == "usermanager:refresh"))
                    return;
                SheerResponse.Eval("refresh()");
            }
        }

        private void WriteLanguageAndBrowserCssClass() => this.Form.Attributes["class"] = string.Format("{0} {1}", (object)UIUtil.GetBrowserClassString(), (object)UIUtil.GetLanguageCssClassString());

        private void UsersItemContentCreated(object sender, GridItemContentCreatedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)e, nameof(e));
            if (e.Content.FindControl("FullNameLabel") is Label control1)
                control1.Text = HttpUtility.HtmlEncode(e.Item[(object)"Profile.FullName"].ToString());
            if (!(e.Content.FindControl("CommentLabel") is Label control2))
                return;
            string str = HttpUtility.HtmlEncode(e.Item[(object)"Profile.Comment"] == null ? string.Empty : e.Item[(object)"Profile.Comment"].ToString());
            control2.Text = str;
        }

        private void Users_ItemDataBound(object sender, GridItemDataBoundEventArgs e)
        {
            User dataItem = e.DataItem as User;
            Assert.IsNotNull((object)dataItem, "user");
            e.Item[(object)"Profile.PortraitFullPath"] = (object)WebUtil.SafeEncode(Themes.MapTheme(dataItem.Profile.Portrait));
        }
    }
}
