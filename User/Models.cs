using System.ComponentModel.DataAnnotations;

namespace BoardGameBackend.Models
{
    public class UserModel
    {
        public required  Guid Id {get; set;}

        public required string Username { get; set; }
        public required  string Password { get; set; }
    }

    public class UserModelDto
    {
        public required  Guid Id {get; set;}

        public required string Username { get; set; }
    }

    public class AuthenticateData {
        public required  UserModel User {get; set;} // Guid To tak jak UUID innaczej

        public required  string Token { get; set;}
    }

    public class AuthenticateDataDto {
        public required  UserModel User {get; set;} // Guid To tak jak UUID innaczej

        public required  string Token { get; set;}
    }


    public class RegisterUserDto
    {
        public required  string Username { get; set; }
        public required  string Password { get; set; }
    }

    public class LoginUserDto
    {
        public required  string Username { get; set; }
        public required  string Password { get; set; }
    }
    public class GameVersionCheckDto
    {
        public required int VersionId { get; set; }
    }
}