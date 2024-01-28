using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class CreditManager : MonoBehaviour
{
    //[SerializeField] private Animator creditAnimator;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneName.MainMenu.ToString());
        }
    }
    
}
