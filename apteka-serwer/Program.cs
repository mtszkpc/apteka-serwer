using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

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
                    string[] subs = data.Split(";") ;
                    String data1 = null; //string do wysylania danych
                    
                    // Process the data sent by the client.
                    if (subs[0] == "1") //test polaczenia
                    {
                        data1 = "polaczono z baza danych";
                    }


                    else if (subs[0] == "2") //logowanie
                    {
                        if (subs[1].Equals("aa")&& subs[2].Equals("aa"))
                            data1 = "2;ok";
                        else
                        {
                            //poprawna rejestracja
                            data1 = "blad logowania";
                        }
                    }

                    else if (subs[0]=="3") //rejestracja
                    {
                        //zajety login
                        if (subs[3].Equals("a"))
                            data1 = "login zajety";
                        else
                        {
                            //poprawna rejestracja
                            data1 = "3;ok";
                        }
                    }
                    else if (subs[0] == "m") //apteki
                    {
                        //zajety login
                        if (subs[2].Equals("aaa"))
                            data1 = "lek;abc;def;ghi";
                        else
                        {
                            //poprawna rejestracja
                            data1 = "brak leku";
                        }
                    }

                    else //koncowy blad, nieobslugiwany komunikat
                    {
                        data1 = "blad logowania";
                    }

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data1);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", data1);
                }

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