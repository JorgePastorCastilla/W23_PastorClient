using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;
using System.Text;
using UnityEditor;
using Assets.Scripts.Models;

public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    private float spawnRate = 1.0f;
    private int score;
    public TextMeshProUGUI scoreText;
    public GameObject titleScreen;
    public GameObject gameOverScreen;
    public bool isGameActive;
    public InputField messageInput;
    public Text chatBoard;
    public Player playerManager;
    public TextMeshProUGUI playerEmailText;
    
    public string loginTime;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<Player>();

        playerEmailText.text = playerManager.Email;
        loginTime = DateTime.Now.ToString();
        StartCoroutine(Refresh());
    }

    public void StartGame(int difficulty)
    {
        isGameActive = true;
        score = 0;
        UpdateScore(0);
        titleScreen.gameObject.SetActive(false);
        spawnRate /= difficulty;
        StartCoroutine(SpawnTarget());
    }
    public void SendMessage()
    {
        StartCoroutine(Enviar());
    }

    IEnumerator Enviar()
    {
        ChatModel newUser = new ChatModel();
        newUser.Id = playerManager.PlayerId;
        newUser.message = messageInput.text;
        newUser.time = DateTime.Now.ToString();

        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/chat/insertnewmessage", "POST"))
        {
            string bodyJson = JsonUtility.ToJson(newUser);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertnewMESSAGE > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("MESSAGE OK > Info: " + httpClient.responseCode);
            }
        }
    }
    private IEnumerator Refresh()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            yield return GetMessages();

        }
    }
    private IEnumerator GetMessages()
    {
        chatBoard.text += "";
        
        using (UnityWebRequest httpClient = new UnityWebRequest(playerManager.GetHttpServer() + "api/chat/getMessages", "POST"))
        {
            Assets.Scripts.Models.Time time = new Assets.Scripts.Models.Time();
            time.time = loginTime;
f            var bodyJson = JsonUtility.ToJson(time);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetMessages > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                ListChatModel messages = new ListChatModel();
                var response = httpClient.downloadHandler.text.Replace("\"", "");
                response = "{\"mylist\":" + response + "}";
                messages = JsonUtility.FromJson<ListChatModel>(response);
                foreach(var m in messages.mylist)
                {
                    chatBoard.text += m.Id.Substring(0,3)+": "+m.message;
                }
                Debug.Log("GetAspNetUserId > Info: " + playerManager.PlayerId);
            }

        }
    }
    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int randomIndex = UnityEngine.Random.Range(0, 4);
            Instantiate(targets[randomIndex]);
        }
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        gameOverScreen.gameObject.SetActive(true);
        isGameActive = false;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

}
