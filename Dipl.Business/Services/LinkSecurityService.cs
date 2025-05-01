using System.Security.Cryptography;
using Dipl.Business.Entities;
using Dipl.Common.Util;
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
            DegreeOfParallelism = 4,
            MemorySize = 46 * 1024, 
            Iterations = 1
        };
        var verifier = await argon.GetBytesAsync(32);

        link.VerifierSalt = salt;
        link.VerifierHash = verifier;
    }

    public static async Task<Stream> EncryptDataAsync(BaseLink link, string password, Stream target)
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
        
        var argonFull = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            MemorySize = 46 * 1024, 
            Iterations = 1
        };
        var key = await argonFull.GetBytesAsync(32);
        
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

    public static async Task<Stream> DecryptDataAsync(BaseLink link, string password, Stream data)
    {
        if (link.VerifierSalt is null || link.VerifierHash is null)
            throw new Exception("Link security parameters are not specified properly");

        if (password is null)
            throw new Exception("Password is null");

        var argon = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = link.Salt,
            DegreeOfParallelism = 4,
            MemorySize = 46 * 1024, 
            Iterations = 1
        };
        var key = await argon.GetBytesAsync(32);

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[16];
        await data.ReadExactlyAsync(iv);
        aes.IV = iv;

        var transform = aes.CreateDecryptor();
        return new CryptoStream(data, transform, CryptoStreamMode.Read);
    }

    public static async Task<bool> PasswordMatchesLink(BaseLink link, string? password)
    {
        if (password is null)
            return false;
        
        var verifierCheck = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = link.VerifierSalt,
            DegreeOfParallelism = 4,
            MemorySize = 46 * 1024, 
            Iterations = 1
        };
        var expected = await verifierCheck.GetBytesAsync(32);

        return CryptographicOperations.FixedTimeEquals(expected, link.VerifierHash);
    }
}