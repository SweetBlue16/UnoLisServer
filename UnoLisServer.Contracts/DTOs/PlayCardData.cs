namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Represents the data required to play a card in a lobby-based game session.
    /// </summary>
    public class PlayCardData
    {
        public string LobbyCode { get; set; }
        public string Nickname { get; set; }
        public string CardId { get; set; }
        public int? SelectedColorId { get; set; }
    }
}
