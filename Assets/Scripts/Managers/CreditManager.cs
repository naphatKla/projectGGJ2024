using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditManager : MonoBehaviour
{
    //[SerializeField] private Animator creditAnimator;
    [SerializeField] private Image transition;
    private bool _isTransitioning;

    // Update is called once per frame
    void Update()
    {
        if (_isTransitioning) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isTransitioning = true;
            transition.GetComponent<Animator>().SetTrigger("ChangeScene");
            DOVirtual.DelayedCall(0.6f, (() =>
            {
                SceneManager.LoadScene(SceneName.MainMenu.ToString());
            }));
        }
    }
    
}
