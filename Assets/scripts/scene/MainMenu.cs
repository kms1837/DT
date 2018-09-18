using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public Transform SceneFaderImage;
    // Use this for initialization
    void Start () {
        SceneFaderImage.GetComponent<Fader>().fadeOutStart(() => {
            SceneFaderImage.gameObject.SetActive(false);
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void newGame() {
        SceneFaderImage.GetComponent<Fader>().fadeInStart(() => {
            SceneManager.LoadScene("quest");
        });
    }
}
