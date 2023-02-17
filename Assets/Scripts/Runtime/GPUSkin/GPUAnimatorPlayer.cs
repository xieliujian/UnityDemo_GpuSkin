using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GPUAnimatorPlayer : MonoBehaviour
{
    public int frameRate;
    public int frameCount;
    public int curFrame;

    Animator animator;

    private void OnEnable()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.Play("a001_walk01", 0);

        animator.recorderStartTime = 0;
        animator.recorderStopTime = 1.0f / frameRate * frameCount;
        animator.StartRecording(frameCount);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        for (int i = 0; i < frameCount; ++i)
        {
            animator.Update(1.0f / frameRate);
        }

        animator.StopRecording();
        animator.StartPlayback();
    }

    private void OnValidate()
    {
        animator.playbackTime = (float)curFrame * (1.0f / frameRate);
        animator.Update(0);
    }
}
