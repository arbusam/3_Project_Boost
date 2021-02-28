using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Rocket : MonoBehaviour
{

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float delay = 1f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip crash;
    [SerializeField] AudioClip completeLevel;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem crashParticles;
    [SerializeField] ParticleSystem completeLevelParticles;

    [SerializeField] InputAction fireAction;
    [SerializeField] InputAction rotateAction;

    new Rigidbody rigidbody;
    AudioSource audioSource;

    enum State { Alive, Dying, Transending }
    State state = State.Alive;

    bool collisions = true;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    void OnEnable()
    {
        fireAction.Enable();
        rotateAction.Enable();
    }

    void OnDisable()
    {
        fireAction.Disable();
        rotateAction.Disable();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !collisions) { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                state = State.Transending;
                audioSource.Stop();
                audioSource.PlayOneShot(completeLevel);
                completeLevelParticles.Play();
                Invoke("LoadNextScene", delay);
                break;
            default:
                state = State.Dying;
                audioSource.Stop();
                audioSource.PlayOneShot(crash);
                crashParticles.Play();
                Invoke("RestartScene", delay);
                break;
        }
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = GetNextSceneIndex(currentSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }

    private int GetNextSceneIndex(int currentIndex)
    {
        if (currentIndex != 5)
        {
            return currentIndex + 1;
        }
        else
        {
            return 0;
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKey(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKey(KeyCode.C))
        {
            collisions = !collisions;
        }
    }

    private void RespondToRotateInput()
    {

        float rotation = rotateAction.ReadValue<Vector2>().x;

        rigidbody.angularVelocity = Vector3.zero;

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || rotation < 0)
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D) || rotation > 0) // Can not rotate Right and Left at the same time
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
    }

    private void RespondToThrustInput()
    {

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.JoystickButton1)) // Can thrust while simultanisly rotating
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidbody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }
}
