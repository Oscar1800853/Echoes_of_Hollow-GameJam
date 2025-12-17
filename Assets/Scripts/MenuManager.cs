using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManagement : MonoBehaviour
{
    public GameObject guiaPanel;

    private bool guiaActiva = false;
    
    public void Jugar()
    {
        SceneManager.LoadScene("CinemInicio");
    }

    public void Guia()
    {
        guiaPanel.SetActive(true);
        guiaActiva = true;
    }

    void Update()
    {
        if (guiaActiva && Input.GetKeyDown(KeyCode.Escape))
        {
            guiaPanel.SetActive(false);
            guiaActiva = false;
        }
    }
}
