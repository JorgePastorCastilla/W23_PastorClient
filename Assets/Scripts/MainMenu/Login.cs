using Assets.Scripts.Models;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public Button loginButton;
    public Button logoutButton;
    public Button playGameButton;
    public Text messageBoardText;
    public Player playerManager;

    private string httpServerAddress;
    public bool isLogin;

    private void Start()
    {
        httpServerAddress = playerManager.GetHttpServer();
        
        passwordInputField.text = "Secret_20";

        isLogin = false;

    }


    public void OnLoginButtonClicked()
    {
        StartCoroutine(TryLogin());
    }

    private void GetToken()
    {

    }

    private IEnumerator TryLogin()
    {

        if (string.IsNullOrEmpty(playerManager.Token))
        {
            yield return GetAuthenticationToken();
        }

        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/Account/UserId"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("TryLogin > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                playerManager.PlayerId = httpClient.downloadHandler.text.Replace("\"", "");
                Debug.Log("TryLogin > Info: Estas logeado");
            }

        }

        yield return GetInfoPlayer();
        isLogin = true;
        loginButton.interactable = false;
        logoutButton.interactable = true;
        playGameButton.interactable = true;
    }
    private IEnumerator GetAuthenticationToken()
    {
        WWWForm data = new WWWForm();

        data.AddField("grant_type", "password");
        data.AddField("username", emailInputField.text);
        data.AddField("password", passwordInputField.text);

        using (UnityWebRequest httpClient = UnityWebRequest.Post(playerManager.GetHttpServer() + "Token", data))
        {

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAuthenticationToken > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                playerManager.Token = authToken.access_token;
            }
        }
    }

    private IEnumerator GetInfoPlayer()
    {

        //using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/GetPlayer/" + player.Id, "GET"))
        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/Player/GetPlayerInfo/" + playerManager.PlayerId, "GET"))
        {
            yield return GetAuthenticationToken();
            httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                Debug.Log(playerManager.GetHttpServer() + "api/Player/GetPlayerInfo/" + playerManager.PlayerId);
                throw new System.Exception("GetInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                Assets.Scripts.Models.Player playerJson = JsonUtility.FromJson<Assets.Scripts.Models.Player>(jsonResponse);

                Debug.Log("GetInfoPlayer > Info: " + playerJson.Email);
                playerManager.PlayerId = playerJson.Id;
                playerManager.Nickname = playerJson.Nickname;
                playerManager.Email = playerJson.Email;

            }
        }
    }
    public void OnLogoutButtonClicked()
    {
        TryLogout();
    }

    private void TryLogout()
    {
            playerManager.Token = string.Empty;
            playerManager.PlayerId = string.Empty;
            playerManager.Email = string.Empty;
            loginButton.interactable = true;
            logoutButton.interactable = false;
            playGameButton.interactable = false;
    }
}
