using System.Security.Cryptography;
using Dipl.Business.Entities;
using Dipl.Common.Util;

namespace Dipl.Business.Services;

public static class LinkSecurityService
{
    private const int Iterations = 600_000;

    public static void SetupSecureLinkAsync(string password, BaseLink link)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);  

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var verifier = pbkdf2.GetBytes(32);

        link.VerifierSalt = salt;
        link.VerifierHash = verifier;
    }

    public static Stream EncryptDataAsync(BaseLink link, string password, Stream target)
    {
        if (link.VerifierSalt is null || link.VerifierHash is null)
            throw new Exception("Link security parameters are not specified properly");

        var salt = new byte[16];
        if (link.Salt is not null)
            salt = link.Salt;
        else
        {
            RandomNumberGenerator.Fill(salt);
            link.Salt = salt;
        }
        
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(32);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV);
        memoryStream.Seek(0, SeekOrigin.Begin);
        
        var transform = aes.CreateEncryptor();
        var cryptoStream = new CryptoStream(target, transform, CryptoStreamMode.Read);
        
        return new MultiStream(memoryStream, cryptoStream);
    }

    public static Stream DecryptDataAsync(BaseLink link, string password, Stream data)
    {
        if (link.VerifierSalt is null || link.VerifierHash is null || link.Salt is null)
            throw new Exception("Link security parameters are not specified properly");

        if (password is null)
            throw new Exception("Password is null");
        

        using var pbkdf2 = new Rfc2898DeriveBytes(password, link.Salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(32);

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[16];
        data.ReadExactly(iv);
        aes.IV = iv;

        var transform = aes.CreateDecryptor();
        return new CryptoStream(data, transform, CryptoStreamMode.Read);
    }

    public static bool PasswordMatchesLink(BaseLink link, string? password)
    {
        if (password is null)
            return false;

        if (link.VerifierSalt is null)
            throw new Exception("Link is in invalid state for checking password");
        
        using var pbkdf2 = new Rfc2898DeriveBytes(password, link.VerifierSalt, Iterations, HashAlgorithmName.SHA256);
        var expected = pbkdf2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(expected, link.VerifierHash);
    }
}