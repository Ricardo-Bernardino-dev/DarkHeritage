﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public float damage;
	public float speed;
	public float accuracy;
	public GameObject muzzlePrefab;
	public GameObject hitPrefab;
	public AudioClip shotSFX;
	public AudioClip hitSFX;
	public List<GameObject> trails;
	private Vector3 offset;
	private bool collided;

	void Start() 
	{	

		if (accuracy != 100) {
			accuracy = 1 - (accuracy / 100);

			for (int i = 0; i < 2; i++) {
				var val = 1 * Random.Range(-accuracy, accuracy);
				var index = Random.Range(0, 2);
				if (i == 0) {
					if (index == 0)
						offset = new Vector3(0, -val, 0);
					else
						offset = new Vector3(0, val, 0);
				} else {
					if (index == 0)
						offset = new Vector3(0, offset.y, -val);
					else
						offset = new Vector3(0, offset.y, val);
				}
			}
		}

		if (muzzlePrefab != null) {
			var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
			muzzleVFX.transform.forward = gameObject.transform.forward + offset;
			var ps = muzzleVFX.GetComponent<ParticleSystem>();
			if (ps != null)
				Destroy(muzzleVFX, ps.main.duration);
			else {
				var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
				Destroy(muzzleVFX, psChild.main.duration);
			}
		}

		if (shotSFX != null && GetComponent<AudioSource>()) {
			GetComponent<AudioSource>().PlayOneShot(shotSFX);
		}
	}

	void Update() 
	{	
		if (speed != 0)
			transform.position += (transform.forward + offset) * (speed * Time.deltaTime);
	}

	void OnCollisionEnter(Collision co) 
	{
		if (!co.gameObject.CompareTag("Bullet") && !collided) {
			collided = true;
			
			if (trails.Count > 0) {
				for (int i = 0; i < trails.Count; i++) {
					if (trails[i].TryGetComponent<ParticleSystem>(out var ps)) 
					{
						ps.Stop();
					}
				}
			}
		
			speed = 0;
			GetComponent<Rigidbody>().isKinematic = true;

			ContactPoint contact = co.contacts[0];
			Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
			Vector3 pos = contact.point;

			if (hitPrefab != null) {
				var hitVFX = Instantiate(hitPrefab, pos, rot);
				hitVFX.GetComponent<AudioSource>().PlayOneShot(hitSFX);
				var ps = hitVFX.GetComponent<ParticleSystem>();
				if (ps == null) {
					var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
					Destroy(hitVFX, psChild.main.duration);
				} else
					Destroy(hitVFX, ps.main.duration);
			}

			StartCoroutine(DestroyParticle(0f));
		}

		if (co.gameObject.CompareTag("Enemy"))
		{
			co.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
		}

		if (co.gameObject.CompareTag("Boss"))
        {
            BossScript boss = co.gameObject.GetComponent<BossScript>();
            boss.TakeDamage(damage);
        }

		if (co.gameObject.CompareTag("Player"))
		{
			FindObjectOfType<BossScript>().stunPlayer = true;

		}
	}

	public IEnumerator DestroyParticle(float waitTime) 
	{

		yield return new WaitForSeconds(waitTime);
		Destroy(gameObject);
	}


}
