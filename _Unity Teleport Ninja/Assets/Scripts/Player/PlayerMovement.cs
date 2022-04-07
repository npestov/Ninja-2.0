using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed;
    private PlayerAnim anim;

    Vector3 touchPosition;

    private float timer;
    private float jumpDelay = 0.5f;
    float originalMass;

    //Side to side
    [SerializeField]
    float sideSpeed;
    [SerializeField]
    float jumpForce;
    [SerializeField]
    float jumpOffWallForce;
    [SerializeField]
    float runningUpWallSpeed;

    [Space]

    [SerializeField]
    float jumpBoosterForce;
    Vector3 lastClickPos;
    float turnIndex = 0.5f;
    Rigidbody rb;

    public RunningState runningState = RunningState.NONE;


    // Start is called before the first frame update
    void Awake()
    {
        anim = FindObjectOfType<PlayerAnim>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        originalMass = rb.mass;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.State == GameState.Walking || GameManager.Instance.State == GameState.Killing)
            Move();
    }

    private void Update()
    {
        anim.Turn(turnIndex);

        if (Input.GetMouseButtonDown(0))
            lastClickPos = Input.mousePosition;

        if (runningState == RunningState.UP_WALL_RUNNING)
            transform.position += new Vector3(0, runningUpWallSpeed * Time.deltaTime, 0);
    

        timer -= Time.deltaTime;
    }

    private void UpdateRunnerState(RunningState state)
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        switch (state)
        {
            case RunningState.RUNNING:
                rb.useGravity = true;
                anim.Run();
                break;
            case RunningState.LEFT_WALL_RUNNING:
                anim.WallRun(true);
                break;
            case RunningState.RIGHT_WALL_RUNNING:
                anim.WallRun(false);
                break;
            case RunningState.UP_WALL_RUNNING:
                if (runningState == RunningState.UP_WALL_RUNNING) return;

                anim.UpWallRun();
                rb.useGravity = false;
                break;
            case RunningState.JUMPING:
                StartCoroutine(IncreaseGravity());
                anim.RegularJump();
                rb.useGravity = true;
                break;
            case RunningState.SLIDE:
                anim.Slide();
                break;
            case RunningState.FLIP:
                anim.Vault();
                break;
        }
        runningState = state;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor") && GameManager.Instance.State != GameState.Menu)
        {
            UpdateRunnerState(RunningState.RUNNING);
        }

        if (GameManager.Instance.State != GameState.Walking)
            return;

        if (collision.gameObject.CompareTag("Obstacle") && GameManager.Instance.State != GameState.Killing)
        {
            GameManager.Instance.UpdateGameState(GameState.Lose);
        }
        if (collision.gameObject.CompareTag("rightwall") || collision.gameObject.CompareTag("leftwall"))
        {
            if (Input.GetMouseButton(0))
                touchPosition = Input.mousePosition;
            rb.useGravity = false;

            if (collision.gameObject.CompareTag("rightwall"))
            {
                UpdateRunnerState(RunningState.RIGHT_WALL_RUNNING);
            }
            else
            {
                UpdateRunnerState(RunningState.LEFT_WALL_RUNNING);
            }
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
        if (collision.gameObject.CompareTag("runupwall"))
        {
            UpdateRunnerState(RunningState.UP_WALL_RUNNING);
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (GameManager.Instance.State != GameState.Walking)
            return;

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            if (GameManager.Instance.State == GameState.Walking &&  runningState != RunningState.UP_WALL_RUNNING)
            {
                Jump(jumpForce);
                UpdateRunnerState(RunningState.JUMPING);
            }
        }
        if (collision.gameObject.CompareTag("rightwall") || collision.gameObject.CompareTag("leftwall"))
        {
            UpdateRunnerState(RunningState.JUMPING);
            Jump(jumpOffWallForce);
        }
        if (collision.gameObject.CompareTag("runupwall"))
        {
            collision.gameObject.tag = "Untagged";
            UpdateRunnerState(RunningState.FLIP);
            rb.useGravity = true;
        }

    }

    private void Jump(float force)
    {
        if (timer < 0)
        {
            rb.AddForce(transform.up * force, ForceMode.Impulse);
            rb.AddForce(transform.forward, ForceMode.Impulse);
            timer = jumpDelay;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("bonusWarning"))
        {
            Destroy(collision.gameObject);
            transform.DOMoveX(GameObject.Find("BonusTarget").transform.position.x + 0.7f, 0.7f);
        }
        if (collision.gameObject.CompareTag("jumpbooster"))
        {
            Jump(jumpBoosterForce);
        }
        if (collision.gameObject.CompareTag("slide"))
        {
            UpdateRunnerState(RunningState.SLIDE);
        }
        if (collision.gameObject.CompareTag("slideend"))
        {
            UpdateRunnerState(RunningState.RUNNING);
        }
        if (collision.gameObject.CompareTag("vault"))
        {
            UpdateRunnerState(RunningState.FLIP);
        }

    }

    public void IncreaseSpeed()
    {
        playerSpeed += 0.002f;
    }

    void Move()
    {
        //Move less if killing
        Vector3 deltaPosition = new Vector3(0,0,0);
        if (runningState != RunningState.UP_WALL_RUNNING)
        {
            if (GameManager.Instance.State == GameState.Killing)
                deltaPosition = transform.forward * playerSpeed / 1.2f;
            else
                deltaPosition = transform.forward * playerSpeed;
        }

        //SIDE TO SIDE START, comment this

        touchPosition = Input.mousePosition;
        //left
        if (Input.GetMouseButton(0) && GameManager.Instance.State != GameState.Killing && touchPosition.x > lastClickPos.x * 1.1f)
        {
            if (turnIndex > 0.35f)
                turnIndex -= 0.1f * Time.deltaTime * Mathf.Sqrt(Mathf.Abs(lastClickPos.x - touchPosition.x));
            if (runningState != RunningState.RIGHT_WALL_RUNNING)
                deltaPosition += transform.right * sideSpeed * Mathf.Sqrt(Mathf.Abs(lastClickPos.x - touchPosition.x));
        }
        //right
        else if (Input.GetMouseButton(0) && GameManager.Instance.State != GameState.Killing && touchPosition.x < lastClickPos.x * 0.9f)
        {
            if (turnIndex < 0.65f)
                turnIndex += 0.1f * Time.deltaTime * Mathf.Sqrt(Mathf.Abs(lastClickPos.x - touchPosition.x));
            if (runningState != RunningState.LEFT_WALL_RUNNING)
                deltaPosition -= transform.right * sideSpeed * Mathf.Sqrt(Mathf.Abs(lastClickPos.x - touchPosition.x));
        }
        else
        {
            if (turnIndex > 0.5)
            {
                turnIndex -= 0.5f * Time.deltaTime;
            }
            if (turnIndex < 0.5)
            {
                turnIndex += 0.5f * Time.deltaTime;
            }
        }
        
       
        if (deltaPosition.x != float.NaN && deltaPosition.x != float.PositiveInfinity && deltaPosition.x != float.NegativeInfinity)
        { 
            transform.position += deltaPosition * Time.deltaTime;
        }
         
        //uncomment if not using side
        transform.position += deltaPosition * Time.deltaTime;
        //SIDE TO SIDE END

    }

    private IEnumerator IncreaseGravity()
    {
        yield return new WaitForSeconds(1);
        while (runningState == RunningState.JUMPING)
        {
            yield return new WaitForSeconds(0.1f);
            rb.mass += 0.2f;
        }
        rb.mass = originalMass;
    }
}

public enum RunningState
{
    NONE,
    RUNNING,
    LEFT_WALL_RUNNING,
    RIGHT_WALL_RUNNING,
    UP_WALL_RUNNING,
    JUMPING,
    FLIP,
    SLIDE
}
