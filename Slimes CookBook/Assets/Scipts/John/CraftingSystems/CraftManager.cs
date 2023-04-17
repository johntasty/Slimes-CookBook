using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
public class CraftManager : MonoBehaviour
{
        
    private IdealGasLaw _Equations;

    [SerializeField]
    private float angle = 10f;
    public float Temperature
    {
        get { return angle; }
        set => angle = value;
    }
    public float _TempBar = 0f;
    public float _PressBar = 0f;
    public float Temp = 0f;
    public float Pressure = 0f;
    public float Volumes = 0f;
    public float PressureWizard = 0f;
   
   
    private Vector2 target;
    private Vector2 targetPressure;
    private Vector2 pos;
    private Vector2 posPressure;

    private bool Loaded = false;

    [SerializeField]
    Image _Bar;
    [SerializeField]
    Image _BarPressure;

    [SerializeField]
    List<Elements> _Elements;
       

    [SerializeField]
    List<float> _Quantities;
    public List<float> Quantities
    {
        get { return _Quantities; }
    }
    Tuple<float, float,float> mixtures;
    Tuple<float, float> testing;
    Tuple<float, float, float, float> factors;

    public event Action<float> _Flow;
    // Start is called before the first frame update
    void Awake()
    { 
        
        //playerInput = new SlimesCookBook();
        _Equations = new IdealGasLaw();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Loaded)
        {
            _Bar = GameObject.Find("Thermo").GetComponent<Image>();
            UpdateEquations();
        }
        
    }
    void UpdateEquations()
    {
        _Bar.transform.localPosition = pos;
        
        mixtures = _Equations.CombinedGasMixtures(_Elements, _Quantities);

        factors = _Equations.IdealGasLawTemp(angle, mixtures.Item1, mixtures.Item2, mixtures.Item3);

        testing = _Equations.CalculateMolesofSubstance(_Quantities[1], _Elements[1].MollarMass, _Quantities[2], _Elements[2].MollarMass);
        Volumes = testing.Item2;
        PressureWizard = factors.Item1;
        Pressure = _Equations.CalculateTotalMass(_Elements, _Quantities);
        Temp = _Equations.CalculatePurity(Pressure, Volumes);
        Temp = Mathf.Clamp01(Temp);
        TempGauge(factors.Item2);
        PressureGauge(factors.Item1);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Crafting")
        {
            Loaded = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Crafting")
        {
            Loaded = true;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
    }
   

    void TempGauge(float _Temp)
    {
        _TempBar = (_Temp / 5000);
        _TempBar = Mathf.Clamp(_TempBar, -1f, 1f);
        float distance = 1 - Mathf.Abs(_TempBar);
        float increment = Mathf.Lerp(1, 0f, distance / 1);
        target = new Vector2((int)Mathf.Sign(_TempBar) * (3.5f), 0);
        pos = Vector2.Lerp(Vector2.zero, target, Mathf.Abs(_TempBar));
      
    }

    void PressureGauge(float _Pressure)
    {
        _PressBar = (_Pressure / 200);
        _PressBar = Mathf.Clamp(_PressBar, 0, 1f);

        targetPressure = new Vector2(0, 700);
        posPressure = Vector2.Lerp(Vector2.zero, targetPressure, Mathf.Abs(_PressBar));

    }
    public void AddElement(Elements elementAdd)
    {
        if (!_Elements.Contains(elementAdd)) {
            _Elements.Add(elementAdd);

            float Index = 0f;
            _Quantities.Add(Index);
            float Num = _Elements.IndexOf(elementAdd);
            _Flow?.Invoke(Num);
        }
        else
        {
            float Num = _Elements.IndexOf(elementAdd);
            _Flow?.Invoke(Num);
        }
      
    }
    public void RegulateFlow(int positionIndex,float amount)
    {
        _Quantities[positionIndex] = amount;
    }

   
}
