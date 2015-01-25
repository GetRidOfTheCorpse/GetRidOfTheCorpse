using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float CharacterSpeed = 6f;
    public RuntimeAnimatorController[] animationControllers;

    private GameObject body;
    private Transform myTransform;
    private float rot;
    public float pickupdist = 0.5f;
    private Animator animator;

    private Vector2 movDirection;

    private int xm = 0;
    private int ym = 0;

    public bool showCorpseHelp = false;

    private bool hasKey = false;

    public bool HasBody() {
        return body != null;
    }

    // Use this for initialization
	void Start () {
        myTransform = (Transform)GetComponent("Transform");
        animator = (Animator)GetComponent("Animator");
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody2D rigidBody = (Rigidbody2D)GetComponent("Rigidbody2D");

        if (Input.GetKey("up"))
        {
            ym = 1;
            xm = 0;
            animator.SetInteger("Direction", 2);
            animator.SetBool("Moving", true);
        }
        else if (Input.GetKey("down"))
        {
            ym = -1;
            xm = 0;
            animator.SetInteger("Direction", 0);
            animator.SetBool("Moving", true);

        }
        else
        {
            ym = 0;
            if (Input.GetKey("left"))
            {
                xm = -1;
                animator.SetInteger("Direction", 1);
                animator.SetBool("Moving", true);

            }
            else if (Input.GetKey("right"))
            {
                xm = +1;
                animator.SetInteger("Direction", 3);
                animator.SetBool("Moving", true);

            }
            else
            {
                xm = 0;
                animator.SetBool("Moving", false);
            }
        }

        //if(xm != 0 || ym != 0) rot = Mathf.Atan2(ym, xm)*Mathf.Rad2Deg;


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

                    if (Input.GetKeyDown("space"))
                    {
                        body = obj[i];
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
            //GameObject.Destroy(gameObject.GetComponent("DistanceJoint2D"));
            if (Input.GetKeyUp("space"))
            {
                body = null;
                MainCanvas.Instance.HideHelpText(HelpText.ActionToDropCorpse);
            } else {
                if (showCorpseHelp) MainCanvas.Instance.ShowHelpText(HelpText.ActionToDropCorpse);
            }
        }

        movDirection = new Vector2(xm, ym);
        //myTransform.rotation = Quaternion.Euler(0, 0, rot);
        movDirection *= CharacterSpeed * (HasBody() ? 0.675f : 1);

        if (body != null)
        {
            animator.speed = 0.2f;
            Transform other = (Transform)body.GetComponent("Transform");
            Animator otherAnim = (Animator)body.GetComponent("Animator");

            int charAngle = animator.GetInteger("Direction");
            Vector2 diff = new Vector2();
            switch (charAngle)
            {
                case 0:
                    diff = new Vector2(0, 1);
                    otherAnim.SetInteger("Direction", 2);
                    break;
                case 1:
                    diff = new Vector2(1, 0);
                    otherAnim.SetInteger("Direction", 3);
                    break;
                case 2:
                    diff = new Vector2(0, -1);
                    otherAnim.SetInteger("Direction", 0);
                    break;
                case 3:
                    diff = new Vector2(-1, 0);
                    otherAnim.SetInteger("Direction", 1);
                    break;
            }
            other.position = myTransform.position + (Vector3)(diff * 0.5f);
            //other.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(other.position.y - myTransform.position.y, other.position.x - myTransform.position.x) * Mathf.Rad2Deg);
        }
        else
        {
            animator.speed = 0.5f;
        }

        rigidBody.velocity = movDirection;
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
            MainCanvas.Instance.ShowHelpText(HelpText.WrongDirection, 3);
        }
        else if (other.CompareTag("Key")) 
        {
            hasKey = true;
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Block") && hasKey) 
        {
            Destroy(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Arrow"))
        {
            var sprite = other.GetComponent<SpriteRenderer>();

            StartCoroutine(FadeOutArrow(sprite));
        }
    }

    IEnumerator FadeOutArrow(SpriteRenderer arrow)
    {
        while (arrow.color.a > 0)
        {
            var color = arrow.color;
            color.a -= Time.deltaTime * 2;
            arrow.color = color;
            yield return null;
        }

        Destroy(arrow.gameObject);
    }
}
