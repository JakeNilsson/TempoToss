using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Note : MonoBehaviour
{
    public GameObject whole;
    public GameObject sliced;
    public GameObject perfIcon;
    public GameObject goodIcon;
    public GameObject missIcon;

    private Rigidbody2D noteRigidbody;
    private Collider2D noteCollider;
    private Camera mainCamera;

    public int points = 10;

    private bool isOffScreen;
    private bool slicable = false;
    private string type;

    private void Awake() {
        noteRigidbody = GetComponent<Rigidbody2D>();
        noteCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        perfIcon.SetActive(false);
        goodIcon.SetActive(false);
        missIcon.SetActive(false);
    }

    private void Update() {
        // Destroy the note if it goes off the screen, but it starts below the screen, so we need to check if it goes above the screen
        if (transform.position.y > mainCamera.ViewportToWorldPoint(Vector3.zero).y && !isOffScreen) {
            isOffScreen = true;
        }

        if (transform.position.y < mainCamera.ViewportToWorldPoint(Vector3.zero).y && isOffScreen) {
            // if the note is still whole, reset the combo
            if (whole.activeSelf) {
                FindObjectOfType<GameManager>().ResetCombo();
            }
            
            Destroy(gameObject);    // Destroy the note
        }
    }



    private void Slice(Vector3 direction, Vector3 position, float force, GameObject icon) {
        FindObjectOfType<GameManager>().IncreaseScore(points);
        FindObjectOfType<GameManager>().IncreaseCombo(1);
        
        
        whole.SetActive(false);
        sliced.SetActive(true);

        noteCollider.enabled = false;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        sliced.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Rigidbody2D[] slices = sliced.GetComponentsInChildren<Rigidbody2D>();
        

        foreach (Rigidbody2D slice in slices) {
            slice.velocity = noteRigidbody.velocity;
            Vector2 dir2 = new() {
                x = direction.x,
                y = direction.y
            };

            Vector2 pos2 = new() {
                x = position.x,
                y = direction.y
            };

            slice.AddForceAtPosition(dir2 * force, pos2, ForceMode2D.Impulse);
        }
        GameObject iconType = Instantiate(icon, position, Quaternion.identity);
        iconType.SetActive(true);
        Destroy(iconType, 2f);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (this.tag){
        case "Blue Note":
            type = "Blue Perfect";
            break;
        case "Red Note":
            type = "Red Perfect";
            break;
        case "Yellow Note":
            type = "Yellow Perfect";
            break;
        case "Green Note":
            type = "Green Perfect";
            break;
        default:
            Debug.LogError("Default Error");
            break;
        }

        if (other.tag.ToLower().Contains(this.tag.Split(' ')[0].ToLower())){
            slicable = true;
        }

        if (other.CompareTag(type)){
            points = 30;
        }
        if (other.CompareTag("Player") && slicable == true) {
            Blade blade = other.GetComponent<Blade>();
            if (points == 30)
                Slice(blade.direction, blade.transform.position, blade.sliceForce, perfIcon);
            else Slice(blade.direction, blade.transform.position, blade.sliceForce, goodIcon);
         }

    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag(type)) {
            points = 10;
        }

        if (other.tag.ToLower().Contains(this.tag.Split(' ')[0].ToLower())){
            slicable = false;
        }
    }
}
