// Services/AuthResult.cs (veya Models klasöründe)
using System.Collections.Generic;

namespace MemoriseApp.Models // veya MemoriseApp.Models
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        public static AuthResult Success() => new AuthResult { Succeeded = true };
        public static AuthResult Failure(IEnumerable<string> errors) => new AuthResult { Succeeded = false, Errors = errors };
        public static AuthResult Failure(string error) => new AuthResult { Succeeded = false, Errors = new[] { error } };
    }
}