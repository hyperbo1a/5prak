using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;


namespace _4prak
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            P_komentaras.Visible = false;
            P_slaptazodis.Visible = false;
            P_URL.Visible = false;
            label2.Visible = false;
            label12.Visible = false;
            label13.Visible = false;
        }

        private static readonly string filePath = "slaptazodziai.txt";
        private static readonly string encryptionKey = "qYz0t0R4P1+y4a2R7U5L1Jcz4P8Q2D7L";

        private void NS_sukurti_Click(object sender, EventArgs e)
        {
            var name = NS_pav.Text;
            var password = NS_slaptazodis.Text;
            var url = NS_URL.Text;
            var comment = NS_komentaras.Text;
            SavePassword(name, password, url, comment);
            MessageBox.Show("Slaptazodis issaugotas.");
        }

        private void P_paieska_Click(object sender, EventArgs e)
        {
            var name = P_pavadinimas.Text;
            var entry = SearchPassword(name);
            if (entry != null)
            {
                P_slaptazodis.Text = DecryptString(entry.Split(':')[1], Key);
                P_URL.Text = entry.Split(':')[2];
                P_komentaras.Text = entry.Split(':')[3];
                MessageBox.Show("Slaptazodis rastas.");

            }
            else
            {
                MessageBox.Show("Slaptazodis nerastas.");
            }
        }

        private void P_parodyti_Click(object sender, EventArgs e)
        {
            P_komentaras.Visible = true;
            P_slaptazodis.Visible = true;
            P_URL.Visible = true;
            label2.Visible = true;
            label12.Visible = true;
            label13.Visible = true;
        }

        private void SA_naujinti_Click(object sender, EventArgs e)
        {
            var name = SA_pavadinimas.Text;
            var newPassword = SA_slaptazodis.Text;
            UpdatePassword(name, newPassword);
            MessageBox.Show("Slaptazodis atnaujintas.");
        }

        private void TS_trinti_Click(object sender, EventArgs e)
        {
            var name = TS_pavadinimas.Text;
            DeletePassword(name);
            MessageBox.Show("Slaptazodis istrintas.");
        }



        public static void SavePassword(string name, string password, string urlOrApp, string comment)
        {
            var encryptedPassword = EncryptString(password, encryptionKey);
            var entry = $"{name}:{encryptedPassword}:{urlOrApp}:{comment}";

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(entry);
            }
        }

        public static string SearchPassword(string name)
        {
            if (!File.Exists(filePath)) return null;
            var lines = File.ReadAllLines(filePath);
            return lines.FirstOrDefault(line => line.Split(':')[0] == name);
        }

        public static void UpdatePassword(string name, string newPassword)
        {
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath);
            var updatedLines = lines.Select(line =>
            {
                var parts = line.Split(':');
                if (parts[0] == name)
                {
                    var encryptedPassword = EncryptString(newPassword, encryptionKey);
                    return $"{parts[0]}:{encryptedPassword}:{parts[2]}:{parts[3]}";
                }
                return line;
            }).ToArray();

            File.WriteAllLines(filePath, updatedLines);
        }
        public static void DeletePassword(string name)
        {
            if (!File.Exists(filePath))
                return;
            var lines = File.ReadAllLines(filePath);
            var remainingLines = lines.Where(line => line.Split(':')[0] != name);
            File.WriteAllLines(filePath, remainingLines);
        }

        public static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static string DecryptString(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }



        public static void EncryptFile(string filePath, string key)
        {
            var text = File.ReadAllText(filePath);
            var encryptedText = EncryptString(text, key);
            File.WriteAllText(filePath, encryptedText);
        }

        public static void DecryptFile(string filePath, string key)
        {
            var encryptedText = File.ReadAllText(filePath);
            var decryptedText = DecryptString(encryptedText, key);
            File.WriteAllText(filePath, decryptedText);
        }

        private void Uzdaryti_Click(object sender, EventArgs e)
        {
            EncryptFile(Path, Key);
            Application.Exit();
        }

        public static string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+[]{}|;:,.<>?";
            char[] password = new char[length];
            byte[] randomBytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[randomBytes[i] % validChars.Length];
            }

            return new string(password);
        }

        private void sugeneruot_Click(object sender, EventArgs e)
        {
            int passwordLength = 12; 
            var randomPassword = GenerateRandomPassword(passwordLength);
            NS_slaptazodis.Text = randomPassword;
        }

        public static string Key
        {
            get { return encryptionKey; }
        }
        public static string Path
        {
            get { return filePath; }
        }
    }
}
