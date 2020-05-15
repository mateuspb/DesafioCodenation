using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cipher
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string token = "8e046c4a92e8bcb9c25422866ca194da064a83f6";
            try {
                // parâmetros da solicitação do tipo GET para a URL
                HttpClient cliente = new HttpClient();
                string resultado = await cliente.GetStringAsync("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=" + token);
                //{ "numero_casas":1,"token":"8e046c4a92e8bcb9c25422866ca194da064a83f6","cifrado":"ju xpslt po nz nbdijof. volopxo","decifrado":"","resumo_criptografico":""}
                Console.WriteLine("entrada: " + resultado);

                // deserializa objeto para utilizar as variáveis
                var post = JsonConvert.DeserializeObject<Post>(resultado);

                // decifra o campo de texto cifrado utilizando o número de casas de deslocamento
                int numCar = 0, num = 0;
                string textoDec = "", textoCif = "";
                textoCif = post.cifrado.ToLower();
                numCar = textoCif.Length;
                for (int i = 0; i < numCar; i++)
                {
                    num = Convert.ToInt32(textoCif[i]);
                    if (num >= 97 && num <= 122)
                    {
                        num = num - post.numero_casas;
                        if (num < 97)
                        {
                            num = 123 - (97 - num);
                        }
                    }
                    textoDec += Convert.ToChar(num);
                }

                // atribui o valor ao campo da estrutura
                post.decifrado = textoDec;

                // efetua o cálculo do HASH usando o método SHA1 sobre o texto decifrado
                byte[] buffer = Encoding.Default.GetBytes(textoDec);
                SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
                string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "").ToLower();

                // atribui o valor ao campo da estrutura
                post.resumo_criptografico = hash;

                // serializa variáveis pra montar JSON e criar arquivo de resposta
                var sub = JsonConvert.SerializeObject(post);

                // grava arquivo com o JSON para enviar a URL
                File.WriteAllText("answer.json", sub.ToString());

                // cria objeto para fazer o envio do arquivo POST
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();

                // converte arquivo em estrutura byte
                byte[] imagebytearraystring = ImageFileToByteArray("answer.json");
                // seta os parâmetros
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Length), "answer", "answer.json");
                // seta a URL e faz o envio do arquivo
                HttpResponseMessage response = httpClient.PostAsync("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=" + token, form).Result;
                // fecha o objeto
                httpClient.Dispose();
                // le o retorno
                string sd = response.Content.ReadAsStringAsync().Result;

                // imprime o Retorno do POST
                Console.WriteLine("\nResultado: " + sd);

                // converte arquivo em Byte Array
                byte[] ImageFileToByteArray(string fullFilePath)
                {
                    FileStream fs = File.OpenRead(fullFilePath);
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                    fs.Close();
                    return bytes;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("erro main.");
                Console.WriteLine(E.Message);
            }
            Console.ReadLine();
        }
        public class Post
        {
            public int numero_casas { get; set; }
            public string token { get; set; }
            public string cifrado { get; set; }
            public string decifrado { get; set; }
            public string resumo_criptografico { get; set; }
        }
    }
}
