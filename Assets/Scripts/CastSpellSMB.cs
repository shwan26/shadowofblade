using UnityEngine;

public class CastSpellSMB : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var sp = animator.GetComponent<SwordRainSpell>();
        if (sp) sp.OnCastEnd();
    }
}
