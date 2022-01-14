using System.Security.Cryptography;
using System.Text;


string original = "Here is some data to encrypt!";


using (var rsa = RSA.Create())
{


    var encrypted = EncryptBytes(Encoding.UTF8.GetBytes(original), rsa.ExportParameters(false));
    var plainBytes = DecryptBytes(encrypted, rsa.ExportParameters(true));
    var roundtrip = Encoding.UTF8.GetString(plainBytes);

    Console.WriteLine("Original:   {0}", original);
    Console.WriteLine("Encrypted:  {0}", Convert.ToBase64String(encrypted));
    Console.WriteLine("Round Trip: {0}", roundtrip);
    Console.WriteLine();
    Console.ReadLine();
}

byte[] EncryptBytes(byte[] DataToEncrypt, RSAParameters RSAKeyInfo)
{
    try
    {
        byte[] encryptedData;
        using (var rsa = RSA.Create())
        {

            rsa.ImportParameters(RSAKeyInfo);

            encryptedData = rsa.Encrypt(DataToEncrypt, RSAEncryptionPadding.OaepSHA512);
        }
        return encryptedData;
    }

    catch (CryptographicException e)
    {
        Console.WriteLine(e.Message);

        return null;
    }
}

byte[] DecryptBytes(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
{
    try
    {
        byte[] decryptedData;

        using (var rsa = RSA.Create())
        {

            rsa.ImportParameters(RSAKeyInfo);

            decryptedData = rsa.Decrypt(DataToDecrypt, RSAEncryptionPadding.OaepSHA512);
        }
        return decryptedData;
    }

    catch (CryptographicException e)
    {
        Console.WriteLine(e.ToString());

        return null;
    }
}