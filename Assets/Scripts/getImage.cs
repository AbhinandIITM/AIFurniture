using System.Collections;
using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Networking;

public class GetImage : MonoBehaviour
{
    private static readonly HttpClient client = new HttpClient();
    public TextMeshProUGUI promptText;

    private string apiUrl = "https://modelslab.com/api/v6/realtime/text2img";
    private string apiKey = "Cedl5hujGrhswiPvzJj0u9pAkQNdOMLgC5GRiBH3yLhOJvfG5knt5Jo3igwg";
    private string savePath; // Directory to save the image locally
    public Canvas promptCanvas; // Reference to the canvas for the prompt input
    public Canvas loadingCanvas; // Reference to the loading canvas
    public Canvas viewimagecanvas; // Reference to the canvas for viewing the image
    public RawImage imageDisplay; // Reference to the RawImage component for displaying the image

    [System.Serializable]
    public class ApiResponse
    {
        public string[] output;
        public string status;
        public float generationTime;
        public int id;
    }

    [System.Serializable]
    public class RequestPayload
    {
        public string key;
        public string prompt;
        public string negative_prompt;
        public string samples;
        public string height;
        public string width;
        public bool safety_checker;
        public string seed;
        public bool base64;
        public string webhook;
        public string track_id;
    }

    private void Start()
    {
        // Set the save path to persistentDataPath for Android
        savePath = Path.Combine(Application.persistentDataPath, "DownloadedImages");
        Debug.Log("Save path set to: " + savePath);
    }

    public void GetImageFromPrompt()
    {
        string prompt = promptText.text;
        Debug.Log("Prompt text: " + prompt);
        Debug.Log("Getting image for prompt: " + prompt);
        StartCoroutine(SendPrompt(prompt));
    }

    private IEnumerator SendPrompt(string prompt)
    {
        Debug.Log("Sending request to API...");

        RequestPayload payload = new RequestPayload
        {
            key = apiKey,
            prompt = prompt,
            negative_prompt = "",
            samples = "1",
            height = "1024",
            width = "1024",
            safety_checker = false,
            seed = null,
            base64 = true,
            webhook = null,
            track_id = null
        };

        string jsonPayload = JsonUtility.ToJson(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        Debug.Log("Payload sent: " + jsonPayload);
        promptCanvas.gameObject.SetActive(false); // Hide the prompt canvas
        loadingCanvas.gameObject.SetActive(true); // Show the loading canvas
        Task<HttpResponseMessage> postTask = client.PostAsync(apiUrl, content);
        yield return new WaitUntil(() => postTask.IsCompleted);

        if (postTask.Result.IsSuccessStatusCode)
        {
            Debug.Log("API call successful!");

            Task<string> readTask = postTask.Result.Content.ReadAsStringAsync();
            yield return new WaitUntil(() => readTask.IsCompleted);

            string jsonResponse = readTask.Result;
            Debug.Log("API Response: " + jsonResponse);

            // Unity's JsonUtility requires a wrapper class and array
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);

            if (response.output != null && response.output.Length > 0)
            {
                string base64ImageUrl = response.output[0]; // Base64 URL in the response
                Debug.Log("Image URL received: " + base64ImageUrl);

                // Start the image download and display
                StartCoroutine(DownloadAndDisplayImage(base64ImageUrl));
            }
            else
            {
                Debug.LogError("No output image found.");
            }
        }
        else
        {
            Debug.LogError("API Error: " + postTask.Result.StatusCode);
        }
    }

    // Coroutine to download and save the image
    private IEnumerator DownloadAndDisplayImage(string base64Url)
    {
        Debug.Log("Starting image download from URL: " + base64Url);
        
        using (UnityWebRequest request = UnityWebRequest.Get(base64Url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Image downloaded successfully!");

                // Get the base64 string
                string base64Image = request.downloadHandler.text;

                Debug.Log("Base64 image data received, length: " + base64Image.Length);

                // Decode the base64 string into byte array
                byte[] imageBytes = System.Convert.FromBase64String(base64Image);
                Debug.Log("Base64 string decoded into byte array, length: " + imageBytes.Length);

                // Save the image locally using persistentDataPath
                string filePath = Path.Combine(savePath, "gen_image.png");

                // Ensure directory exists
                Directory.CreateDirectory(savePath);
                Debug.Log("Directory created at: " + savePath);

                // Save the file
                File.WriteAllBytes(filePath, imageBytes);
                Debug.Log("Image saved locally at: " + filePath);

                // Hide loading canvas and show the image view canvas
                loadingCanvas.gameObject.SetActive(false); // Hide the loading canvas
                viewimagecanvas.gameObject.SetActive(true); // Show the canvas for viewing the image

                // Load the saved image into a Texture2D
                Texture2D texture = new Texture2D(2, 2); // Create a new Texture2D
                texture.LoadImage(imageBytes); // Load the image data into the texture

                // Set the texture to the RawImage component
                imageDisplay.texture = texture;

                Debug.Log("Image displayed on RawImage successfully!");
            }
            else
            {
                Debug.LogError("Failed to download image: " + request.error);
            }
        }
    }
}
