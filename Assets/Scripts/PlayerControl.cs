using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    private float Factor = 3f;
    public float MaxDistance = 2;
    Vector3 _direction = new Vector3(1, 0, 0);
    Rigidbody _rigidbody;

    private bool enableInput = false;

    public Transform Head;
    public Transform Body;

    public GameObject[] BoxTemplates;
    public GameObject Stage;
    private GameObject currentStage;

    private Vector3 _cameraRelativePosition;

    private float startTime;

    public Transform StartPanel;
    public Button StartButton;
    public Button ListButton;
    public Button ExitButton;
    public Button RestartButton;

    public Text CutTimeText;

    public AudioSource GameAudioSource;
    public AudioSource PlayerAudioSource;
    public AudioClip BgmAudio;
    public AudioClip GameOverAudio;
    public AudioClip CutTimeAudio;

    public Text ScoreText;
    public Text ScoreAnimeText;
    private int lastReward = 1;
    private int score;

    public GameObject _ParticleSystem;

    private bool firstCollider = true;
    Ray ray;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = transform.GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = Vector3.zero;
        currentStage = Stage;
        SpawnStage();

        GameAudioSource.clip = BgmAudio;
        GameAudioSource.loop = true;
        GameAudioSource.Play();

        _cameraRelativePosition = Camera.main.transform.position - transform.position;

        //OnJump();
        //StartCoroutine(Timer());

        StartButton.onClick.AddListener(delegate
        {
            enableInput = false;
            StartPanel.gameObject.SetActive(false);
            ScoreText.gameObject.SetActive(true);
            CutTimeText.gameObject.SetActive(true);
            StartCoroutine(Timer());
            //enableInput = true;
        });
        ListButton.onClick.AddListener(delegate
        {

        });
        ExitButton.onClick.AddListener(delegate
        {
            Application.Quit();
        });
        RestartButton.onClick.AddListener(delegate
        {
            RestartButton.gameObject.SetActive(false);
            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        });
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            if (hit.transform.tag == "UI")
            {
                return;
            }
            else
            {
                if (enableInput)
                {
#if UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0))
                    {
                        StartPress();
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        EndPress();
                    }

                    if (Input.GetMouseButton(0))
                    {
                        KeepPress();
                    }
#elif UNITY_ANDROID
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    StartPress();
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    EndPress();
                }

                if (Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    KeepPress();
                }
            }
#endif
                }
            }
        }
    }

    IEnumerator Timer()
    {
        GameAudioSource.clip = CutTimeAudio;
        GameAudioSource.loop = false;

        int i = 3;
        while (i > 0)
        {
            GameAudioSource.Play();
            yield return new WaitForSeconds(1);
            i--;
            CutTimeText.text = i.ToString();
        }
        if (i == 0)
        {
            GameAudioSource.clip = BgmAudio;
            GameAudioSource.loop = true;
            GameAudioSource.Play();

            CutTimeText.gameObject.SetActive(false);
            enableInput = true;
        }
    }
    //public void OnJump()
    //{
    //    _rigidbody.AddForce(new Vector3(0, 5f, 0), ForceMode.Impulse);
    //    transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd);
    //}
    public void OnJump(float time)
    {
        _rigidbody.AddForce(new Vector3(0, 5f, 0) + (_direction) * time * Factor, ForceMode.Impulse);
        transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd);
    }

    public void StartPress()
    {
        startTime = Time.time;
        PlayerAudioSource.Play();
        _ParticleSystem.SetActive(true);
    }

    public void KeepPress()
    {
        if (currentStage.transform.localScale.y > 0.3f)
        {
            Body.transform.localScale += new Vector3(1, -1, 1) * 0.05f * Time.deltaTime;
            Head.transform.localPosition += new Vector3(0, -1, 0) * 0.1f * Time.deltaTime;

            currentStage.transform.localScale += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
            currentStage.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
        }
    }

    public void EndPress()
    {
        PlayerAudioSource.Stop();
        _ParticleSystem.SetActive(false);
        var pressTime = Time.time - startTime;
        OnJump(pressTime);

        Body.transform.DOScale(0.1f, 0.2f);
        Head.transform.DOLocalMoveY(0.29f, 0.2f);

        currentStage.transform.DOLocalMoveY(-0.25f, 0.2f);
        currentStage.transform.DOScaleY(0.5f, 0.2f);

        enableInput = false;
    }

    public void AddScore(ContactPoint[] contacrts)
    {
        if (contacrts.Length > 0)
        {
            var hitPoint = contacrts[0].point;
            hitPoint.y = 0;

            var stagePos = currentStage.transform.position;
            stagePos.y = 0;
            var percision = Vector3.Distance(hitPoint, stagePos);
            if (percision < 0.1f)
            {
                lastReward *= 2;
            }
            else
            {
                lastReward = 1;
            }
            StartCoroutine(ScoreAnim(lastReward));
            score += lastReward;
            ScoreText.text = score.ToString();

        }
    }

    public IEnumerator ScoreAnim(int num)
    {
        ScoreAnimeText.gameObject.SetActive(true);
        ScoreAnimeText.text = "+ " + num;
        ScoreAnimeText.GetComponent<Animation>().Play();
        yield return new WaitForSeconds(1);
        ScoreAnimeText.gameObject.SetActive(false);
    }

    public void SpawnStage()
    {
        GameObject prefab;
        if (BoxTemplates.Length > 0)
        {
            prefab = BoxTemplates[Random.Range(0, BoxTemplates.Length)];
        }
        else
        {
            prefab = Stage;
        }

        var stage = Instantiate(prefab);
        stage.transform.position = currentStage.transform.position + _direction * Random.Range(1.1f, MaxDistance);

        var randomScale = Random.Range(0.5f, 1);
        stage.transform.localScale = new Vector3(randomScale, 0.5f, randomScale);

        stage.GetComponent<Renderer>().material.color = new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));
    }

    private void OnCollisionExit(Collision collision)
    {
        enableInput = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogError(collision.gameObject.name);
        if (collision.gameObject.name == "Ground")
        {
            GameOver();
        }
        else
        {
            if (currentStage != collision.gameObject)
            {
                var contacts = collision.contacts;

                if (contacts.Length == 1 && contacts[0].normal == Vector3.up)
                {
                    currentStage = collision.gameObject;
                    AddScore(contacts);
                    RandomDirection();
                    SpawnStage();
                    MoveCamera();

                    if (firstCollider)
                    {
                        firstCollider = false;
                        return;
                    }
                    else
                    {
                        enableInput = true;
                    }
                }
                //else
                //{
                //    GameOver();
                //}
            }
            else
            {
                var contacts = collision.contacts;
                if (contacts.Length == 1 && contacts[0].normal == Vector3.up)
                {
                    if (firstCollider)
                    {
                        firstCollider = false;
                        return;
                    }
                    else
                    {
                        enableInput = true;
                    }
                }
                //else
                //{
                //    GameOver();
                //}
            }
        }
    }

    private void RandomDirection()
    {
        var seed = Random.Range(0, 2);
        _direction = seed == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
        transform.right = _direction;
    }

    public void MoveCamera()
    {
        Camera.main.transform.DOMove(transform.position + _cameraRelativePosition, 1);
    }

    public void GameOver()
    {
        RestartButton.gameObject.SetActive(true);
        //Time.timeScale = 0;
    }
}
