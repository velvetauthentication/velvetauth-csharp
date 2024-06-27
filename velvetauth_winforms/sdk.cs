using System;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MyAppSDK
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://velvetauth.com/api/1.0";
    private readonly string _appId;
    private readonly string _secret;
    private readonly string _version;


    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime ExpiryDate { get; private set; }

    public string hwid = WindowsIdentity.GetCurrent().User.Value;


    public MyAppSDK(string appId, string secret, string version)
    {
        _httpClient = new HttpClient();
        _appId = appId;
        _secret = secret;
        _version = version;
    }

    private HttpResponseMessage Post(string endpoint, object data)
    {
        try
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            return _httpClient.PostAsync(_apiBaseUrl + endpoint, content).Result;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            return null;
        }
    }

    public bool Initialize()
    {
        try
        {
            var requestData = new
            {
                type = "init",
                app_id = _appId,
                secret = _secret,
                version = _version
            };

            var response = Post("index.php", requestData);

            if (response != null && response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

                if (jsonResponse != null && jsonResponse.error != null && jsonResponse.error.ToString() == "wrong_version")
                {
                    string downloadUrl = jsonResponse.download_url.ToString();

                    downloadUrl = downloadUrl.Replace("\\", "");

                    MessageBox.Show("Your using an out dated version of the program. Redirecting to update URL");

                    if (Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri uriResult))
                    {
                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = uriResult.ToString(),
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);

                        return false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid download URL format");
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                MessageBox.Show("Initialization failed: " + response?.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            return false;
        }
    }


    public bool RegisterLicense(string username, string password, string licenseKey, string email)
    {
        try
        {
            var requestData = new
            {
                type = "register",
                app_id = _appId,
                username = username,
                password = password,
                hwid = hwid,
                license_key = licenseKey,
                email = email
            };

            var response = Post("index.php", requestData);

            if (response != null && response.IsSuccessStatusCode)
            {
                var responseContentTask = response.Content.ReadAsStringAsync();
                responseContentTask.Wait();
                var responseContent = responseContentTask.Result.Trim();

                MessageBox.Show("Response Content: " + responseContent);

                JObject jsonResponse = null;
                try
                {
                    jsonResponse = JObject.Parse(responseContent);
                }
                catch (JsonReaderException ex)
                {
                    MessageBox.Show("Error parsing JSON: " + ex.Message);
                    MessageBox.Show("Registration failed: Invalid JSON format");
                    return false;
                }


                if (jsonResponse != null && jsonResponse["message"] != null && jsonResponse["message"].ToString() == "License registered successfully")
                {

                    if (jsonResponse["user"] != null)
                    {
                        JObject user = (JObject)jsonResponse["user"];
                        string userUsername = user["username"].Value<string>();
                        string userEmail = user["email"].Value<string>();
                        DateTime expiryDate = user["expiry_date"].Value<DateTime>();

                        

                        MessageBox.Show("License registered successfully");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Registration failed: User details not found in response");
                        return false;
                    }
                }
                else
                {
                    string errorMessage = jsonResponse != null ? jsonResponse["error"]?.ToString() : "Unknown error";
                    MessageBox.Show($"Registration failed: {errorMessage}");
                    return false;
                }
            }
            else
            {
                if (response != null)
                {
                    var responseContentTask = response.Content.ReadAsStringAsync();
                    responseContentTask.Wait();
                    var responseContent = responseContentTask.Result.Trim();

                    if (responseContent.Contains("Email is already in use"))
                    {
                        MessageBox.Show("Registration failed: Email is already in use");
                    }
                    else if (responseContent.Contains("Username is already in use"))
                    {
                        MessageBox.Show("Registration failed: Username is already in use");
                    }
                    else if (responseContent.Contains("License is already used"))
                    {
                        MessageBox.Show("Registration failed: License is already used");
                    }
                    else
                    {
                        MessageBox.Show($"Registration failed: {responseContent}");
                    }
                }
                else
                {
                    MessageBox.Show("Registration failed: No response from server");
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            MessageBox.Show("Error occurred during registration");
            return false;
        }
    }

    public bool Login(string username, string password)
    {
        try
        {
            var requestData = new
            {
                type = "login",
                app_id = _appId,
                hwid = hwid,
                username = username,
                password = password
            };

            var response = Post("index.php", requestData);

            if (response != null && response.IsSuccessStatusCode)
            {
                var responseContentTask = response.Content.ReadAsStringAsync();
                responseContentTask.Wait();
                var responseContent = responseContentTask.Result.Trim();

                MessageBox.Show("Response Content: " + responseContent);

                JObject jsonResponse = null;
                try
                {
                    jsonResponse = JObject.Parse(responseContent);
                }
                catch (JsonReaderException ex)
                {
                    MessageBox.Show("Error parsing JSON: " + ex.Message);
                    MessageBox.Show("Login failed: Invalid JSON format");
                    return false;
                }

                if (jsonResponse != null && jsonResponse["message"] != null && jsonResponse["message"].ToString() == "Login successful")
                {

                    if (jsonResponse["user"] != null)
                    {
                        JObject user = (JObject)jsonResponse["user"];
                        int userId = user["id"].Value<int>();
                        string userUsername = user["username"].Value<string>();
                        string userEmail = user["email"].Value<string>();
                        DateTime expiryDate = user["expiry_date"].Value<DateTime>();


                        Username = userUsername;
                        Email = userEmail;
                        ExpiryDate = expiryDate;

                     

                        MessageBox.Show("Login successful");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Login failed: User details not found in response");
                        return false;
                    }
                }
                else
                {
                    string errorMessage = jsonResponse != null ? jsonResponse["error"]?.ToString() : "Unknown error";
                    MessageBox.Show($"Login failed: {errorMessage}");
                    return false;
                }
            }
            else
            {
                if (response != null)
                {
                    var responseContentTask = response.Content.ReadAsStringAsync();
                    responseContentTask.Wait();
                    var responseContent = responseContentTask.Result.Trim();

                    MessageBox.Show($"Login failed: {responseContent}");
                }
                else
                {
                    MessageBox.Show("Login failed: No response from server");
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            MessageBox.Show("Error occurred during login");
            return false;
        }
    }

    public bool ExtendExpiry(string username, string licenseKey)
    {
        try
        {
            var requestData = new
            {
                type = "extend_expiry",
                app_id = _appId,
                username = username,
                license_key = licenseKey
            };

            var response = Post("index.php", requestData);

            if (response != null && response.IsSuccessStatusCode)
            {
                var responseContentTask = response.Content.ReadAsStringAsync();
                responseContentTask.Wait();
                var responseContent = responseContentTask.Result.Trim();

                MessageBox.Show("Response Content: " + responseContent);

                JObject jsonResponse = null;
                try
                {
                    jsonResponse = JObject.Parse(responseContent);
                }
                catch (JsonReaderException ex)
                {
                    MessageBox.Show("Error parsing JSON: " + ex.Message);
                    MessageBox.Show("Extend expiry failed: Invalid JSON format");
                    return false;
                }

                if (jsonResponse != null && jsonResponse["message"] != null && jsonResponse["message"].ToString() == "License expiry extended successfully")
                {
                    // Update local expiry date if available in response
                    if (jsonResponse["new_expiry_date"] != null)
                    {
                        ExpiryDate = jsonResponse["new_expiry_date"].Value<DateTime>();
                        MessageBox.Show($"Expiry Date extended to: {ExpiryDate}");
                    }

                    MessageBox.Show("License expiry extended successfully");
                    return true;
                }
                else
                {
                    string errorMessage = jsonResponse != null ? jsonResponse["error"]?.ToString() : "Unknown error";
                    MessageBox.Show($"Extend expiry failed: {errorMessage}");
                    return false;
                }
            }
            else
            {
                if (response != null)
                {
                    var responseContentTask = response.Content.ReadAsStringAsync();
                    responseContentTask.Wait();
                    var responseContent = responseContentTask.Result.Trim();

                    MessageBox.Show($"Extend expiry failed: {responseContent}");
                }
                else
                {
                    MessageBox.Show("Extend expiry failed: No response from server");
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            MessageBox.Show("Error occurred during extending expiry");
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
