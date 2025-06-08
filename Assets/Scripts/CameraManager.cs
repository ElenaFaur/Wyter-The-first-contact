using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera[] allVirtualCameras;
    [SerializeField]private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer currentFramingTransposer;

    [Header("Y Damping Settings")]
    [SerializeField] private float fallPanAmount = 0.1f;
    [SerializeField] private float panTime = 0.2f;
    public float playerFallSpeedThreshold = -10f;
    public bool isLerpingYDamping { get; private set; }
    public bool hasLerpedYDamping { get; private set; }
    private float normalYDamp;

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var cam in allVirtualCameras)
        {
            cam.Follow = PlayerMovement.Instance.transform;
        }

        // Initialize with first active camera
        foreach (var cam in allVirtualCameras)
        {
            if (cam.enabled)
            {
                currentCamera = cam;
                currentFramingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                normalYDamp = currentFramingTransposer.m_YDamping;
                break;
            }
        }
    }

    public CinemachineFramingTransposer GetCurrentFramingTransposer()
    {
        return currentFramingTransposer;
    }

    public void SwapCamera(CinemachineVirtualCamera newCam)
    {
        currentCamera.enabled = false;
        currentCamera = newCam;
        currentCamera.enabled = true;
        currentFramingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void StartYDampingCoroutine(bool isPlayerFalling)
    {
        StartCoroutine(LerpYDamping(isPlayerFalling));
    }

    private IEnumerator LerpYDamping(bool isPlayerFalling)
    {
        isLerpingYDamping = true;

        float startYDamp = currentFramingTransposer.m_YDamping;
        float endYDamp = isPlayerFalling ? fallPanAmount : normalYDamp;

        if (isPlayerFalling)
        {
            hasLerpedYDamping = true;
        }
        else
        {
            hasLerpedYDamping = false;
        }

        float timer = 0f;
        while (timer < panTime)
        {
            timer += Time.deltaTime;
            float lerpedAmount = Mathf.Lerp(startYDamp, endYDamp, timer / panTime);
            currentFramingTransposer.m_YDamping = lerpedAmount;
            yield return null;
        }

        isLerpingYDamping = false;
    }
}