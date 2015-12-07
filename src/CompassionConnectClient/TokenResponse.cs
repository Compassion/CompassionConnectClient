namespace CompassionConnectClient
{
    internal class TokenResponse
    {
        public string AccessToken { get; set; }

        public int ExpiresIn { get; set; }

        public string TokenType { get; set; }
    }
}