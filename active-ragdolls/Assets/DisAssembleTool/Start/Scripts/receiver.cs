using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class receiver : MonoBehaviour
{
    //Any mentioning of the word "scene" in this code refers to the SampleScene.unity.
    private Vector3 startPosition;

    private Quaternion startRotation;

    private Transform endTarget;

    private string endTargetName;

    private GameObject endTargetObject;

    //This variable determines how fast your parts will assemble or disassemble.
    //The bigger this variable, the faster the movement.
    //There is no right or wrong on this variable. Do experiment to achieve your liking.
    //If you want all parts to move at the same speed, simply declare it in the function Start below.
    //If not, you can manually adjust your parts individually since this is a public variable.
    public float smoothMove;

    public virtual void Start()
    {
        //Store the starting position and rotation of each part.
        //These variables will be used in the MoveBack function to move the object back to its original starting position (to assemble).
        this.startPosition = this.transform.position;
        this.startRotation = this.transform.rotation;
        //smoothMove = 5.5;//Set the speed here if you want all parts to move at same speed. Simply uncomment this line.
        //As the objects are disassembling, they are actually moving to a pre-determined target. The target can be any form 3D object, as long as it has the transform.position property.
        //In this scene, the targets are actually exact duplicate of the original objects.
        //This is the recommended method, since it allows you to see how everything will look like after they are disassembled.
        //They are placed within the scene to reflect the final disassembled condition.
        //Their Mesh Renderer components are disabled from the Inspector panel to do their roles, which is only to tell their respective originals where to move when they are disassembled.
        //The following line is written to save time linking each moveable object to its target.
        this.endTargetName = ((((this.transform.parent.parent.name + "/") + this.transform.parent.name) + "_targets") + "/") + this.name;
        //Once we have the name of the target, we then attach the object itself to the moveable object, using the following.
        this.endTargetObject = GameObject.Find(this.endTargetName);
        //Finally, as we have the object attached, we record the transform.position of the target.
        this.endTarget = this.endTargetObject.transform;
    }

    public virtual void Update()
    {
    }

    //Move the object to the target position from its starting position, causing it to disassemble.
    public virtual void MoveToTarget()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, this.endTarget.position, Time.deltaTime * this.smoothMove);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.endTarget.rotation, Time.deltaTime * this.smoothMove);
    }

    //Move the object back to its original starting position, causing it to assemble.
    public virtual void MoveBack()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, this.startPosition, Time.deltaTime * this.smoothMove);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.startRotation, Time.deltaTime * this.smoothMove);
    }

    public receiver()
    {
        this.smoothMove = 5.5f;
    }

}