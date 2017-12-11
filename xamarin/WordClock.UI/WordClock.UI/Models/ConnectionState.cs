namespace WordClock.UI.Models
{
    public class ConnectionState
    {
        public ConnectionState(StateType type, string message)
        {
            Type = type;
            Message = message;
        }

        public StateType Type { get; }
        public string Message { get; }

        public static ConnectionState Connected() =>
            new ConnectionState(StateType.Connected, "Connected to Clock");
        
        public static ConnectionState Connecting(string ipAddress) =>
            new ConnectionState(StateType.Connecting, $"Connecting to {ipAddress}");
        
        public static ConnectionState Failed(string message) =>
            new ConnectionState(StateType.Failed, $"Connection failed:\n{message}");
    }
}