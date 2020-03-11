using UnityEngine;

public class Player : MonoBehaviour
{
    private const string httpServer = "http://localhost:54516/";
    public string GetHttpServer()
    {
        return httpServer;
    }
    public string GetHttpSecureServer()
    {
        return "https://localhost:44358/";
    }

    private string _token;
    public string Token
    {
        get { return _token; }
        set { _token = value; }
    }

    private string _playerId;
    public string PlayerId
    {
        get { return _playerId; }
        set { _playerId = value; }
    }

    private string _email;
    public string Email
    {
        get { return _email; }
        set { _email = value; }
    }
    private string _nickname;
    public string Nickname
    {
        get { return _nickname; }
        set { _nickname = value; }
    }

}
