using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

class MyTcpListener
{
    public static string GetKeyString(RSAParameters publicKey)
    {

        var stringWriter = new System.IO.StringWriter();
        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        xmlSerializer.Serialize(stringWriter, publicKey);
        return stringWriter.ToString();
    }

    public static string Encrypt(string textToEncrypt, string publicKeyString)
    {
        var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            try
            {
                rsa.FromXmlString(publicKeyString.ToString());
                var encryptedData = rsa.Encrypt(bytesToEncrypt, true);
                var base64Encrypted = Convert.ToBase64String(encryptedData);
                return base64Encrypted;
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    public static string Decrypt(string textToDecrypt, string privateKeyString)
    {
        var bytesToDescrypt = Encoding.UTF8.GetBytes(textToDecrypt);

        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            try
            {

                // server decrypting data with private key                    
                rsa.FromXmlString(privateKeyString);

                var resultBytes = Convert.FromBase64String(textToDecrypt);
                var decryptedBytes = rsa.Decrypt(resultBytes, true);
                var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData.ToString();
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }


    public static void Main()
    {
        TcpListener server = null;
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    // Process the data sent by the client.

                }
                var cryptoServiceProvider = new RSACryptoServiceProvider(2048); //2048 - Długość klucza
                var privateKey = cryptoServiceProvider.ExportParameters(true); //Generowanie klucza prywatnego
                var publicKey = cryptoServiceProvider.ExportParameters(false); //Generowanie klucza publiczny

                string publicKeyString = GetKeyString(publicKey);
                string privateKeyString = GetKeyString(privateKey);

                Console.WriteLine("KLUCZ PUBLICZNY: ");
                Console.WriteLine(publicKeyString);
                Console.WriteLine("-------------------------------------------");


                Console.WriteLine("KLUCZ PRYWATNY: ");
                Console.WriteLine(privateKeyString);
                Console.WriteLine("-------------------------------------------");


                string textToEncrypt = data;
                Console.WriteLine("TEKST DO ZASZYFROWANIA: ");
                Console.WriteLine(textToEncrypt);
                Console.WriteLine("-------------------------------------------");

                string encryptedText = Encrypt(textToEncrypt, publicKeyString); //Szyfrowanie za pomocą klucza publicznego
                Console.WriteLine("ZASZYFROWANY TEXT: ");
                Console.WriteLine(encryptedText);
                Console.WriteLine("-------------------------------------------");

                string decryptedText = Decrypt(encryptedText, privateKeyString); //Odszyfrowywanie za pomocą klucza prywatnego

                Console.WriteLine("ODSZYFROWANY TEXT: ");
                Console.WriteLine(decryptedText);

                data = publicKeyString;

                //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("abc");

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", "abc");



                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }
}