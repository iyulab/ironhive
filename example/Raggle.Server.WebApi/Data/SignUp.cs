namespace Raggle.Server.WebApi.Data;

public class SignUp
{
    public required string UserName { get; set; }

    public required string Password { get; set; }

    public required string ConfirmPassword { get; set; }
}
