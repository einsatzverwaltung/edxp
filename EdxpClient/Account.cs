using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EdxpClient
{
    [Command(Name = "account", Description = "Account Information")]
    [Subcommand(
        typeof(MyAccount),
        typeof(ListAccount),
        typeof(GetAccount))]
    class Account : CommandBase
    {
        public async Task<int> OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }

    [Command(Name = "my", Description = "Get Information about your own account")]
    class MyAccount : CommandBase
    {
        public int OnExecute(IConsole console)
        {
            var c = ClientFactory.Create(Verbose);

            return Helper.RunHttpCall(async () =>
            {
                var res = await c.GetAccountInfoAsync();

                Helper.DumpObjectToOutput(res, OutputFormat);
            });
        }
    }

    [Command(Name = "get", Description = "Get Information about your own account")]
    class GetAccount : CommandBase
    {
        [Argument(0, Description = "ID of the Account / Identitiy", Name = "Account ID")]
        public string AccountId { get; set; }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            var c = ClientFactory.Create(Verbose);
            Guid uuid;
            if (Guid.TryParse(AccountId, out uuid))
            {
                return Helper.RunHttpCall(async () =>
                {
                    var res = await c.GetAsync(uuid);

                    Helper.DumpObjectToOutput(res, OutputFormat);
                });
            }
            else
            {
                app.ShowHelp();
                return -1;
            }
        }
    }

    [Command(Name = "ls", Description = "List all known Accounts / Identities on this Server")]
    class ListAccount : CommandBase
    {
        public int OnExecute(IConsole console)
        {
            var c = ClientFactory.Create(Verbose);

            return Helper.RunHttpCall(async () =>
            {
                var res = await c.GetAccountListAsync();

                Helper.DumpObjectToOutput(res, OutputFormat);
            });
        }
    }

    [Command(Name = "api-keys", Description = "List all API Keys of your own account")]
    class ApiKeysAccount : CommandBase
    {
        public async Task<int> OnExecute(IConsole console)
        {
            var c = ClientFactory.Create(Verbose);

            //var res = await c.GetApiKeyListAsync();

            //Helper.DumpObjectToOutput(res, OutputFormat);

            return 0;
        }
    }
}
