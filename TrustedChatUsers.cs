using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SimpleChatAppLibrary; 

public class TrustedChatUsers {

    internal Dictionary<string, string> TrustedUsers; // <username, public key>
    private SimpleChatAppClient _client;

    public TrustedChatUsers(SimpleChatAppClient cl) {
        _client = cl;
    }

    public void AddVerifiedUser(SimpleChatAppMessage msg) {
        TrustedUsers.Add(msg.creatorName, msg.publicKey);
        _client.Prefs.SetString("trusted_users", JsonSerializer.Serialize(TrustedUsers));
        _client.Prefs.Save();
    }

    public bool CheckUser(SimpleChatAppMessage msg) {
        
        // print all trusted
        foreach (KeyValuePair<string, string> kvp in TrustedUsers) {
            Console.WriteLine("{0} : {1}", kvp.Key, kvp.Value);
        }
        
        if (!TrustedUsers.ContainsKey(msg.creatorName)) {
            return false;
        }
        
        string publicKey = TrustedUsers[msg.creatorName];
        if (publicKey != msg.publicKey) {
            return false;
        }
        
        // Verify the signature
        DSACryptoServiceProvider verifier = new();
        verifier.FromXmlString(publicKey);
        byte[] textData = Encoding.UTF8.GetBytes(msg.text);
        byte[] signature = Convert.FromBase64String(msg.signature);
        return verifier.VerifyData(textData, signature);
    }
    
}