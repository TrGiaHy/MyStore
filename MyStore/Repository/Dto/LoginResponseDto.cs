namespace Repository.Dto
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Username { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
