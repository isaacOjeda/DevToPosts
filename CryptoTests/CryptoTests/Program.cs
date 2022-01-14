
using System.Security.Cryptography;
using System.Text;

string original = "Here is some data to encrypt!";

using (var myAes = Aes.Create())
{
    //byte[] encrypted = EncryptStringToBytes(original, myAes.Key, myAes.IV);

    //string roundtrip = DecryptStringFromBytes(encrypted, myAes.Key, myAes.IV);

    var encrypted = EncryptBytes(Encoding.UTF8.GetBytes(original), myAes.Key, myAes.IV);
    var plainBytes = DecryptBytes(encrypted, myAes.Key, myAes.IV);
    var roundtrip = Encoding.UTF8.GetString(plainBytes);

    Console.WriteLine("Original:   {0}", original);
    Console.WriteLine("Encrypted:  {0}", Convert.ToBase64String(encrypted));
    Console.WriteLine("Round Trip: {0}", roundtrip);
    Console.WriteLine();
    Console.ReadLine();
}


byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
{
    byte[] encrypted;

    using (var aesAlg = Aes.Create())
    {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();
        using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (StreamWriter swEncrypt = new(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        encrypted = msEncrypt.ToArray();
    }

    return encrypted;
}

byte[] EncryptBytes(byte[] plainBytes, byte[] Key, byte[] IV)
{
    byte[] encrypted;

    using (var aesAlg = Aes.Create())
    {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();
        using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);

        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        csEncrypt.Close();
        csEncrypt.Flush();

        encrypted = msEncrypt.ToArray();
    }

    return encrypted;
}

byte[] DecryptBytes(byte[] encrypted, byte[] Key, byte[] IV)
{
    using (var aesAlg = Aes.Create())
    {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(encrypted);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using MemoryStream msDestination = new();

        csDecrypt.CopyTo(msDestination);

        return msDestination.ToArray();
    }
}

string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
{
    string? plaintext = null;

    using (var aesAlg = Aes.Create())
    {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(cipherText);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);

        plaintext = srDecrypt.ReadToEnd();
    }

    return plaintext;
}