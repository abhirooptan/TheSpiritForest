using UnityEngine;
using System.Collections;

public class SwordAnim : StateMachineBehaviour {

    private MeshCollider[] weapons;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    animator.SetBool("Level1", false);
    //    animator.SetBool("Level2", false);
    //    animator.SetBool("Level3", false);
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        weapons = animator.gameObject.GetComponent<PlayerScript>().Weapons;
        foreach (MeshCollider weapon in weapons)
        {
            weapon.enabled = true;
        }

        animator.SetBool("Level1", false);
        animator.SetBool("Level2", false);
        animator.SetBool("Level3", false);
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (MeshCollider weapon in weapons)
        {
            weapon.enabled = false;
        }

        animator.SetBool("Level1", false);
        animator.SetBool("Level2", false);
        animator.SetBool("Level3", false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
