using System.Collections;
using UnityEngine;

public class PokeballAnimationHandler : MonoBehaviour
{
    public Animator animator;

    public void PlayTouch()
    {
        animator.SetTrigger("Touch");
    }

    public IEnumerator PlayShake(int times)
    {
        for (int i = 0; i < times; i++)
        {
            animator.SetTrigger("Shake");
            yield return new WaitForSeconds(1f); // đợi 1 giây giữa mỗi lần shake
        }
    }
    public void PlaySuccess()
    {
        animator.SetTrigger("Success");
    }

    public void PlayBreakOut()
    {
        animator.SetTrigger("Fail");
    }
}
