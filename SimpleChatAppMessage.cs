namespace SimpleChatAppLibrary; 

public class SimpleChatAppMessage {
    
    /// <summary>
    /// The internal ID of the message.
    /// </summary>
    public string messageId { get; set; }
    
    /// <summary>
    /// The username of the sender.
    /// </summary>
    public string creatorName { get; set; }
    
    /// <summary>
    /// The message text.
    /// </summary>
    public string text { get; set; }
    
    /// <summary>
    /// The time the message was sent in binary time.
    /// </summary>
    public long createdAt { get; set; }
    
    /// <summary>
    /// The signature of the message.
    /// </summary>
    public string signature { get; set; }
    
    /// <summary>
    /// The public key of the sender.
    /// </summary>
    public string publicKey { get; set; }
    
    /// <summary>
    /// Gets the time the message was sent in a DateTime object (UTC timezone).
    /// </summary>
    /// <returns>The DateTime object representing when the message was sent.</returns>
    public DateTime GetCreatedTimeUtc() => DateTime.FromBinary(createdAt);
    
    /// <summary>
    /// Gets the time the message was sent in a DateTime object (local timezone).
    /// </summary>
    /// <returns>The DateTime object representing when the message was sent.</returns>
    public DateTime GetCreatedTimeLocal(DateTime time) => DateTime.FromBinary(createdAt).ToLocalTime();
    
    /// <summary>
    /// Gets the message ID as a Guid object
    /// </summary>
    /// <returns>The Guid Object</returns>
    public Guid GetMessageId() => Guid.Parse(messageId);
}