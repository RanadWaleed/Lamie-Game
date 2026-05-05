using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Game3_MasterManager : MonoBehaviour
{
    public static Game3_MasterManager Instance;

    [Header("Lubnah Guide Settings (إعدادات لبنى)")]
    public GameObject lubnahCharacter;
    public AudioSource lubnahMouthAudioSource;
    public AudioClip voiceTask1;
    public AudioClip voiceTask2;
    public AudioClip voiceTask3;
    public AudioClip voiceTask4;
    public AudioClip voiceEnding;
    public GameObject lubnahBrushProp;

    [Header("Camera Settings")]
    public Camera mainCamera;
    private float originalSize;
    private Vector3 originalPosition;

    [Header("Task 1 Settings (Table)")]
    public Transform task1Target;
    public float task1ZoomSize = 2.5f;

    [Header("Task 2 Settings (Drawing Table)")]
    public Transform task2Target;
    public float task2ZoomSize = 2.5f;

    [Header("Task 3 Settings (Shelves)")]
    public Transform task3Target;
    public float task3ZoomSize = 2.2f;

    [Header("Task 3 Panel Animation")]
    public RectTransform task3BottomPanel;
    public Vector2 panelHiddenPos = new Vector2(0, -1500f);
    public Vector2 panelVisiblePos = new Vector2(0, -300f);
    public float panelSlideDuration = 1.0f;

    [Header("UI Elements")]
    public GameObject transitionNextButton;
    public Image darkOverlay;
    public Transform taskBoard;
    public GameObject finalTableObject;
    public GameObject task3Outline;

    [Header("Environment")]
    public GameObject mainEnvironmentTable;

    [Header("Cinematic Ending Elements")]
    public ParticleSystem finalEndDust;
    public GameObject finalGrandImage;

    [Header("Managers")]
    public Task1_Manager task1Manager;
    public Task2_Manager task2Manager;
    public Task3_Manager task3Manager;
    public Task4_Manager task4Manager;

    private int currentTaskNumber = 1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalPosition = mainCamera.transform.position;
        originalSize = mainCamera.orthographicSize;

        darkOverlay.color = new Color(0, 0, 0, 0);
        darkOverlay.gameObject.SetActive(false);
        taskBoard.localScale = Vector3.zero;

        if (finalTableObject != null) finalTableObject.SetActive(false);
        if (task3Outline != null) task3Outline.SetActive(false);
        if (transitionNextButton != null) transitionNextButton.SetActive(false);

        if (lubnahCharacter != null) lubnahCharacter.SetActive(true);
        if (task4Manager != null) task4Manager.gameObject.SetActive(false);
        StartCoroutine(IntroSequenceTask1());
    }

    public void OnTransitionButtonClicked()
    {
        if (currentTaskNumber == 1)
        {
            currentTaskNumber = 2;
            StartTask2();
        }
        else if (currentTaskNumber == 2)
        {
            currentTaskNumber = 3;
            StartTask3();
        }
        else if (currentTaskNumber == 3)
        {
            currentTaskNumber = 4; 
            if (transitionNextButton != null) transitionNextButton.SetActive(false);
            StartCoroutine(CinematicTransitionToTask4());
        }
    }

    public void OnTask1Completed()
    {
        if (transitionNextButton != null) transitionNextButton.SetActive(true);
    }


    public void OnTask2Completed()
    {
        if (transitionNextButton != null) transitionNextButton.SetActive(true);
    }
    public void OnTask3Completed()
    {
        if (transitionNextButton != null) transitionNextButton.SetActive(true);
    }
    public void StartTask2()
    {
        if (transitionNextButton != null) transitionNextButton.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(CinematicTransitionToTask2());
    }
    public void StartTask3()
    {
        if (transitionNextButton != null) transitionNextButton.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(CinematicTransitionToTask3());
    }

    IEnumerator CinematicTransitionToTask3()
    {
        // 1. 🎬 اختفاء ناعم جداً لكل عناصر اللعبة الثانية (بدل ما تختفي فجأة)
        if (task2Manager != null)
        {
            yield return StartCoroutine(task2Manager.FadeOutTask2(1.0f));
        }

        // 2. ترانزيشن الرجوع للغرفة (زوم أوت من اللعبة الثانية)
        float elapsed = 0;
        float duration = 1.2f;
        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        if (finalTableObject != null) finalTableObject.SetActive(true);
        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, originalPosition, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, originalSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        // 3. 🗣️ لبنى تتكلم للمهمة الثالثة (والفرشة حقت اللعبة الثانية اختفت لحالها)
        yield return StartCoroutine(PlayLubnahSpeech(3));

        // 4. زوم للرفوف حق اللعبة الثالثة
        if (task3Manager != null) task3Manager.gameObject.SetActive(true);

        elapsed = 0;
        duration = 1.5f;
        startPos = mainCamera.transform.position;
        startSize = mainCamera.orthographicSize;
        Vector3 targetPos = new Vector3(task3Target.position.x, task3Target.position.y, originalPosition.z);

        darkOverlay.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, task3ZoomSize, elapsed / duration);
            darkOverlay.color = new Color(0, 0, 0, Mathf.Clamp01(elapsed / duration) * 0.6f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (task3Outline != null) task3Outline.SetActive(true);
        if (task3BottomPanel != null)
        {
            task3BottomPanel.gameObject.SetActive(true);
            elapsed = 0;
            while (elapsed < panelSlideDuration)
            {
                task3BottomPanel.anchoredPosition = Vector2.Lerp(panelHiddenPos, panelVisiblePos, elapsed / panelSlideDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            task3BottomPanel.anchoredPosition = panelVisiblePos;
        }

        if (task3Manager != null) task3Manager.LoadLevel(0);
    }

    IEnumerator CinematicTransitionToTask4()
    {
        // 1. إخفاء عناصر اللعبة 3
        if (taskBoard != null) taskBoard.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (task3BottomPanel != null) task3BottomPanel.gameObject.SetActive(false);
        if (task3Outline != null) task3Outline.SetActive(false);
        if (task3Manager != null) task3Manager.gameObject.SetActive(false);

        // 2. زوم أوت للغرفة
        float elapsed = 0;
        float duration = 1.0f;
        Vector3 currentPos = mainCamera.transform.position;
        float currentSize = mainCamera.orthographicSize;

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(currentPos, originalPosition, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(currentSize, originalSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = originalPosition;
        mainCamera.orthographicSize = originalSize;

        // 3. لبنى تتكلم للمهمة الرابعة
        yield return StartCoroutine(PlayLubnahSpeech(4));

        // 4. تشغيل المهمة الرابعة
        if (task4Manager != null)
        {
            task4Manager.gameObject.SetActive(true);
            task4Manager.StartGame4AfterLubnah();
        }
    }
    
    public void PlayEndSequence()
    {
        StartCoroutine(EndSequence());
    }

    IEnumerator PlayLubnahSpeech(int taskNumber)
    {
        if (lubnahBrushProp != null)
        {
            lubnahBrushProp.SetActive(taskNumber == 2);
        }

        AudioClip clipToPlay = null;
        if (taskNumber == 1) clipToPlay = voiceTask1;
        else if (taskNumber == 2) clipToPlay = voiceTask2;
        else if (taskNumber == 3) clipToPlay = voiceTask3;
        else if (taskNumber == 4) clipToPlay = voiceTask4; 
        else if (taskNumber == 5) clipToPlay = voiceEnding;
        if (clipToPlay != null && lubnahMouthAudioSource != null)
        {
            lubnahMouthAudioSource.clip = clipToPlay;
            lubnahMouthAudioSource.Play();
            yield return new WaitForSeconds(clipToPlay.length + 0.5f);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator IntroSequenceTask1()
    {
        yield return StartCoroutine(PlayLubnahSpeech(1));

        float elapsed = 0;
        float duration = 1.5f;
        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        Vector3 targetPos = new Vector3(task1Target.position.x, task1Target.position.y, originalPosition.z);

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, task1ZoomSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = task1ZoomSize;

        darkOverlay.gameObject.SetActive(true);
        elapsed = 0;
        while (elapsed < 1f)
        {
            darkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0f, 0.6f, elapsed / 1f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        taskBoard.gameObject.SetActive(true);
        elapsed = 0;
        while (elapsed < 0.5f)
        {
            taskBoard.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        taskBoard.localScale = Vector3.one;

        if (task1Manager != null) task1Manager.LoadLevel(0);
    }

    IEnumerator CinematicTransitionToTask2()
    {
        if (taskBoard != null) taskBoard.gameObject.SetActive(false);

        float startAlpha = 0f;
        if (darkOverlay != null) startAlpha = darkOverlay.color.a;

        float elapsed = 0;
        float duration = 1.2f;
        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, originalPosition, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, originalSize, elapsed / duration);

            if (darkOverlay != null)
            {
                darkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, 0f, elapsed / duration));
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = originalPosition;
        mainCamera.orthographicSize = originalSize;

        if (darkOverlay != null)
        {
            darkOverlay.color = new Color(0, 0, 0, 0f);
            darkOverlay.gameObject.SetActive(false);
        }

        if (task1Manager != null) task1Manager.gameObject.SetActive(false);

        if (finalTableObject != null) finalTableObject.SetActive(true);
        if (task1Manager != null && task1Manager.finalCompleteStars != null)
        {
            task1Manager.finalCompleteStars.gameObject.SetActive(true);
            task1Manager.finalCompleteStars.Play();
        }

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(PlayLubnahSpeech(2));

        if (task2Manager != null) task2Manager.gameObject.SetActive(true);

        elapsed = 0;
        duration = 2.5f;
        startPos = mainCamera.transform.position;
        startSize = mainCamera.orthographicSize;
        Vector3 targetPos = new Vector3(task2Target.position.x, task2Target.position.y, originalPosition.z);

        if (darkOverlay != null) darkOverlay.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, task2ZoomSize, elapsed / duration);

            if (darkOverlay != null)
            {
                darkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, elapsed / duration));
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = task2ZoomSize;
        if (darkOverlay != null) darkOverlay.color = new Color(0, 0, 0, 1f);

        if (mainEnvironmentTable != null) mainEnvironmentTable.SetActive(false);
        if (task2Manager != null) task2Manager.ShowTableOnly();

        yield return new WaitForSeconds(1.0f);

        if (task2Manager != null) task2Manager.EnableBrushCursor();

        elapsed = 0;
        float fadeOutDuration = 1.5f;
        while (elapsed < fadeOutDuration)
        {
            if (darkOverlay != null)
            {
                darkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);

        if (task2Manager != null) task2Manager.LoadLevel(0);
    }



    IEnumerator EndSequence()
    {
    
        if (task4Manager != null)
        {
            task4Manager.HidePanelCompletely();
            yield return new WaitForSeconds(0.5f);
            task4Manager.gameObject.SetActive(false);
        }

      
        if (finalEndDust != null)
        {
            finalEndDust.gameObject.SetActive(true);
            finalEndDust.transform.position = originalPosition;
            finalEndDust.Play();
        }

       
        yield return StartCoroutine(PlayLubnahSpeech(5));

        yield return new WaitForSeconds(0.4f);

        if (finalGrandImage != null) finalGrandImage.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (MasterManager.Instance != null) MasterManager.Instance.ShowNextButton();
    }
}