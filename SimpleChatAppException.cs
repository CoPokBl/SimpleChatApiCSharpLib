namespace SimpleChatAppLibrary; 

public class SimpleChatAppException : Exception {
    
    public SimpleChatAppException() { }
    public SimpleChatAppException(string message) : base(message) { }
    public SimpleChatAppException(string message, Exception innerException) : base(message, innerException) { }
    
}