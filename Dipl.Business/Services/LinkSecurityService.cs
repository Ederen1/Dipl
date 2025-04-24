using System.Security.Cryptography;
using Dipl.Business.Entities;
using Konscious.Security.Cryptography;

namespace Dipl.Business.Services;

public static class LinkSecurityService
{
    public static async Task SetupSecureLinkAsync(string password, BaseLink link)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);  

        // Derive verifier (light cost)
        var argon = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 1,
            MemorySize = 64 * 1024, 
            Iterations = 2
        };
        var verifier = await argon.GetBytesAsync(32);

        link.Salt = salt;
        link.VerifierHash = verifier;
    }

    public static async Task<Stream> EncryptDataAsync(BaseLink link, string password, Stream target)
    {
        if (link.Salt is null || link.VerifierHash is null)
            throw new Exception("Link security parameters are not specified properly");
        
        var argonFull = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = link.Salt,
            DegreeOfParallelism = 4,
            MemorySize = 256 * 1024, 
            Iterations = 4
        };
        var key = await argonFull.GetBytesAsync(32);
        
        using var aes = Aes.Create();
        aes.Key = key;

        if (link.AesIV is not null)
            aes.IV = link.AesIV;
        else
        {
            aes.GenerateIV();
            link.AesIV = aes.IV;
        }
        
        var transform = aes.CreateEncryptor();
        return new CryptoStream(target, transform, CryptoStreamMode.Read);
    }

    public static async Task<Stream> DecryptDataAsync(BaseLink link, string password, Stream data)
    {
        if (link.AesIV is null || link.Salt is null || link.VerifierHash is null)
            throw new Exception("Link security parameters are not specified properly");

        if (password is null)
            throw new Exception("Password is null");
        
        if(!await PasswordMatchesLink(link, password))
            throw new CryptographicException("Invalid password.");  

        var argon = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = link.Salt,
            DegreeOfParallelism = 4, 
            MemorySize = 256 * 1024,
            Iterations = 4
        };
        var key = await argon.GetBytesAsync(32);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = link.AesIV;

        var transform = aes.CreateDecryptor();
        return new CryptoStream(data, transform, CryptoStreamMode.Read);
    }

    public static async Task<bool> PasswordMatchesLink(BaseLink link, string? password)
    {
        if (password is null)
            return false;
        
        var verifierCheck = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = link.Salt,
            DegreeOfParallelism =1 ,
            MemorySize = 64 * 1024,
            Iterations = 2
        };
        var expected = await verifierCheck.GetBytesAsync(32);

        return CryptographicOperations.FixedTimeEquals(expected, link.VerifierHash);
    }
}