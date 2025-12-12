namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Context for playing a card in the game
    /// </summary>
    public class PlayCardContext
    {
        public string LobbyCode { get; set; }
        public string Nickname { get; set; }
        public string CardId { get; set; }
        public int? SelectedColorId { get; set; }

        public PlayCardContext(string lobbyCode, string nickname, string cardId, int? colorId)
        {
            LobbyCode = lobbyCode;
            Nickname = nickname;
            CardId = cardId;
            SelectedColorId = colorId;
        }
    }
}