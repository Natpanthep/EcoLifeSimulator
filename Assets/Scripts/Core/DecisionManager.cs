using UnityEngine;
using UnityEngine.EventSystems;

public class DecisionManager : MonoBehaviour
{
    public static DecisionManager I;
    
    [SerializeField] private Camera cam;
    [SerializeField] private UIManager ui;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        if (ui == null) ui = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

        if (Input.GetMouseButtonDown(0))
        {
            // ignore clicks over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);

            if (hit.collider != null)
            {
                var it = hit.collider.GetComponent<Interactable>();
                if (it != null)
                {
                    var decision = it.GetRandomDecision();
                    if (decision != null && ui != null)
                    {
                        ui.ShowDecision(decision, it.displayName);
                    }
                }
            }
        }
    }
}
