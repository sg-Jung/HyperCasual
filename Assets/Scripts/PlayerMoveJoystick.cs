using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMoveJoystick : MyJoystick
{
    [Space(10f)][Header("MINE")][Space(10f)]
    public GameObject joyStick;
    public PlayerController playerController;
    public float moveSpeed;
    public float rotateSpeed;
    public float gravity;
    public bool isMove;
    public bool movingLock;
    private CharacterController cc;
    private Transform player;
    private Animator anim;

    protected override void Start()
    {
        base.Start();
        background.gameObject.SetActive(false);

        player = playerController.transform;
        cc = playerController.GetComponent<CharacterController>();
        anim = playerController.GetComponentInChildren<Animator>();
    }

    private void PlayerMove()
    {
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
        if(!cc.isGrounded) cc.Move(new Vector3(0f, -gravity, 0f) * 3f * Time.deltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
        }

        cc.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void Update()
    {
        if(isMove && !movingLock)
        {
            PlayerMove();
        }

        AnimationSelect();
    }

    public void SetMovingLock(bool flag)
    {
        joyStick.SetActive(!flag);
        movingLock = flag;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(movingLock) return;

        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
        isMove = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (movingLock) return;

        background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);
        isMove = false;
    }

    private void AnimationSelect()
    {
        if (anim == null) return;

        anim.SetBool("isMove", isMove);
        anim.SetBool("isStack", playerController.isStacking);
    }
}