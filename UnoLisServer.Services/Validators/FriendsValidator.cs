namespace UnoLisServer.Services.Validators
{
    public static class FriendsValidator
    {
        public static void ValidateNicknames(string nick1, string nick2)
        {
            if (string.IsNullOrWhiteSpace(nick1) || string.IsNullOrWhiteSpace(nick2))
            {
                throw new System.ArgumentException("Nicknames can not be empty.");
            }
        }
    }
}