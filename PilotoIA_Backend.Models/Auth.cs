namespace PilotoIA_Backend.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public MensajeRespuesta Result { get; set; } = new MensajeRespuesta();
    }

    public class ValidateTokenResponse
    {
        public bool IsValid { get; set; }
        public MensajeRespuesta Result { get; set; } = new MensajeRespuesta();
    }
}