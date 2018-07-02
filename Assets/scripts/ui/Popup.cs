using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour {
    public Transform Parent;
    public Transform BackPanel;
    private string description;
    public string Description {
        get { return description; }
        set {
            Text descriptionLabel = this.transform.Find("DescriptionLabel").GetComponent<Text>();
            description = value;
            descriptionLabel.text = description;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void show(string title, string setDescription, UnityEngine.Events.UnityAction btnAction, UnityEngine.Events.UnityAction backAction) {
        Common.showBackPanel(Parent, BackPanel);
        this.gameObject.SetActive(true);
        this.transform.SetSiblingIndex(Parent.childCount);

        Text titleLabel = this.transform.Find("TitleLabel").GetComponent<Text>();
        Description = setDescription;
        Button button = this.transform.Find("Button1").GetComponent<Button>();
        Button cancelButton = this.transform.Find("Button2").GetComponent<Button>();

        titleLabel.text = title;

        button.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        button.onClick.AddListener(btnAction);
        cancelButton.onClick.AddListener(backAction);
    }

    public void clear() {
        Common.clearCloneUIObj(Parent);
        this.gameObject.SetActive(false);
    }
}
