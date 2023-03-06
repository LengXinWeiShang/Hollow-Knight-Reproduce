using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimState : StateMachineBehaviour
{
    public PlayerState state;
    public int n;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacter2D player = animator.GetComponent<PlayerCharacter2D>();
        player.OnAnimStateEnter(state, n);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacter2D player = animator.GetComponent<PlayerCharacter2D>();
        player.OnAnimStateUpdate(state, n);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacter2D player = animator.GetComponent<PlayerCharacter2D>();
        player.OnAnimStateExit(state, n);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}