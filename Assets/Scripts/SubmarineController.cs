﻿using System.Collections.Generic;
using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    // move emissionForce to each individual emitter and enemy?
    public GameObject emitterPrefab;
    List<Emitter> emitters;
    Rigidbody2D rb;

    private KeyCode[] keyPool;

    void Start()
    {
        keyPool = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F};
        rb = GetComponent<Rigidbody2D>();

        emitters = new List<Emitter>();
        foreach(Transform child in transform.Find("Emitters"))
        {
            Emitter emitter = child.GetComponent<Emitter>();
            if (emitter != null)
            {
                emitter.letter.transform.position = transform.position + new Vector3(emitter.offset.x, emitter.offset.y);
                emitters.Add(emitter);
            }
        }
    }

    void Update()
    {
        foreach (var emitter in emitters)
        {
            if (!emitter.active) { continue; }
            bool holeCovered = Input.GetKey(emitter.linkedKey);
            emitter.enableParticles(!holeCovered);
            if (!holeCovered)
            {
                emitter.enableLetter();
                Vector3 force = -emitter.transform.forward.normalized * emitter.emissionForce;
                rb.AddForceAtPosition(force, emitter.transform.position);
            }
            else
            {
               emitter.disableLetter();
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SwapLetters();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            Destroy(collision.gameObject); // TODO: animate, remember to disable collider while it fades

            // TODO: there should only be one hit, but we should double check...
            ContactPoint2D contact = collision.contacts[0];

            Debug.DrawRay(contact.point, contact.normal, Color.green, 2, false);
            Vector2 point = contact.point;
            GameObject emitterGO = Instantiate(
                emitterPrefab,
                new Vector3(point.x, point.y, 0),
                Quaternion.LookRotation(-contact.normal),
                transform);
            Emitter emitter = emitterGO.GetComponent<Emitter>();
            emitter.setLinkedKey(collision.gameObject.GetComponent<Enemy>().linkedKey);
            emitter.emissionForce = enemy.emissionForce;
            emitters.Add(emitter.GetComponent<Emitter>());
        }
    }

    private void SwapLetters()
    {
        var letter1 = Random.Range(0,transform.Find("Emitters").childCount);
        var letter2 = Random.Range(0,transform.Find("Emitters").childCount);
        while(letter1 == letter2)
            letter2 = Random.Range(0, transform.Find("Emitters").childCount);
        //Aqui ya se decidio cuales
        Emitter em1 = transform.Find("Emitters").GetChild(letter1).GetComponent<Emitter>();
        KeyCode kc1 = em1.linkedKey;
        Emitter em2 = transform.Find("Emitters").GetChild(letter2).GetComponent<Emitter>();
        KeyCode kc2 = em2.linkedKey;

        AssignLeterToEmitter(em1, kc2);
        AssignLeterToEmitter(em2, kc1);

    }

    private void AssignLeterToEmitter(Emitter emitter, KeyCode kc)
    {
        emitter.linkedKey = kc;
        emitter.letter = GetKeyBox(kc).gameObject;
        emitter.letter.transform.position = transform.position + new Vector3(emitter.offset.x, emitter.offset.y);
    }

    private Transform GetKeyBox(KeyCode key)
    {
        string name = key.ToString();
        return transform.Find("Letters").Find(name);
    }
}
