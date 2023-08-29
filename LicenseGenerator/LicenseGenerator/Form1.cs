using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

using System.Windows.Forms;

namespace LicenseGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            labelValide.Hide();
            labelPasValide.Hide();
        }

        byte[] key = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // La clé doit être de 128, 192 ou 256 bits (16, 24 ou 32 bytes).
        byte[] iv = Encoding.UTF8.GetBytes("1234567890ABCDEF"); // Le vecteur d'initialisation (IV) doit être de 16 bytes.
        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            string plainText = dateTimePicker1.Value.ToString("yyyy-MM-dd");


            
            if (!textBoxProgram.Text.Equals(""))
                plainText += textBoxProgram.Text;
            if (!textBoxMac.Text.Equals(""))
                plainText += textBoxMac.Text;



            textBoxResult.Text = EncryptString(plainText, key, iv);

        }

        public string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                    }
                    byte[] encryptedBytes = ms.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }


        public string DecryptString(string cipherText, byte[] key, byte[] iv)
        {

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
              
                
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    
            
        
        public string myMACAdress()
        {
            string macAddress = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress = nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddress;
        }
        

    public string GetLicenseText()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.txt");

        if (File.Exists(filePath))
        {
               string result =  File.ReadAllText(filePath);
                return result;
        }

        return string.Empty; // Return an empty string if the file does not exist or cannot be accessed.
    }
     

    public void WriteLicenseText(string content)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.txt");

        File.WriteAllText(filePath, content);
    }
        
        public bool validateKey(string testedString)
        {
            string programName = textBoxProg.Text;
            string decryptedString = DecryptString(testedString, key, iv);
           
            // Extraire la partie de la clé correspondant à la date
            string dateString = decryptedString.Substring(0, 10);

            

                if (DateTime.TryParse(dateString, out DateTime date))
                {
                    if (date < DateTime.Now)
                    {
                        labelPasValide.Show();
                    return false;
                    }
                }

            if (decryptedString.Substring(10).Equals(programName) || decryptedString.Substring(10).Equals(programName + myMACAdress()))
            {
                labelValide.Show();
                return true;
            }

            labelPasValide.Show();
            return false;
        }
        

        private void button1_Click(object sender, EventArgs e)

        {
            labelValide.Hide();
            labelPasValide.Hide();
            if (!textBoxTest.Equals(""))
            WriteLicenseText(textBoxTest.Text);
            string testedString = GetLicenseText();
            validateKey(testedString);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBoxMac.Text = myMACAdress();

        }
    }
}
