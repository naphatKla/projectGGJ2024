using DG.Tweening;
using Manager;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditManager : MonoBehaviour
{
    //[SerializeField] private Animator creditAnimator;
    [SerializeField] private Image transition;
    [SerializeField] private AudioClip creditMusic;
    private bool _isTransitioning;
    
    void Awake()
    {
        SoundManager.Instance.PlayMusic(creditMusic,isSkipIfCurrentClipPlaying:true);
    }
    
    void Update()
    {
        if (_isTransitioning) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isTransitioning = true;
            transition.GetComponent<Animator>().SetTrigger("ChangeScene");
            SoundManager.Instance.FadeOutMusic(2f, AfterFadeAction.Stop);
            DOVirtual.DelayedCall(0.6f, (() =>
            {
                SceneManager.LoadScene(SceneName.MainMenu.ToString());
                SoundManager.Instance.FadeInMusic(0.5f, true);
            }));
        }
    }
    
}
