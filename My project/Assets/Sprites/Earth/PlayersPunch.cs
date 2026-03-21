using UnityEngine;

public class SimplePunch : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.f))
        {
            animator.SetTrigger("PunchEarth");
        }
    }
}