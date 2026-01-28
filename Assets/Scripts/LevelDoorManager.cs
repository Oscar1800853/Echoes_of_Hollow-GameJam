using UnityEngine;

public class LevelDoorManager : MonoBehaviour
{
    public Animator[] doorAnimators;
    private int enemyCount;
    private bool doorsOpened = false;

    private void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("enemy").Length;
    }

    public void EnemyDefeated()
    {
        if (doorsOpened) return;

        enemyCount--;

        if(enemyCount <=0)
        {
            OpenAllDoors();
        }
    }

    void OpenAllDoors()
    {
        doorsOpened = true;

        foreach (Animator anim in doorAnimators)
        {
            anim.SetTrigger("OpenDoor");
        }
    }
}
