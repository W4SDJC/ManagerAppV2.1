using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        byte[] data = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }
    public static string Decrypt(string encryptedText)
    {
        try {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(encryptedText);
            byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }catch(Exception ex) { return "error"; }
    }
}
