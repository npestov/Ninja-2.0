using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        anim.speed = 1;
    }

    public void IncreaseAnimSpeed()
    {
        anim.speed = anim.speed + 0.01f;
    }

    public void Run()
    {
        if (!GameManager.Instance.isBonus)
            anim.SetTrigger("run");
        else
            Idle();
    }
    public void Idle()
    {
        anim.SetTrigger("idle");
    }

    public void Die()
    {
        anim.SetTrigger("die");
    }

    public void QuickSlash()
    {
        anim.SetTrigger("quickSlash");
    }
    public void WarpAnim()
    {
        anim.SetTrigger("warp");
    }

    public void Falling()
    {
        if (!GameManager.Instance.isBonus)
            anim.SetTrigger("falling");
        else
            Idle();
    }
    public void RegularJump()
    {
        Debug.Log("jumpanim");
        anim.SetTrigger("jump");
    }
    public void WallRun(bool isLeft)
    {
        Debug.Log("wallrun");
        if (isLeft)
        {
            anim.SetTrigger("leftWallRun");
        }
        else
        {
            anim.SetTrigger("rightWallRun");
        }
    }
    public void UpWallRun()
    {
        anim.SetTrigger("upwall");
    }

    public void StrikeToHalf()
    {
        anim.SetTrigger("strike");
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Water")) {
            GameManager.Instance.UpdateGameState(GameState.Lose);
        }

    }

    public void ResetAnims()
    {
        anim.ResetTrigger("run");
        anim.ResetTrigger("falling");
    }

    public void Turn(float turnAmmount)
    {
        anim.SetFloat("Turn", turnAmmount);
    }
}
