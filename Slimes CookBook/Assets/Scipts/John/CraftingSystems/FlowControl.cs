using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class FlowControl : MonoBehaviour
{
    float[] _Quantities;

    [SerializeField]
    CraftManager _Manager;
    [SerializeField]
    SimpleControll _PlayerChar;
    private SlimesCookBook playerInput;
    private Inventory _SlimeInventory;
    [SerializeField]
    private float _FlowSpeed;
    [SerializeField]
    private float _FlowAmount;
    [SerializeField]
    private int _MatIndex;

    private Vector2 _LookInput;
    public float FlowAmount
    {
        get { return _FlowAmount; }

        set => _FlowAmount = value;
    }

    public string _CompoundName;

    public bool _AmountActive = false;
    public bool _SpeedActive = false;

    [SerializeField]
    Elements current;
    [SerializeField]
    Iinteractable _CurrentSelected;
    [SerializeField]
    Dictionary<Elements, List<float>> _ElementsDict = new Dictionary<Elements, List<float>>();
    [SerializeField]
    Dictionary<Elements, IEnumerator> _CoroutinesTrack = new Dictionary<Elements, IEnumerator>();
    public Dictionary<Elements, List<float>> ElementsDict
    {
        set => _ElementsDict = value;
    }
    void Awake()
    {       
        
        playerInput = new SlimesCookBook();
        _SlimeInventory = GetComponent<Inventory>();
        foreach(Elements item in _SlimeInventory.Materials)
        {
            List<float> _temp = new List<float> { 0f, 0f, 0f };
            _ElementsDict.Add(item, _temp);
        }
        _Manager._Flow += AddIndex;
        _PlayerChar.Look += LookInput;
    }


    private void Update()
    {
               
    }
    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void LookInput(Vector2 _Input)
    {
        _LookInput = _Input;
        if (_CurrentSelected != null)
        {
            if (_AmountActive)
            {
                ControlAmount(_LookInput.x);
                SetAmount();
            }
            else if (_SpeedActive)
            {
                ControlFlowSpeed(_LookInput.x);
                SetSpeed();
            }
        }
    }
  
    void ControlAmount(float value)
    {
        _FlowAmount += value;        
    }
    void ControlFlowSpeed(float value)
    {
        _FlowSpeed += value;
        Mathf.Clamp(_FlowSpeed, 0f, 10f);
    }

    void _InventoryItem(string _Compoment)
    {
         _SlimeInventory.Elements.TryGetValue(_Compoment,out Object value);
         current = (Elements)value;
    }
    void _SetElementToManager()
    {
        Debug.Log(current.CompoundName);
        _Manager.AddElement(current);
    }
    public void AddIndex(float Index)
    {
        _ElementsDict.TryGetValue(current, out List<float> _values);
        if (_values.Contains(Index)) return;
        
        _values[0] = Index;
    }
    void FlowRegulator()
    {
        _ElementsDict.TryGetValue(current, out List<float> _values);       
        //_Flow = _values[1];        
        _values[1] = _FlowAmount;
    }
    public void SetAmount()
    {
        if (_CurrentSelected == null) return;
        _AmountActive = true;
        _SpeedActive = false;
        _CurrentSelected.GetCurrent(_FlowAmount, 0);
    }
    public void SetSpeed()
    {
        if (_CurrentSelected == null) return;
        _AmountActive = false;
        _SpeedActive = true;
        _CurrentSelected.GetCurrent(_FlowSpeed, 1);
    }
    public void StartFlow()
    {
        FlowRegulator();
        _ElementsDict.TryGetValue(current, out List<float> _values);
        bool check = _CoroutinesTrack.TryGetValue(current, out IEnumerator trackCor);
        if (!check)
        {
            _CoroutinesTrack.Add(current, _FlowRegulator((int)_values[0], _values[1], 1f));
        }
        _CoroutinesTrack.TryGetValue(current, out IEnumerator Routine);
        var rout = Routine;
        StartCoroutine(rout);
    }
    public void StopTrack()
    {
        _CoroutinesTrack.TryGetValue(current, out IEnumerator trackCor);
        var rout = trackCor;
        StopCoroutine(rout);
    }
    //public void OnFire(InputAction.CallbackContext context)
    //{
    //    _InventoryItem(_CompoundName);
    //    _SetElementToManager();
    //}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Compound")
        {
            _CurrentSelected = other.GetComponent<Iinteractable>();
            string _Name = _CurrentSelected.GetName();
            _CurrentSelected.Promt(true);
            _InventoryItem(_Name);
            Debug.Log(current.CompoundName);
            _Manager.AddElement(current);

        }
        if (other.tag == "Loader" && UnityEngine.SceneManagement.SceneManager.sceneCount == 1)
        {
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("Crafting", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Compound")
        {
             _CurrentSelected = other.GetComponent<Iinteractable>();
            _CurrentSelected.Promt(false);
            _CurrentSelected = null;
        }
       
    }
    private IEnumerator _FlowRegulator(int Index, float amount, float time)
    {
        while (true)
        {
            float amountStack = _Manager.Quantities[Index];
            amountStack += amount;
            if (amountStack < 0) amountStack = 0;
            _Manager.RegulateFlow(Index, amountStack);
            yield return new WaitForSeconds(time);
        }
        
    }
}
