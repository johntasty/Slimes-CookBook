using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WizardSpecific : MonoBehaviour
{
    TemperatureHandler _Temp;
    [SerializeField]
    SimpleControll _PlayerChar;
    [SerializeField]
    CraftManager _Slime;

    [SerializeField]
    Image _Progress;
    [SerializeField]
    private PlayerInputManager playerInputManager;
    [SerializeField]
    Material _StaffBall;
    
    static readonly int Squish = Shader.PropertyToID("_Squishiness");
    static readonly int Scale = Shader.PropertyToID("_Scalar");
    static readonly int Tessalate = Shader.PropertyToID("_tes");
    static readonly int Speed = Shader.PropertyToID("_Speed");

    float valueMin = 0.1f;
    float valueMax = 0.65f;

    private void Awake()
    {
        _Temp = new TemperatureHandler();
       
    }
    private void FixedUpdate()
    {
        if(SceneManager.sceneCount == 2)
        {
            Pressure();
            LoadProgress();
        }
       
    }
    private void OnEnable()
    {

        transform.GetComponentInChildren<CinemachineInput>().look = transform.GetComponent<PlayerInput>().actions.FindAction("Look");
        playerInputManager.onPlayerJoined += AddPlayer;
        _PlayerChar.Look += LookInput;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        if (_Progress.gameObject.activeInHierarchy) { _Progress.gameObject.SetActive(false); }
        _StaffBall.SetFloat(Scale, 0.2f);
        _StaffBall.SetFloat(Squish, 2);
        _StaffBall.SetFloat(Tessalate, 1f);
        _StaffBall.SetFloat(Speed, 1f);
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
        _PlayerChar.Look -= LookInput;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void AddPlayer(PlayerInput obj)
    {
        
        obj.transform.TryGetComponent<CraftManager>(out CraftManager slime);
        _Slime = slime;
    }

    private void LookInput(Vector2 _Input)
    {
        if (_Slime == null) return;
        _Slime.Temperature = _Temp._Temperature(_Input);               
    }

    void LoadProgress()
    {
        if (_Slime == null) return;
        if (!_Progress.gameObject.activeInHierarchy) { _Progress.gameObject.SetActive(true); }
        _Progress.fillAmount = _Slime.Temp;
    }
    void Pressure()
    {
        if (_Slime == null) return;
        float pressure = _Slime.PressureWizard;
        _StaffBall.SetFloat(Squish, pressure);
        float range = valueMax - valueMin;
        float scaleFactor = pressure / 200f;
        float scaledVal = valueMin + scaleFactor * range;
        _StaffBall.SetFloat(Scale, scaledVal);

        float scaleTime = 1 + scaleFactor * (5 - 1);
        _StaffBall.SetFloat(Tessalate, scaleTime);
        _StaffBall.SetFloat(Speed, scaleTime);
        
    }
}
