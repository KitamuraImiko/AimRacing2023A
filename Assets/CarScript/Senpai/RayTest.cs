using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTest : MonoBehaviour
{
	public float rayDistance = 20.0f;
	//レイの当たり判定を返す
	public bool RayCast()
	{
		Ray ray = new Ray(transform.position, transform.forward);

		RaycastHit hit;

		if (Physics.Raycast(transform.position, transform.forward * 10, out hit, rayDistance))
		{
			Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue, 0.1f);
			return true;
			//if (hit.collider.tag != "Player")
			//{
			//    Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue, 0.1f);
			//    return true;
			//}
		}

		//foreach (RaycastHit hitH in Physics.RaycastAll(ray))
		//{
		//    Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue, 0.1f);
		//    return true;
		//}

		Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 0.1f);

		return false;
	}
}
