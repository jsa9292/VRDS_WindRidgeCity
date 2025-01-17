﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : MonoBehaviour
{
    public bool loop;
    public float radius;
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public List<Node> exits;
    public List<Node> conflicts;
    public bool exitOn = true;

    public bool detectNodes;
    public bool detectCars;
    public float detectDist = 1f;
    public bool EqHeight = true;
    public float heightTo = 1f;
    public float drawSensitivity;
    public int occupied;
    public bool stoping;
    public bool stop;
    public bool cleanLists;
    public bool isIntersectionNode = false;
    //Curvature Production
    public List<Vector3> roadMovePositions; //A node follower will access the nodes move positions, the cubes will be registerd into this position list also
    public bool createCurves; //check in order to add better curvature to a road
    public bool destroyCurves; //Dev tool to reset messed up curves
    public bool showCurves = false;
    public bool reverse;
    public float numSubNodes; //Determines how many sub nodes are added in between three cube nodes
    public float minDist;
    public float angleThres;
    //MAX_dist - variable for later
    public int smoothCount;
    public bool leftTurn = false;
    public bool rightTurn = false;
    public void Awake() {
        for (int i = 0; i < transform.childCount; i++)
        {
            //Destroy(transform.GetChild(i).GetComponent<Collider>());
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    void CreateCurves()
    {
        roadMovePositions.Clear();
        int numIterations = transform.childCount;
        if (numIterations < 2) {
            return;
        }
        //Get access to first three nodes
        Transform nodeA = transform.GetChild(0);
        Transform nodeB = transform.GetChild(1);

        roadMovePositions.Add(nodeA.position);
        if (loop)
        {
            numIterations++;
        }
        for (int i = 0; i < numIterations - 1; i++)
        {
            Vector3 oldPos;
            Vector3 newPos;
            oldPos = nodeA.position;
            for (float j = 0; j <= 1f; j += 1 / numSubNodes)
            {
                newPos = nodeA.position * (1f - j) + nodeB.position * j;
                if ((oldPos - newPos).magnitude > minDist)
                {
                    roadMovePositions.Add(newPos);
                    oldPos = newPos;
                }
            }
            nodeA = NextNodeT(nodeA);
            nodeB = NextNodeT(nodeB);

        }
        //adding subnodes

        //smoothing
        RaycastHit hit;
        for (int j = 1; j < smoothCount; j++)
        {
            for (int i = 1; i < roadMovePositions.Count - 1; i++)
            {
                roadMovePositions[i] = roadMovePositions[i-1] * .3f + roadMovePositions[i] * .4f + roadMovePositions[i + 1] * .3f;
                
            }
        }
        roadMovePositions.Add(nodeB.position); 
        for (int i = 0; i <= roadMovePositions.Count - 1; i++)
        {
            if (Physics.Raycast(roadMovePositions[i]+Vector3.up*3f, -Vector3.up, out hit, 5f))
            {
                roadMovePositions[i] = new Vector3(roadMovePositions[i].x, hit.point.y + heightTo, roadMovePositions[i].z);
            }
            else
            {
                Debug.Log("can't find height for " + transform.name);
            }
        }
        if (reverse)
        {
            roadMovePositions.Reverse();
        }
    }
    void OnDrawGizmosSelected() {
        Gizmos.color = color1;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform currentNode = transform.GetChild(i);
            Gizmos.DrawLine(currentNode.position, currentNode.position + currentNode.up);
            Gizmos.DrawLine(currentNode.position, NextNodeT(currentNode).position);
        }
        
        if (destroyCurves)
        {
            roadMovePositions.Clear();
            destroyCurves = false;
        }
        
        if (detectCars)
        {
            Collider[] c;
            occupied = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                c = Physics.OverlapSphere(transform.GetChild(i).position, detectDist);
                NodeFollower nf;
                foreach (Collider j in c)
                {
                    if (nf = j.transform.GetComponent<NodeFollower>())
                    {
                        nf.node = this;
                        occupied++;
                    }
                }
            }
            detectCars = false;
        }
        
    }
    void OnDrawGizmos()    {
        /* Makes it so that we can see which paths are turned off */
        //if (stop) return;

        //Function to turn the move positions on or off
        if (showCurves && roadMovePositions != null)
        {
            if (stop) Gizmos.color = color3;
            
            else if (occupied == 0) Gizmos.color = color2;
            else Gizmos.color = color4;
            for (int i = 0; i < roadMovePositions.Count - 1; i++)
            {
                Gizmos.DrawLine(roadMovePositions[i], roadMovePositions[i + 1]);
                Gizmos.DrawLine(roadMovePositions[i+1], roadMovePositions[i] + Vector3.up * .5f);// + Vector3.up * 1.5f * i / roadMovePositions.Count);
            }
        }
        if (showCurves && (roadMovePositions == null || roadMovePositions.Count==0)) {
            Gizmos.color = color1;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform currentNode = transform.GetChild(i);
                Gizmos.DrawLine(currentNode.position, currentNode.position + currentNode.up);
                Gizmos.DrawLine(currentNode.position, NextNodeT(currentNode).position);
            }
        }
        if (EqHeight)
        {
            int lm = 1 << 9;
            lm = ~lm;

            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 p = transform.GetChild(i).position;
                RaycastHit hit;
                if (Physics.Raycast(transform.GetChild(i).position + Vector3.up * 10000f, -Vector3.up, out hit, 20000f, lm))
                {
                    transform.GetChild(i).position = hit.point + Vector3.up * heightTo;
                    //Debug.DrawLine(transform.GetChild(i).position + Vector3.up * 10000f,hit.point);
                }
            }
            EqHeight = false;
        }
        if (cleanLists)
        {
            exits.RemoveAll(node => node == null);
            conflicts.RemoveAll(node => node == null);
            cleanLists = false;
        }
        if (detectNodes)
        {
            Collider[] c;
            exits.Clear();
            int lm = 1 << 9;
            c = Physics.OverlapSphere(transform.GetChild(transform.childCount - 1).position, detectDist, lm);
            foreach (Collider i in c)
            {
                Node n;
                if ((n = i.transform.parent.GetComponent<Node>()) && (i.transform.parent != transform))
                {
                    i.transform.position = transform.GetChild(transform.childCount - 1).position;
                    if (transform.name[0] == 'X' && i.transform.parent.name[0] == 'X') continue;
                    else exits.Add(n);
                }
            }
            foreach (Node n in exits) {
                n.createCurves = true;
            }
            createCurves = true;
            detectNodes = false;

        }
        if (createCurves)
        {
            createCurves = false;
            CreateCurves();
        }


    }
    public Transform NextNodeT(Transform nodeTnow)
    {
        if (stop) return nodeTnow;
        if (nodeTnow.transform.parent != transform) return transform.GetChild(0);
        if (nodeTnow.GetSiblingIndex() + 1 < transform.childCount)
            return nodeTnow.parent.GetChild(nodeTnow.GetSiblingIndex() + 1);
        else if (loop)
            return nodeTnow.parent.GetChild(0);
        else return nodeTnow;
    }
}
