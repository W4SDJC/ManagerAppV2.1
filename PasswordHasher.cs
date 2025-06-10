using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ManagerAppV2._1
{
    public static class PasswordHasher
    {
        // Размер соли (рекомендуется минимум 16 байт)
        private const int SaltSize = 16;
        // Размер хеша (для PBKDF2-SHA256 рекомендуется 32 байта)
        private const int HashSize = 32;
        // Количество итераций (чем больше - тем безопаснее, но медленнее)
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не должен быть пустым.", nameof(password));

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }


        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try {
                // Конвертируем обратно из base64
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Извлекаем соль
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Создаем хеш введенного пароля
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Сравниваем хеши
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                        return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка расшифровки пароля: " + ex.Message);
                return false;
            }
        }
    }
}
