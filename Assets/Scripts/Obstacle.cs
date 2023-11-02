using Runner;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] ObstacleType obstacleType;

    public ObstacleType ObstacleType => obstacleType;
}
