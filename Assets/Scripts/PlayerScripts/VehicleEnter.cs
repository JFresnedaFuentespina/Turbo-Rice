using Unity.Cinemachine;
using UnityEngine;

public class VehicleEnter : MonoBehaviour
{
    public Transform player;
    public GameObject playerObject;
    public GameObject carObject;
    public MonoBehaviour playerController;
    public MonoBehaviour carController;
    public CinemachineCamera playerCam;
    public CinemachineCamera carCam;
    public Transform CurrentTransform =>
        inCar ? carObject.transform : playerObject.transform;

    public float interactDistance = 3f;
    public bool inCar = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float dist = Vector3.Distance(player.position, carObject.transform.position);
            if (!inCar && dist < interactDistance)
            {
                EnterCar();
            }
            else if (inCar)
            {
                ExitCar();
            }
        }
    }

    void EnterCar()
    {
        inCar = true;

        Animator animator = player.GetComponent<Animator>();
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", 0);

        animator.Rebind();
        animator.Update(0);

        playerController.enabled = false;
        playerObject.SetActive(false);

        carController.enabled = true;
        carObject.SetActive(true);

        playerCam.Priority = 0;
        carCam.Priority = 20;
    }

    void ExitCar()
    {
        inCar = false;

        Vector3 exitPos = carObject.transform.position + carObject.transform.right * -2f;
        exitPos = SnapToGround(exitPos);

        CharacterController cc = player.GetComponent<CharacterController>();
        Animator animator = player.GetComponent<Animator>();

        playerController.enabled = false;
        carController.enabled = false;

        cc.enabled = false;
        player.transform.position = exitPos;
        cc.enabled = true;

        Physics.SyncTransforms();

        playerObject.SetActive(true);
        cc.enabled = true;
        playerController.enabled = true;

        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", 0);
        animator.Update(0);

        carCam.Priority = 0;
        playerCam.Priority = 20;
    }

    Vector3 SnapToGround(Vector3 pos)
    {
        Ray ray = new Ray(pos + Vector3.up * 2f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            pos.y = hit.point.y + 0.1f;
        }

        return pos;
    }
}
