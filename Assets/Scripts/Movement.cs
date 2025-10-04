using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    private bool cycling = false;
    private Character player;

    private void Update() {
        
    if (cycling)
        {
            player.transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
            Vector3 direction = player.GetOpponent().transform.position - player.transform.position;
            if (direction.sqrMagnitude > 0.001f) player.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    public void Setup(Character newPlayer) => player = newPlayer;
    public bool GetCycling() => cycling;
    public void SetCycling(bool state) => cycling = state;
}
