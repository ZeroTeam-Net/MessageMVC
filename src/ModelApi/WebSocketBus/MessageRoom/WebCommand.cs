namespace BeetleX.Zeroteam.WebSocketBus
{
    public class WebCommand
    {
        public string Room { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }

        public string User { get; set; }

        public const string Enter = "enter";
        public const string Quit = "quit";
        public const string Talk = "talk";
        public const string Real = "real";
    }
}
