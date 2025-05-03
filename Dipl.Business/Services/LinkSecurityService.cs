using System.Security.Cryptography;
using System.Text;
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

        var verifier = await DeriveKey(salt, password);

        link.VerifierSalt = salt;
        link.VerifierHash = verifier;
    }

    public static async Task<Stream> EncryptDataAsync(BaseLink link, string password, Stream target)
    {
        if (link.VerifierSalt is null || link.VerifierHash is null)
            throw new Exception("Link security parameters are not specified properly");

        var salt = new byte[16];
        if (link.Salt is not null)
        {
            salt = link.Salt;
        }
        else
        {
            RandomNumberGenerator.Fill(salt);
            link.Salt = salt;
        }

        var key = await DeriveKey(salt, password);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var transform = aes.CreateEncryptor();
        var cryptoStream = new CryptoStream(target, transform, CryptoStreamMode.Read, true);
        return new MultiStream(memoryStream, cryptoStream);
    }

    public static async Task<Stream> DecryptDataAsync(BaseLink link, string password, Stream data)
    {
        if (link.VerifierSalt is null || link.VerifierHash is null || link.Salt is null)
            throw new Exception("Link security parameters are not specified properly");

        if (string.IsNullOrEmpty(password))
            throw new Exception("Password is not set");

        var key = await DeriveKey(link.Salt, password);
        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[16];
        await data.ReadExactlyAsync(iv);
        aes.IV = iv;

        var transform = aes.CreateDecryptor();
        return new CryptoStream(data, transform, CryptoStreamMode.Read, true);
    }

    private static async Task<byte[]> DeriveKey(byte[] salt, string password)
    {
        var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            MemorySize = 19 * 1024,
            Iterations = 2
        };

        return await argon.GetBytesAsync(32);
    }

    public static async Task<bool> PasswordMatchesLink(BaseLink link, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var expected = await DeriveKey(link.VerifierSalt!, password);
        return CryptographicOperations.FixedTimeEquals(expected, link.VerifierHash);
    }
}