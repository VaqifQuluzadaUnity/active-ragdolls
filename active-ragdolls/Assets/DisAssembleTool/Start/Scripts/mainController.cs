using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class mainController : MonoBehaviour
{
    //Any mentioning of the word "scene" in this code refers to the SampleScene.unity.
    public int partsAmount; //Type in the amount of steps or groups that you will dis/assemble from the Unity3D inspector panel to this public variable.

    //In this scene, there are 6 sequences of parts assembling and disassembling.
    //Do notice, however, that we have set this variable to 7, instead of 6.
    //This is the best solution so far to avoid bugs so do keep this as a guideline: Add 1 to the amount steps/groups.
    //E.g.: If you are going to have 16 steps, enter 17.
    //Each sequence can include more than one 3D objects, they are identified and grouped together using the tags system in Unity3D.
    public string tagPrefix; //You can change this variable from the Inspector panel.

    //This variable should contain the prefix of your tag.
    //In this scene, the prefix is 'parts'. Therefore, we have typed in 'parts' to this variable in the Inspector panel.
    private bool isDisassembling;

    private int partsAmountModifier;

    private string currentParts;

    private int activeParts;

    public virtual void Start()
    {
        //The default position is where everything is still assembled.
        //If you want your objects start as being disassembleed, simply uncomment the next 3 lines.
        //partsAmountModifier = 1;
        //isDisassembling = true;
        //Disassemble();
        this.partsAmountModifier = this.partsAmount; //Do NOT remove this line. This calibrates the whole code.
    }

    public virtual void Update()
    {
        //The following lines in Update function are the main mover.
        //They have to be in Update to avoid jerky animations.
        if (this.isDisassembling)
        {
            this.Disassemble();
        }
        if (!this.isDisassembling)
        {
            this.Assemble();
        }
    }

    //Find objects with the current tag name inside the for loop below and call the script(receiver) that disassemble the objects automatically.
    //NOTICE how each moving 3D objects have 'receiver.js' script attached.
    //If you have a lot of objects to attach the script to, simply select them from your Hierarchy window in Unity3D and drag-and-drop the script to the Inspector panel.
    //For details on the direction of where the objects will be moving, see comments in 'receiver.js'.
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

    //Find objects with the current tag name inside the for loop and calls the script(receiver) to assemble the objects automatically.
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
        //As mentioned in the documentation, the GUIs are fully customizeable.
        //Alternatively, you can change the input by using keyboards, using the Input.GetKeyDown function.
        //If you are planning to do so, you have to keep include the condition - Everything after the &&.
        //This button will disassemble the objects one by one
        if (GUI.Button(new Rect((Screen.width / 2) - 200, Screen.height - 65, 98, 63), "Detach\n1 Seq.") && (this.activeParts < this.partsAmount))
        {
            this.partsAmountModifier--;
            this.isDisassembling = true;
        }
        //This button will assemble the objects one by one
        if (GUI.Button(new Rect((Screen.width / 2) - 100, Screen.height - 65, 98, 63), "Attach\n1 Seq.") && (this.activeParts >= 1))
        {
            this.partsAmountModifier++;
            this.isDisassembling = false;
        }
        //This button will disassemble all the objects at one go
        if (GUI.Button(new Rect(Screen.width / 2, Screen.height - 65, 98, 63), "Detach\nAll") && (this.activeParts < this.partsAmount))
        {
            this.partsAmountModifier = 1;
            this.isDisassembling = true;
        }
        //This is to assemble all the objects
        if (GUI.Button(new Rect((Screen.width / 2) + 100, Screen.height - 65, 98, 63), "Attach\nAll") && (this.activeParts >= 1))
        {
            this.partsAmountModifier = this.partsAmount;
            this.isDisassembling = false;
        }
    }

}