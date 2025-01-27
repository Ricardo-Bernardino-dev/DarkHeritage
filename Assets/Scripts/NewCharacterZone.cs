using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NewCharacterZone : MonoBehaviour
{
    public TextMeshProUGUI pickupText;
    [SerializeField] private CharacterManager charManager;
    [SerializeField] private Transform vfxTransform;
    public Characters character;
    private bool inArea = false;

    [SerializeField] private dialogue dialogueScript;

    void Start()
    {
        pickupText.enabled = false;
    }

    void Update()
    {
        if (dialogueScript.dialogueFinished){
            charManager.UnlockCharacter(character, vfxTransform);
            Destroy(transform.parent.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inArea = true;
            pickupText.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inArea = false;
            pickupText.enabled = false;
        }
    }

}