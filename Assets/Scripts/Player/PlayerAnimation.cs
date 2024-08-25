using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    private RuntimeAnimatorController stand;
    [SerializeField]
    private RuntimeAnimatorController sword;
    [SerializeField]
    private RuntimeAnimatorController gun;
    [SerializeField]
    private RuntimeAnimatorController move;

    private Animator animator => GetComponent<Animator>();

    public void ChangeAnimation(string animationName)
    {
        switch (animationName)
        {
            case "stand":
                animator.runtimeAnimatorController = stand;
                break;
            case "sword":
                animator.runtimeAnimatorController = sword;
                break;
            case "gun":
                animator.runtimeAnimatorController = gun;
                break;
            case "move":
                animator.runtimeAnimatorController = move;
                break;
        }
    }
}
