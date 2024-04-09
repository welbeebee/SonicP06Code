using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public enum Index
	{
		Jeep,
		Bike,
		Hover,
		Glider
	}

	
	public Index Type;

	public bool IsGetOut;

	public bool IsShoot;

	[Header("Prefab")]
	public GameObject[] Vehicles;

	private VehicleBase Target;

	public void SetParameters(int _Type, bool _IsGetOut, bool _IsShoot)
	{
		Type = (Index)(_Type - 1);
		IsGetOut = _IsGetOut;
		IsShoot = _IsShoot;
	}

	private void Awake()
	{
		SpawnVehicle();
	}

	private void Update()
	{
		if (!Target)
		{
			SpawnVehicle();
		}
	}

	private void SpawnVehicle()
	{
		Target = Object.Instantiate(Vehicles[(int)Type], base.transform.position, base.transform.rotation).GetComponent<VehicleBase>();
		Target.IsGetOut = IsGetOut;
		Target.IsShoot = IsShoot;
	}
}
