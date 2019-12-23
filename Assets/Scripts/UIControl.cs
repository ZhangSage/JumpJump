using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    public Transform StartPanel;
    public Button StartButton;
    public Button ListButton;
    public Button ExitButton;


    // Start is called before the first frame update
    void Start()
    {
        StartButton.onClick.AddListener(delegate
        {
            StartPanel.gameObject.SetActive(false);
        });
        ListButton.onClick.AddListener(delegate
        {

        });
        ExitButton.onClick.AddListener(delegate
        {
            Application.Quit();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
