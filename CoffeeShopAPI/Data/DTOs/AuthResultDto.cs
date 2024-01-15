namespace CoffeeShopAPI.Data.DTOs
{
    public class AuthResultDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
