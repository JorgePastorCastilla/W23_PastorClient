using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
using System.Net.Http;
using Assets.Scripts.Models;

public class Register : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public Button registerButton;
    public Text messageBoardText;
    public Player playerManager;

    string httpServer;

    private void Start()
    {
        httpServer = playerManager.GetHttpServer();
        passwordInputField.text = "Secret_20";
        confirmPasswordInputField.text = "Secret_20";
    }

    public void OnRegisterButtonClick()
    {
        StartCoroutine( RegisterNewUser() );
    }

     IEnumerator RegisterNewUser()
     {
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        else if (string.IsNullOrEmpty(passwordInputField.text))
        {
            throw new NullReferenceException("Password can't be void");
        }
        else if (passwordInputField.text != confirmPasswordInputField.text)
        {
            throw new Exception("Passwords don't match");
        }

        AspNetUserRegister newUser = new AspNetUserRegister();
        newUser.Email = emailInputField.text;
        newUser.Password = passwordInputField.text;
        newUser.ConfirmPassword = confirmPasswordInputField.text;

        var userToRegister = JsonUtility.ToJson(newUser);

        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/Account/register", "POST"))
        {
            string bodyJson = JsonUtility.ToJson(newUser);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.SetRequestHeader("Content-type", "application/json");

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("RegistrarAspNetUser > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegistrarAspNetUser > Info: " + httpClient.responseCode);
            }
        }
        StartCoroutine(InsertPlayer());
    }
    IEnumerator InsertPlayer()
    {
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        else if (string.IsNullOrEmpty(passwordInputField.text))
        {
            throw new NullReferenceException("Password can't be void");
        }
        else if (passwordInputField.text != confirmPasswordInputField.text)
        {
            throw new Exception("Passwords don't match");
        }

        Assets.Scripts.Models.Player newUser = new Assets.Scripts.Models.Player();
        newUser.Email = emailInputField.text;
        newUser.Nickname = emailInputField.text;
        yield return GetAspNetUserId();
        newUser.Id = playerManager.PlayerId;

        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/player/insertnewplayer", "POST"))
        {
            string bodyJson = JsonUtility.ToJson(newUser);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            yield return GetAuthenticationToken();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("Insertnewplayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegistrarAspNetUser > Info: " + httpClient.responseCode);
            }
        }
    }

    private IEnumerator GetAspNetUserId()
    {

        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/Account/UserId", "GET"))
        {

            byte[] bodyRaw = Encoding.UTF8.GetBytes("Nothing");

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            if (string.IsNullOrEmpty(playerManager.Token))
            {
                yield return GetAuthenticationToken();
            }
            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAspNetUserId > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                playerManager.PlayerId = httpClient.downloadHandler.text.Replace("\"", "");
                Debug.Log("GetAspNetUserId > Info: " + playerManager.PlayerId);
            }

        }

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

}
