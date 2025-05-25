using System.Security.Cryptography;
using System.Text;
using Dipl.Business.Entities;
using Dipl.Common.Util;
using Konscious.Security.Cryptography;

namespace Dipl.Business.Services;

/// <summary>
///     Provides methods for securing links with password-based encryption and verification.
///     Uses Argon2id for key derivation and AES for encryption.
/// </summary>
public static class LinkSecurityService
{
    /// <summary>
    ///     Sets up the necessary cryptographic parameters (verifier salt and hash) on a link for password protection.
    ///     Applies both for UploadLinks and RequestLinks
    /// </summary>
    /// <param name="password">The password to protect the link with.</param>
    /// <param name="link">The link entity to secure.</param>
    public static async Task SetupSecureLinkAsync(string password, BaseLink link)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var verifier = await DeriveKey(salt, password);

        link.VerifierSalt = salt;
        link.VerifierHash = verifier;
    }

    /// <summary>
    ///     Encrypts the provided <paramref name="target" /> stream using a key derived from the link's salt and the provided
    ///     password.
    ///     The IV is prepended to the output stream.
    /// </summary>
    /// <param name="link">The link entity containing security parameters (VerifierSalt, VerifierHash, Salt).</param>
    /// <param name="password">The password to derive the encryption key from.</param>
    /// <param name="target">The stream to encrypt.</param>
    /// <returns>A stream containing the IV followed by the encrypted data.</returns>
    /// <exception cref="Exception">Thrown if link security parameters are not properly specified.</exception>
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

    /// <summary>
    ///     Decrypts the provided <paramref name="data" /> stream using a key derived from the link's salt and the provided
    ///     password.
    ///     It expects the IV to be prepended to the <paramref name="data" /> stream.
    /// </summary>
    /// <param name="link">The link entity containing security parameters (VerifierSalt, VerifierHash, Salt).</param>
    /// <param name="password">The password to derive the decryption key from.</param>
    /// <param name="data">The stream containing the IV followed by the encrypted data.</param>
    /// <returns>A stream containing the decrypted data.</returns>
    /// <exception cref="Exception">
    ///     Thrown if link security parameters are not properly specified or if the password is not
    ///     set.
    /// </exception>
    public static async Task<Stream> DecryptDataAsync(BaseLink link, string password, Stream data)
    {
        if (link.VerifierSalt is null || link.VerifierHash is null || link.Salt is null)
            throw new Exception("Security parameters are wrong");

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

    /// <summary>
    ///     Derives a 32-byte key from the given salt and password using Argon2id.
    /// </summary>
    /// <param name="salt">The salt to use for key derivation.</param>
    /// <param name="password">The password to use for key derivation.</param>
    /// <returns>A 32-byte derived key.</returns>
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

    /// <summary>
    ///     Verifies if the provided password matches the password used to secure the link.
    ///     It derives a key from the provided password and the link's VerifierSalt,
    ///     and then compares it in constant time to the link's VerifierHash.
    /// </summary>
    /// <param name="link">The link entity containing VerifierSalt and VerifierHash.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    public static async Task<bool> PasswordMatchesLink(BaseLink link, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var expected = await DeriveKey(link.VerifierSalt!, password);
        return CryptographicOperations.FixedTimeEquals(expected, link.VerifierHash);
    }
}