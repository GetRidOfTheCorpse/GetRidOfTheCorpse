using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class PlayerController : MonoBehaviour, Character
{
    public static PlayerController Instance
    {
        get { return instance; }
    }
    private static PlayerController instance;

    private BoundingCircle boundingCircle;

    public float CharacterSpeed = 6f;
    public RuntimeAnimatorController[] animationControllers;

    public float pickupdist = 0.5f;

    private GameObject body;
    private Transform myTransform;
    private Animator animator;

    private Rigidbody2D rigidBody;

    private int xm;
    private int ym;

    public bool showCorpseHelp = false;

    private GameObject lastKey;
    private SpriteRenderer smallKey;

    public bool HasBody()
    {
        return body != null;
    }

    public bool HasKey()
    {
        return lastKey != null;
    }

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        myTransform = (Transform)GetComponent("Transform");
        animator = (Animator)GetComponent("Animator");
        smallKey = transform.FindChild("SmallKey").GetComponent<SpriteRenderer>();
        smallKey.enabled = false;

        rigidBody = (Rigidbody2D)GetComponent("Rigidbody2D");

        boundingCircle = new BoundingCircle(transform.position, 10);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        UpdateAnimation();
        UpdateCorpse();
        UpdateCorpseAnimation();

        boundingCircle.Update(transform.position);
        boundingCircle.Draw();

        rigidBody.velocity = new Vector2(xm, ym) * CharacterSpeed * (HasBody() ? 0.675f : 1);
    }

    private void UpdateCorpseAnimation()
    {
        animator.speed = 0.5f;

        if (body != null)
        {
            animator.speed = 0.2f;
            Transform corps = (Transform)body.GetComponent("Transform");
            Animator corpsAnim = (Animator)body.GetComponent("Animator");

            int charAngle = animator.GetInteger("Direction");
            Vector2 diff = new Vector2();
            switch (charAngle)
            {
                case 0:
                    diff = new Vector2(0, 1);
                    corpsAnim.SetInteger("Direction", 2);
                    break;
                case 1:
                    diff = new Vector2(1, 0);
                    corpsAnim.SetInteger("Direction", 3);
                    break;
                case 2:
                    diff = new Vector2(0, -1);
                    corpsAnim.SetInteger("Direction", 0);
                    break;
                case 3:
                    diff = new Vector2(-1, 0);
                    corpsAnim.SetInteger("Direction", 1);
                    break;
            }
            corps.position = myTransform.position + (Vector3)(diff * 0.5f);
        }
    }

    private void UpdateCorpse()
    {
        if (body == null)
        {
            GameObject[] obj = GameObject.FindGameObjectsWithTag("Body");
            bool bodyFound = false;
            for (int i = 0; i < obj.Length; i++)
            {
                Transform other = ((Transform)obj[i].GetComponent("Transform"));
                float magn = (other.position - myTransform.position).magnitude;
                if (magn < pickupdist)
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        body = obj[i];
                        SoundManager.Instance.OneShot(SoundEffect.PickUpLine, body);
                        animator.runtimeAnimatorController = animationControllers[1];
                    }
                    else
                    {
                        if (showCorpseHelp) MainCanvas.Instance.ShowHelpText(HelpText.ActionToDragCorpse);
                        animator.runtimeAnimatorController = animationControllers[0];
                    }
                    bodyFound = true;

                    break;
                }
            }

            if (!bodyFound && showCorpseHelp)
            {
                MainCanvas.Instance.HideHelpText(HelpText.ActionToDragCorpse);
            }
        }
        else if (body != null)
        {
            if (Input.GetButtonUp("Jump"))
            {
                SoundManager.Instance.OneShot(SoundEffect.DropLine, body);
                body = null;
                MainCanvas.Instance.HideHelpText(HelpText.ActionToDropCorpse);
            }
            else
            {
                if (showCorpseHelp) MainCanvas.Instance.ShowHelpText(HelpText.ActionToDropCorpse);
            }
        }
    }

    private void UpdateAnimation()
    {
        if (ym > 0)
        {
            animator.SetInteger("Direction", 2);
            animator.SetBool("Moving", true);
        }
        else if (ym < 0)
        {
            animator.SetInteger("Direction", 0);
            animator.SetBool("Moving", true);
        }
        else if (xm > 0)
        {
            animator.SetInteger("Direction", 3);
            animator.SetBool("Moving", true);
        }
        else if (xm < 0)
        {
            animator.SetInteger("Direction", 1);
            animator.SetBool("Moving", true);
        }
        else
        {
            animator.SetBool("Moving", false);
        }
    }

    private void HandleInput()
    {
        ym = 0;
        xm = 0;

        if (Input.GetAxisRaw("Vertical") > 0)
        {
            ym = 1;
        }
        else if (Input.GetAxisRaw("Vertical") < 0)
        {
            ym = -1;
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            xm = 1;
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            xm = -1;
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<NextLevel>() != null && body != null)
        {
            enabled = false;
            rigidbody2D.velocity = Vector2.zero;
            animator.SetBool("Moving", false);
            var nextLevel = other.GetComponent<NextLevel>().nextLevel;
            MainCanvas.Instance.FadeOut(() => Application.LoadLevel(nextLevel));
        }
        else if (other.CompareTag("DoorIn"))
        {

        }
        else if (other.CompareTag("Key"))
        {
            SoundManager.Instance.OneShot(SoundEffect.Collect, gameObject);
            lastKey = other.gameObject;
            smallKey.enabled = true;
            smallKey.animation.Play("small_key_get", PlayMode.StopAll);
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Block") && HasKey())
        {
            SoundManager.Instance.OneShot(SoundEffect.Open, gameObject);
            Destroy(other.gameObject);
            lastKey = null;
            smallKey.enabled = false;
        }
    }

    public void GotYou(bool body = false)
    {
        if (body || HasBody())
        {
            enabled = false;
            rigidbody2D.velocity = Vector2.zero;
            animator.SetBool("Moving", false);
            var fuckSign = transform.FindChild("FUCK").GetComponent<SpriteRenderer>();
            fuckSign.enabled = true;
            MainCanvas.Instance.FadeOut(() => Application.LoadLevel("LoseScreen"), 2);
            return;
        }
        if (HasKey())
        {
            lastKey.SetActive(true);
            smallKey.animation.Play("small_key_remove");
            lastKey = null;
            SoundManager.Instance.OneShot(SoundEffect.Decollect, gameObject);
        }
    }

    public BoundingCircle GetBoundingCircle()
    {
        return boundingCircle;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
