using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineOpenClose : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Collider triggerZone;             // The trigger zone collider (must be set as isTrigger)

    [Header("Animation Settings")]
    public Animator animator;                // Animator that controls opening/closing animations

    [Header("Machine Reference")]
    public MachineReactor machineReactor;    // Reference to the MachineReactor for triggering reactions

    private bool isOpen = false;             // State tracking

    private void OnTriggerEnter(Collider other)
    {
        // Ensure we're only reacting to the intended trigger zone
        if (other == triggerZone)
        {
            if (!isOpen)
            {
                animator.SetTrigger("Open");
            }
            else
            {
                animator.SetTrigger("Close");

                // If it's a freezer, trigger the cold reaction
                if (machineReactor != null && machineReactor.machineType == MachineType.Freezer)
                {
                    machineReactor.TriggerFreezerReaction();
                }
            }

            isOpen = !isOpen;
        }
    }
}