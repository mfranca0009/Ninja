using UnityEngine;

public static class AnimatorExtensions
{
    public static bool IsPlayingLayer(this Animator animator, int layerIndex)
    {
        return animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime % 1.0f < 1.0f;
    }
 
    public static bool IsPlayingLayer(this Animator animator, string layerName)
    {
        return animator.IsPlayingLayer(animator.GetLayerIndex(layerName));
    }
 
    public static bool IsPlayingAnimation(this Animator animator, string animationName, int layerIndex)
    {
        return animator.IsPlayingLayer(layerIndex) && animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(animationName);
    }
 
    public static bool IsPlayingAnimation(this Animator animator, string animationName, string layerName)
    {
        return animator.IsPlayingAnimation(animationName, animator.GetLayerIndex(layerName));
    }
 
    public static bool IsPlayingAnyLayer(this Animator animator)
    {
        for (int i = 0; i < animator.layerCount; i++)
            if (animator.IsPlayingLayer(i))
                return true;
        
        return false;
    }
}
