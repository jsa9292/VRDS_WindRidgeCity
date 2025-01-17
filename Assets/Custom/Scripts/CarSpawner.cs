﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public List<GameObject> cars;
    public GameObject car;
    private CarAI carAI;
	public int randseed = 0;
	public System.Random rand;
    // Start is called before the first frame update
	void Awake(){

		rand = new System.Random(randseed);
	}
	public void Spawn(Transform spawneeParent)
    {
        int carNum = 0;
        car = cars[Random.Range(0, cars.Count)];
		car = Transform.Instantiate(car,transform.position + transform.forward*-1f,transform.rotation,spawneeParent);

        carAI = car.GetComponent<CarAI>();
        carAI.nf = transform.GetComponent<NodeFollower>();
		carAI.transform.LookAt(carAI.nf.transform);
		//carAI.speed = tm.CarSpeeds[carNum];
        //carAI.steering = tm.CarSteers[carNum];
        //carAI.stopDist = tm.CarStopDists[carNum];
    }
}
