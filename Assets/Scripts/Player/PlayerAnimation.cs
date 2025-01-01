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

    private Animator Animator => GetComponent<Animator>();

    public void ChangeAnimation(string animationName)
    {
        switch (animationName)
        {
            case "stand":
                Animator.runtimeAnimatorController = stand;
                break;
            case "sword":
                Animator.runtimeAnimatorController = sword;
                break;
            case "gun":
                Animator.runtimeAnimatorController = gun;
                break;
            case "move":
                Animator.runtimeAnimatorController = move;
                break;
        }
    }
}
