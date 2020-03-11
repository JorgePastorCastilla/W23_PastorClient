using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayGame : MonoBehaviour
{
    private Player playerController;

    private void OnEnable()
    {
        playerController = FindObjectOfType<Player>();
        if (false)
        {
            gameObject.GetComponent<Button>().interactable = false;
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Prototype 5");
    }

}
