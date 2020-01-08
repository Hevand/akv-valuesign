using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ValueSigning
{
    class Program
    {

        //Azure ID key
        static readonly string ApplicationId = ConfigurationManager.AppSettings.Get("ApplicationId");
        static readonly string ApplicationSecret = ConfigurationManager.AppSettings.Get("ApplicationSecret");

        //Azure Key Vault
        static readonly string VaultUrl = ConfigurationManager.AppSettings.Get("VaultUrl");
        static readonly string Identifier = ConfigurationManager.AppSettings.Get("Identifier");
        static readonly string Version = ConfigurationManager.AppSettings.Get("Version");        
        static readonly string VaultKeyIdentifier = $"{VaultUrl}/keys/{Identifier}/{Version}";        

        //Signing algorithm
        static readonly string algorithm = "RS512"; //Using RS512 implies using SHA512 for digest. This is hardcoded.

        static void Main(string[] args)
        {
            //Get sensitive content
            Console.WriteLine("Please provide value:");
            var sensitiveContent = Console.ReadLine();

            //Calculate digest (hash) of sensitive content
            //NOTE THAT digest + signing algorithm must match.
            //https://docs.microsoft.com/en-us/dotnet/api/azure.security.keyvault.keys.cryptography.signaturealgorithm?view=azure-dotnet
            var hashClient = System.Security.Cryptography.SHA512.Create();
            var digest = hashClient.ComputeHash(Encoding.UTF8.GetBytes(sensitiveContent));
                       
            //Create KeyVaultClient and have AKV sign the digest
            KeyVaultClient client = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(GetAccessTokenAsync),
                new HttpClient());            
            var signature = client.SignAsync(VaultKeyIdentifier, algorithm, digest).Result;

            //Result
            Console.WriteLine("\nSignature calculated:");
            Console.WriteLine($"Input: {sensitiveContent}");
            Console.WriteLine($"Signature: {Convert.ToBase64String(signature.Result)}");

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }

        private static async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            //DEMO ONLY
            //Storing ApplicationId and Key in code is bad idea :)
            var appCredentials = new ClientCredential(ApplicationId, ApplicationSecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);

            var result = await context.AcquireTokenAsync(resource, appCredentials);

            return result.AccessToken;
        }
    }
}
