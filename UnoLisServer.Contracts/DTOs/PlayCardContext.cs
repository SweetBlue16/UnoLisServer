using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.DTOs // <--- Faltaba esto
{
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