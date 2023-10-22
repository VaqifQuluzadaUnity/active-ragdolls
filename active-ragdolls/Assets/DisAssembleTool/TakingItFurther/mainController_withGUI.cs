using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class mainController_withGUI : MonoBehaviour
{
    //This script serves as the mainController.js.
    //If you are not familiar with Unity3D at all, we strongly recommend to start with the documentation regarding SampleScene.unity scene.
    //Only use this script as a reference to enhance the GUI for your final product.
    //To facilitate the process of locating the GUI elements, we have commented them.
    //It will help you to differentiate between mainController.js and mainController_withGUI.js.
    public int partsAmount;

    public string tagPrefix;

    private bool isDisassembling;

    private int partsAmountModifier;

    private string currentParts;

    private int activeParts;

    public GUISkin assembleGUI; //This variable is where you attach the GUISkin object.

    public UnityEngine.UI.Image paperTexture; //This is the background paper texture.

    public UnityEngine.UI.Image gridTexture; //This is the foreground grids.

    public virtual void Start()
    {
        this.partsAmountModifier = this.partsAmount;
    }

    public virtual void Update()
    {
        //The following 2 lines make the background and foreground image as big as your screen all the time.
        this.paperTexture.rectTransform.rect.Set((Screen.width / 2) - Screen.width, (Screen.height / 2) - Screen.height, Screen.width, Screen.height);
        this.gridTexture.rectTransform.rect.Set((Screen.width / 2) - Screen.width, (Screen.height / 2) - Screen.height, Screen.width, Screen.height);
        if (this.isDisassembling)
        {
            this.Disassemble();
        }
        if (!this.isDisassembling)
        {
            this.Assemble();
        }
    }

    public virtual void Disassemble()
    {
        this.activeParts = 1;
        while (this.activeParts <= (this.partsAmount - this.partsAmountModifier))
        {
            this.currentParts = this.tagPrefix + this.activeParts;
            if (this.activeParts < this.partsAmount)
            {
                GameObject[] allChildren = GameObject.FindGameObjectsWithTag(this.currentParts);
                int aChildren = 0;
                while (aChildren < allChildren.Length)
                {
                    receiver scriptToDisassemble = null;
                    GameObject aChild = allChildren[aChildren];
                    scriptToDisassemble = (receiver) aChild.GetComponent(typeof(receiver));
                    scriptToDisassemble.MoveToTarget();
                    aChildren++;
                }
            }
            this.activeParts++;
        }
    }

    public virtual void Assemble()
    {
        this.activeParts = this.partsAmount - 1;
        while (this.activeParts > (this.partsAmount - this.partsAmountModifier))
        {
            this.currentParts = this.tagPrefix + this.activeParts;
            GameObject[] allChildren = GameObject.FindGameObjectsWithTag(this.currentParts);
            int aChildren = 0;
            while (aChildren < allChildren.Length)
            {
                receiver scriptToForm = null;
                GameObject aChild = allChildren[aChildren];
                scriptToForm = (receiver) aChild.GetComponent(typeof(receiver));
                scriptToForm.MoveBack();
                aChildren++;
            }
            this.activeParts--;
        }
    }

    public virtual void OnGUI()
    {
        //Most differences between the two scripts will happen within this function.
        GUI.skin = this.assembleGUI; //Start by declaring which GUISkin we are going to use.
        if (GUI.Button(new Rect(10, Screen.height - 70, 98, 63), "Detach\n1 Seq.") && (this.activeParts < this.partsAmount))
        {
            this.partsAmountModifier--;
            this.isDisassembling = true;
        }
        if (GUI.Button(new Rect(120, Screen.height - 70, 98, 63), "Attach\n1 Seq.") && (this.activeParts >= 1))
        {
            this.partsAmountModifier++;
            this.isDisassembling = false;
        }
        if (GUI.Button(new Rect(Screen.width - 216, Screen.height - 70, 98, 63), "Detach\nAll") && (this.activeParts < this.partsAmount))
        {
            this.partsAmountModifier = 1;
            this.isDisassembling = true;
        }
        if (GUI.Button(new Rect(Screen.width - 108, Screen.height - 70, 98, 63), "Attach\nAll") && (this.activeParts >= 1))
        {
            this.partsAmountModifier = this.partsAmount;
            this.isDisassembling = false;
        }
    }

}