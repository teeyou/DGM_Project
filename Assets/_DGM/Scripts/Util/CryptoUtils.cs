using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class CryptoUtils
{
    // RSA로 AES 키 암호화
    public static byte[] EncryptAESKey(byte[] aesKey, RSAParameters publicKey)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportParameters(publicKey);
            return rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
        }
    }

    // RSA로 AES 키 복호화
    public static byte[] DecryptAESKey(byte[] encryptedKey, RSAParameters privateKey)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportParameters(privateKey);
            return rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
        }
    }

    // AES로 파일 암호화
    public static void EncryptFile(string inputPath, string outputPath)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = AESConfig.Key;
            aes.IV = AESConfig.IV;

            using (var fsInput = new FileStream(inputPath, FileMode.Open))
            using (var fsOutput = new FileStream(outputPath, FileMode.Create))
            using (var cs = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                fsInput.CopyTo(cs);
            }
        }
    }

    // AES로 파일 복호화 → MemoryStream 반환
    public static MemoryStream DecryptFileToMemory(string inputPath, byte[] aesKey, byte[] aesIV)
    {
        //using (var aes = Aes.Create())
        //{
        //    aes.Key = aesKey;
        //    aes.IV = aesIV;

        //    using (var fsInput = new FileStream(inputPath, FileMode.Open))
        //    using (var msOutput = new MemoryStream())
        //    using (var cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
        //    {
        //        cs.CopyTo(msOutput);
        //        msOutput.Position = 0;
        //        return msOutput;
        //    }
        //}
        var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIV;

        var fsInput = new FileStream(inputPath, FileMode.Open);
        var msOutput = new MemoryStream();
        using (var cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
        {
            cs.CopyTo(msOutput);
        }
        msOutput.Position = 0;
        return msOutput; // 여기서는 Dispose하지 않고 반환
    }

    // 랜덤생성 대신, AESConfig에서 키와 IV를 가져옴
    public static void GenerateAESKeyAndIV(string keyPath, string ivPath)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256; // 256비트 키
            aes.GenerateKey();
            aes.GenerateIV();

            File.WriteAllBytes(keyPath, aes.Key); // 키 저장
            File.WriteAllBytes(ivPath, aes.IV);   // IV 저장
        }
    }
}