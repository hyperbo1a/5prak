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
