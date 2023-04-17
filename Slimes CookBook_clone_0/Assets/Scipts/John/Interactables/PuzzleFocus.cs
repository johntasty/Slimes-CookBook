using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleFocus : NetworkBehaviour
{
    [SerializeField]
    Image PopUp;
    [SerializeField]
    string InteractionTag;
    [SerializeField]
    Transform _ObjectToFocus;

    [SerializeField]
    Button InteraactionCanvas = null;
    [SerializeField]
    GameObject Canvas;
    public GameObject _Camera;
    bool check;
    private void Start()
    {
        ObserverListener.Instance.InteractWizard += InteractButton;
    }
    private void InteractButton(bool obj)
    {
                
        if (PopUp.gameObject.activeInHierarchy)
        {
            _Camera.SetActive(true);
            Canvas.SetActive(true);
            if (InteraactionCanvas == null) return;
            InteraactionCanvas.Select();
        }        
    }

    private void OnTriggerEnter(Collider other)
    {
        check = other.GetComponent<NetworkIdentity>().isLocalPlayer;
        if (other.CompareTag(InteractionTag))
        {            
            if (!check) return;
            PopUp.gameObject.SetActive(true);
        }
    }
   
    private void OnTriggerExit(Collider other)
    {
        check = other.GetComponent<NetworkIdentity>().isLocalPlayer;
        if (other.CompareTag(InteractionTag))
        {
            if (!check) return;
            _Camera.SetActive(false);
            Canvas.SetActive(false);
            PopUp.gameObject.SetActive(false);
        }
    }
  
}
