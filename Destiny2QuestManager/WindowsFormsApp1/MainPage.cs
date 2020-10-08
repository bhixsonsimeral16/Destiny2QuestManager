using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainPage : Form
    {
        static string apiKey = "e31cb56a1794452bb3c2b155dd06d19d";
        static string authorizationEndpoint = "https://www.bungie.net/en/OAuth/Authorize";
        static string clientID = "34117";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            //Process.Start(new ProcessStartInfo("https://www.microsoft.com") { UseShellExecute = true });
            //Process.Start("explorer.exe", "https://www.microsoft.com");
            //// Uses JSON.NET - http://www.nuget.org/packages/Newtonsoft.Json
            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

            //    var response = await client.GetAsync("https://www.bungie.net/platform/Destiny/Manifest/InventoryItem/1274330687/");
            //    var content = await response.Content.ReadAsStringAsync();
            //    dynamic item = JsonConvert.DeserializeObject(content);

            //    label1.Text = (item.Response.data.inventoryItem.itemName); //Gjallarhorn
            //}

            // Generates state and PKCE values.
            string state = WebHelper.randomDataBase64url(32);
            string code_verifier = WebHelper.randomDataBase64url(32);
            string code_challenge = WebHelper.base64urlEncodeNoPadding(WebHelper.sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("https://localhost:55275/");
            //String[] prefixes = { redirectURI, string.Format("http://localhost:{0}/", WebHelper.GetRandomUnusedPort()) };
            output("redirect URI: " + redirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            //foreach(string s in prefixes)
            //{
            //    http.Prefixes.Add(s);
            //}
            http.Prefixes.Add(redirectURI);
            output("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            // Using ShellExecute property to force browser rather than file explorer
            // ref: https://stackoverflow.com/questions/21835891/process-starturl-fails
            Process.Start(new ProcessStartInfo(authorizationRequest) { UseShellExecute = true });

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings this app back to the foreground.
            this.Activate();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null || context.Request.QueryString.Get("state") == null)
            {
                output("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // Extracts the code.
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                output(String.Format("Recieved request with invalid state ({0})", incoming_state));
                return;
            }
            output("Authorization code: " + code);
        }

        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public void output(string output)
        {
            label1.Text = label1.Text + output + Environment.NewLine;
            Console.WriteLine(output);
        }
    }
}
