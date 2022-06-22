using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SimpleChatAppLibrary; 

public class TrustedChatUsers {

    internal List<KeyValuePair<string, string>> TrustedUsers; // <username, public key>
    private SimpleChatAppClient _client;

    internal TrustedChatUsers(SimpleChatAppClient cl) {
        _client = cl;
    }

    /// <summary>
    /// Adds the author of the specified message to the list of trusted users.
    /// </summary>
    /// <param name="msg">The message to obtain the author information from.</param>
    public void AddVerifiedUser(SimpleChatAppMessage msg) {
        TrustedUsers.Add(new KeyValuePair<string, string>(msg.creatorName, msg.publicKey));
        _client.Prefs.SetString("trusted_users", JsonSerializer.Serialize(TrustedUsers));
        _client.Prefs.Save();
    }

    /// <summary>
    /// Checks if the author of a message is verified.
    /// </summary>
    /// <param name="msg">The message to obtain the author information from.</param>
    /// <returns>Whether the client can verify the message.</returns>
    public bool CheckUser(SimpleChatAppMessage msg) {
        try {
            return CheckUserUnsafe(msg);
        }
        catch (Exception) {
            return false;
        }
    }

    /// <summary>
    /// Checks if the author of a message is verified.
    /// </summary>
    /// <param name="msg">The message to obtain the author information from.</param>
    /// <returns>Whether the client can verify the message.</returns>
    /// <exception cref="ArgumentNullException">The saved public key was invalid.</exception>
    /// <exception cref="CryptographicException">The saved public key was corrupted.</exception>
    public bool CheckUserUnsafe(SimpleChatAppMessage msg) {
        
        IEnumerable<string> keysForUser = TrustedUsers
            .Where(key => key.Key == msg.creatorName)
            .Select(key => key.Value);
        
        if (!keysForUser.Any()) {
            return false;
        }

        DSACryptoServiceProvider verifier = new();
        foreach (string key in keysForUser) {
            if (key != msg.publicKey) {
                continue;
            }
            
            // Verify the signature
            verifier.FromXmlString(key);
            byte[] textData = Encoding.UTF8.GetBytes(msg.text);
            byte[] signature = Convert.FromBase64String(msg.signature);
            if (verifier.VerifyData(textData, signature)) {
                return true;
            }
            
        }

        return false;
    }
    
}