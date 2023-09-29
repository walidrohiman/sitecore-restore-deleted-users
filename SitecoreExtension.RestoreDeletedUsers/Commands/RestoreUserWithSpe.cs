using Sitecore.Security.Accounts;
using Sitecore.Text;
using Spe.Commands.Security;
using Spe.Commands.Security.Accounts;
using Spe.Core.Validation;
using System.Management.Automation;
using SitecoreExtension.RestoreDeletedUsers.Utilities;
using System;

namespace SitecoreExtension.RestoreDeletedUsers.Commands
{
    [Cmdlet("Restore", "User")]
    [OutputType(new Type[] { typeof(User) })]
    public class RestoreUserWithSpe : BaseSecurityCommand
    {
        [Alias(new string[] { "Name" })]
        [Parameter(Mandatory = true, ParameterSetName = "Id", Position = 0, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        [AutocompleteSet("UserNames")]
        public AccountIdentity Identity { get; set; }

        protected override void ProcessRecord()
        {
            string str = this.Identity?.Name ?? string.Empty;

            if (!string.IsNullOrEmpty(str))
            {
                ListString listString = new ListString
                {
                    str
                };

                User user = User.FromName(str, true);

                var list = DatabaseFunctions.RestoreUsers(listString);

                if (list.Count > 0)
                {
                    this.WriteVerbose("Fail to restore user '" + user.Name + "'.");
                }
                else
                {
                    this.WriteVerbose("Restoring user '" + user.Name + "'.");
                }
            }
        }
    }
}
