namespace BookingSystem.Application.Common.Abstractions;

public interface IPasswordHasher
{
    byte[] HashPassword(string password);
    bool VerifyPassword(string password, byte[] hashedPassword);
}