using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MoviesAPI.Dtos;

namespace MoviesAPI.Services;

public class HashService
{
    public HashResultDto Hash(string text)
    {
        var sal = new byte[16];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(sal);
        }

        return Hash(text, sal);
    }

    public HashResultDto Hash(string text, byte[] sal)
    {
        var keyDerivation = KeyDerivation.Pbkdf2(password: text,
            salt: sal,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 32
        );

        var hash = Convert.ToBase64String(keyDerivation);
        return new HashResultDto()
        {
            Hash = hash,
            Sal = sal
        };
    }
}
