using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    public GameObject housePrefab;
    public string indoorSceneName = "InDoor";
    public GameObject cutSence;
    public float cutsceneDuration = 1f;

    private GameObject playerRef;

    public static bool isTransitioning = false; // 🔸 Dùng chung giữa các scene

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return; // 🔸 Ngăn khi vừa mới load scene

        if (other.CompareTag("Player"))
        {
            isTransitioning = true;
            playerRef = other.gameObject;
            DontDestroyOnLoad(playerRef);

            PlayerPrefs.SetFloat("OutdoorX", transform.position.x);
            PlayerPrefs.SetFloat("OutdoorY", transform.position.y);

            StartCoroutine(TransitionScene());
        }
    }

    private IEnumerator TransitionScene()
    {
        if (cutSence != null)
        {
            cutSence.SetActive(true);
            var anim = cutSence.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("open");
        }

        yield return new WaitForSeconds(cutsceneDuration);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(indoorSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == indoorSceneName)
        {
            GameObject house = Instantiate(housePrefab, Vector3.zero, Quaternion.identity);
            Transform doorPoint = house.transform.Find("door");

            if (doorPoint != null && playerRef != null)
            {
                playerRef.transform.position = doorPoint.position;
                SceneManager.MoveGameObjectToScene(playerRef, scene);
                //playerRef.GetComponent<PlayerMovement>()?.ResetMovement();
            }

            cutSence?.GetComponent<Animator>()?.SetTrigger("close");

            isTransitioning = false; // ✅ Cho phép lại sau khi load xong

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

