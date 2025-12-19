namespace EduMatch.BusinessLogicLayer.Settings
{
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public int AccessTokenMinutes { get; set; } = 120;
        public int RefreshTokenDays { get; set; } = 30;
    }
}
