using System.Security.Cryptography;
using BookingSystem.Application.Common.Abstractions;
using Konscious.Security.Cryptography;

namespace BookingSystem.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public byte[] HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        return [.. salt, .. HashPassword(password, salt)];
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        using var a = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password));
        a.Salt = salt;
        a.DegreeOfParallelism = 4;
        a.Iterations = 4;
        a.MemorySize = 1024 * 64; // Memory size in KB
        return a.GetBytes(128);
    }

    public bool VerifyPassword(string password, byte[] hashedPassword)
    {
        return HashPassword(password, hashedPassword.Take(16).ToArray()).SequenceEqual(hashedPassword.AsSpan(16));
    }
}
