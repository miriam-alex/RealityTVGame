using UnityEngine;

public class OutputStation : MonoBehaviour
{
    public Container outputContainer;
    public ResourceItem resourcePrefab;
    public float generationInterval = 2f;
    public int maxSupply = -1; // -1 for unlimited
    public bool destroyWhenEmpty = false;

    private int currentSupply;
    private float timer;

    private void Start()
    {
        currentSupply = maxSupply;
    }

    private void Update()
    {
		if (maxSupply != -1 && currentSupply <= 0)
		{
            if (destroyWhenEmpty) Destroy(gameObject);
			return;
		}

        timer += Time.deltaTime;
        if (timer >= generationInterval)
        {
            timer = 0f;
            var obj = Instantiate(resourcePrefab);
            obj.transform.position = outputContainer.transform.position;
            obj.transform.localScale = Vector3.one;
            if (outputContainer.AddItem(obj))
            {
                if (maxSupply != -1) currentSupply--;
            }
        }
    }
}
