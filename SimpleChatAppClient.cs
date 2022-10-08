using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SimpleChatAppLibrary;

public class SimpleChatAppClient {

    /// <summary>
    /// Current username.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The IP that requests will get sent to.
    /// </summary>
    public string Ip { get; }
    
    /// <summary>
    /// The IP that requests will get sent to. (Deprecated)
    /// </summary>
    [Obsolete("Use Ip instead.")]
    // ReSharper disable once InconsistentNaming
    public string IP => Ip;

    /// <summary>
    /// The channel that the client is currently in.
    /// </summary>
    public string Channel { get; }
    
    /// <summary>
    /// Object that allows control over trusted users.
    /// </summary>
    public TrustedChatUsers TrustedUsers { get; }

    /// <summary>
    /// Application preferences.
    /// </summary>
    internal SimpleChatLibPrefs Prefs { get; }

    /// <summary>
    /// The client's private signing key.
    /// </summary>
    private string privateKey { get; }
    
    /// <summary>
    /// The client's public signing key.
    /// </summary>
    public string publicKey { get; }

    /// <summary>
    /// The password to use when communicating with the server
    /// </summary>
    private string? Password { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleChatAppClient"/> class.
    /// </summary>
    /// <param name="ip">The server IP to connect to, must be in the format: *protocol*://*ip*:*port*</param>
    /// <param name="name">The username to use when sending messages.</param>
    /// <param name="channel">The channel to send and receive messages to and from.</param>
    /// <param name="password">The password to use to connect to the server, null for no password.</param>
    public SimpleChatAppClient(string ip, string name, string channel, string? password = null) {
        Name = name;
        Ip = ip;
        Channel = channel;
        TrustedUsers = new TrustedChatUsers(this);
        Prefs = new SimpleChatLibPrefs();
        Password = password;
        
        // Load trusted users from file.
        List<KeyValuePair<string, string>>? trust = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(
            Prefs.GetString("trusted_users", "[]"));
        if (trust == null) {
            throw new Exception("Invalid trusted users value in config");
        }
        TrustedUsers.TrustedUsers = trust;
        
        // Try load private key from file.
        string? privateKeyG = Prefs.GetString("private_key");

        // If private key is not found, generate a new one.
        if (privateKeyG == null) {
            DSACryptoServiceProvider signer = new();
            privateKey = signer.ToXmlString(true);
            Prefs.SetString("private_key", privateKey);
            publicKey = signer.ToXmlString(false);
            Prefs.Save();
        }
        else {
            DSACryptoServiceProvider signer = new();
            signer.FromXmlString(privateKeyG);
            privateKey = privateKeyG;
            publicKey = signer.ToXmlString(false);
        }
    }

    /// <summary>
    /// Attempt to communicate with the server.
    /// </summary>
    /// <param name="exception">The exception that was thrown attempting to communicate.</param>
    /// <returns>Whether the communication was successful, if false you should try a different IP.</returns>
    public bool TestConnection(out Exception? exception) {
        try {
            Requests.SendHttpRequest("GET", Ip);
        }
        catch (Exception e) {
            exception = e;
            return false;
        }
        exception = null;
        return true;
    }

    /// <summary>
    /// Attempt to communicate with the server.
    /// </summary>
    /// <returns>Whether the communication was successful, if false you should try a different IP.</returns>
    public bool TestConnection() => TestConnection(out _);

    /// <summary>
    /// Send a message to the connected channel.
    /// </summary>
    /// <param name="message">The text to send.</param>
    public SimpleChatAppMessage SendMessage(string message) {
        DSACryptoServiceProvider MySigner = new DSACryptoServiceProvider();
        MySigner.FromXmlString(privateKey);
        string resp = Requests.SendHttpRequest("POST", $"{Ip}/messages/{Channel}", new Dictionary<string, object> {
            { "text", message },
            { "creatorName", Name },
            { "publicKey", publicKey },
            { "signature", Convert.ToBase64String(MySigner.SignData(Encoding.UTF8.GetBytes(message))) }
        }, Password);
        return JsonSerializer.Deserialize<SimpleChatAppMessage>(resp)!;
    }

    /// <summary>
    /// Gets the specified number of messages from the connected channel.
    /// </summary>
    /// <param name="amount">The number of messages to get.</param>
    /// <param name="offset">The amount to offset the messages by.
    /// For example a value of 5 would skip 5 messages and get the next amount that was specified.</param>
    /// <param name="appearOnline">Whether or not to have the user appear in the online users list.</param>
    /// <returns>A list of the requested messages</returns>
    /// <exception cref="SimpleChatAppException">Will be thrown if the server responds with a null response.</exception>
    public IEnumerable<SimpleChatAppMessage> GetMessages(int amount = 10, int offset = 0, bool appearOnline = true) {
        string url = $"{Ip}/messages/{Channel}?limit={amount}&offset={offset}";
        if (appearOnline) {
          url += $"&name={Name}";
        }
        string response = Requests.SendHttpRequest("GET", url, Password);
        SimpleChatAppMessage[]? messages = JsonSerializer.Deserialize<SimpleChatAppMessage[]>(response);
        if (messages == null) throw new SimpleChatAppException("Failed to get messages (null response)");
        return messages.Reverse();
    }

    /// <summary>
    /// Gets a list of online users in the connected channel.
    /// </summary>
    /// <returns>The requested list of users</returns>
    /// <exception cref="SimpleChatAppException">Will be thrown if the server responds with a null response.</exception>
    public IEnumerable<string> GetOnlineUsers() {
      string response = Requests.SendHttpRequest("GET", $"{Ip}/messages/{Channel}/online", Password);
      string[]? online = JsonSerializer.Deserialize<string[]>(response);
      if (online == null) throw new SimpleChatAppException("Failed to get online (null response)");
      return online;
    }

}
