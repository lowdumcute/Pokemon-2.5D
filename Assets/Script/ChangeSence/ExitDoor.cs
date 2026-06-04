using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitDoor : MonoBehaviour
{
    public string outdoorSceneName = "Main";
    public string outdoorSpawnPointName = "House1ExitPoint"; // 💡 Tên object ngoài map
    public float cutsceneDuration = 1f;
    private GameObject cutSence;

    private GameObject playerRef;

    public void Start()
    {
        if (cutSence == null)
        {
            cutSence = GameObject.Find("cutSence");
            if (cutSence == null)
                Debug.LogWarning("Không tìm thấy GameObject tên 'cutSence' trong scene.");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (DoorTrigger.isTransitioning) return;
        if (other.CompareTag("Player"))
        {
            playerRef = other.gameObject;
            DontDestroyOnLoad(playerRef);

            // 🔍 Ghi lại tên điểm spawn ngoài map
            PlayerPrefs.SetString("SpawnPointName", outdoorSpawnPointName);

            StartCoroutine(ExitHouse());
        }
    }

    private IEnumerator ExitHouse()
    {
        DoorTrigger.isTransitioning = true;

        if (cutSence != null)
        {
            cutSence.SetActive(true);
            var anim = cutSence.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("open");
        }

        yield return new WaitForSeconds(cutsceneDuration);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(outdoorSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == outdoorSceneName && playerRef != null)
        {
            string spawnName = PlayerPrefs.GetString("SpawnPointName", "");
            GameObject spawnPoint = GameObject.Find(spawnName);

            if (spawnPoint != null)
            {
                playerRef.transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy spawn point: {spawnName}");
                playerRef.transform.position = Vector3.zero; // Fallback
            }

            SceneManager.MoveGameObjectToScene(playerRef, scene);

            cutSence?.GetComponent<Animator>()?.SetTrigger("close");
            SceneManager.sceneLoaded -= OnSceneLoaded;

            playerRef.GetComponent<MonoBehaviour>().StartCoroutine(DelayResetTransitionFlag());
        }
    }

    private IEnumerator DelayResetTransitionFlag()
    {
        yield return new WaitForSeconds(1f);
        DoorTrigger.isTransitioning = false;
    }
}
