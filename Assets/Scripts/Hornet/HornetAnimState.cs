using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornetAnimState : StateMachineBehaviour
{
    public HornetState state;
    public int n;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HornetBoss hornet = animator.GetComponent<HornetBoss>();
        hornet.OnAnimStateEnter(state, n);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HornetBoss hornet = animator.GetComponent<HornetBoss>();
        hornet.OnAnimStateUpdate(state, n);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HornetBoss hornet = animator.GetComponent<HornetBoss>();
        hornet.OnAnimStateExit(state, n);
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