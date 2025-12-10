namespace UnoLisServer.Contracts.DTOs
{
    public class PlayCardData
    {
        public string LobbyCode { get; set; }
        public string Nickname { get; set; }
        public string CardId { get; set; }
        public int? SelectedColorId { get; set; }
    }
}
