using AirportCEOModLoader;
using AirportCEOModLoader.Core;
using HarmonyLib;
using Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace PerformanceCEO.NewUpdateSystem;

[HarmonyPatch]
public static class VehicleUpdateManager
{
    internal static List<VehicleController> vehiclesToMove = new List<VehicleController>();
    private static Dictionary<VehicleController, VehicleData> vehicleData = new Dictionary<VehicleController, VehicleData>();

	private static List<Vector3> positions = new List<Vector3>();
	private static List<Quaternion> rotations = new List<Quaternion>();
	private static List<int> indexesToRemove = new List<int>();
	private static List<int> indexesToPlaySound = new List<int>();

	private static ReaderWriterLockSlim pathfindLock = new ReaderWriterLockSlim();

	// This happens EVERY SINGLE FRAME and is very performance impactful
    internal static void VehicleUpdateManagerUpdate()
    {
		// Setup
		positions.Clear();
		rotations.Clear();
		indexesToRemove.Clear();
		indexesToPlaySound.Clear();
		if (positions.Capacity < vehiclesToMove.Count)
		{
			positions.Capacity = vehiclesToMove.Count;
		}
		if (rotations.Capacity < vehiclesToMove.Count)
		{
			rotations.Capacity = vehiclesToMove.Count;
		}

		// DataPull
		for (int i = 0; i < vehiclesToMove.Count; i++)
		{
			VehicleController controller = vehiclesToMove[i];
			VehicleData data = vehicleData[controller];

			if (controller == null || controller.transform == null)
			{
				indexesToRemove.Add(i);
				continue;
			}

			positions.Add(controller.transform.position);
			rotations.Add(controller.transform.rotation);
			data.distanceCheckingPos = data.distanceCheckingTransform.position;
			data.turningCheckingPos = data.turningCheckingTransform.position;

			controller.UpdateSpriteMaterials();
		}

		float delta = Time.deltaTime;

		// IN PARALEL: Calcs
		Parallel.For(0, vehiclesToMove.Count, (int i) =>
		{
			VehicleController controller = vehiclesToMove[i];
			VehicleData data = vehicleData[controller];
			if (!data.model.isOccupied || controller.forceBreakFollowPath)
			{
				controller.forceBreakFollowPath = false;
				indexesToRemove.Add(i);
				return; // this is equivalent to continue
			}
			if (controller.vehiclePathSmooth.turnBoundaries.Length > controller.targetIndex && controller.vehiclePathSmooth.turnBoundaries[controller.targetIndex].HasCrossedLine(data.turningCheckingPos))
			{
				if (controller.shouldCancelCurrentActivity && data.followingNodeBasedPath)
				{
					controller.cancellationAction(arg1: false, "new task assigned");
					indexesToRemove.Add(i);
					return; // this is equivalent to continue
				}
				if (controller.nextNode != null)
				{
					controller.nextNode.NodeUsage--;
				}
				if (controller.targetIndex == controller.vehiclePathSmooth.finishLineIndex)
				{
					controller.shouldDecelerate = false;
					controller.isDecelerating = false;
					if (Singleton<TimeController>.Instance.isAdvancingTime)
					{
						positions[i] = controller.targetPosition;
					}
					indexesToRemove.Add(i);
					return; // this is equivalent to continue
				}
				if (controller.targetIndex < controller.vehiclePathSmooth.turnBoundaries.Length)
				{
					controller.vehiclePathSmooth.turnBoundaries[controller.targetIndex].Passed();
				}
				controller.targetIndex++;
				controller.targetIndex = controller.targetIndex.Clamp(0, controller.vehiclePathSmooth.turnBoundaries.Length - 1);
				if (data.followingNodeBasedPath)
				{
					controller.CycleNodes(controller.targetIndex);
					controller.AdjustTopSpeedToVehiclesInFront();
				}
				controller.currentWaypoint = controller.vehiclePathSmooth.lookPoints[controller.targetIndex];
				if (controller.vehiclePathSmooth.nodes != null)
					{
					controller.Floor = controller.vehiclePathSmooth.nodes[controller.targetIndex].Floor;
				}
			}
			else if (controller.vehiclePathSmooth.turnBoundaries.Length <= controller.targetIndex)
			{
				indexesToRemove.Add(i);
				return; // this is equivalent to continue
			}
			float distance = Utils.GetDistance(controller.currentWaypoint, positions[i]);
			controller.shouldDecelerate = false;
			if (controller.targetIndex < controller.vehiclePathSmooth.lookPoints.Length - 1 && controller.targetIndex != 0)
			{
				if (data.model.direction == Enums.Direction.N || data.model.direction == Enums.Direction.S)
				{
					if (!Mathf.Approximately(controller.currentWaypoint.x, controller.vehiclePathSmooth.lookPoints[(controller.targetIndex + 1).Clamp(0, controller.vehiclePathSmooth.lookPoints.Length)].x))
					{
						controller.shouldDecelerate = true;
					}
				}
				else if ((data.model.direction == Enums.Direction.W || data.model.direction == Enums.Direction.E) && !Mathf.Approximately(controller.currentWaypoint.y, controller.vehiclePathSmooth.lookPoints[(controller.targetIndex + 1).Clamp(0, controller.vehiclePathSmooth.lookPoints.Length)].y))
				{
					controller.shouldDecelerate = true;
				}
			}
			if (!data.model.isOccupied)
			{
				indexesToRemove.Add(i);
				return; // this is equivalent to continue
			}
			if (data.followingNodeBasedPath && controller.targetIndex >= controller.vehiclePathSmooth.lookPoints.Length - 2)
			{
				controller.shouldDecelerate = true;
			}
			if (data.followingNodeBasedPath)
			{
				if (controller.nextNode != null && controller.nextNode.IsOccupied(data.model.referenceID, controller.isAllowedToOverrideNodeOccupancy))
				{
					if (!controller.pathDisruptedByVehicle)
					{
						VehicleController vehicleByReferenceID = Singleton<TrafficController>.Instance.GetVehicleByReferenceID<VehicleController>(controller.nextNode.GetOccupantReferenceID());
						PlaceableObject placeableObject = vehicleByReferenceID?.dependencyObject;
						if (vehicleByReferenceID == null || placeableObject == null)
						{
							controller.isAllowedToOverrideNodeOccupancy = true;
						}
						else
						{
							controller.isAllowedToOverrideNodeOccupancy = placeableObject != controller.dependencyObject || vehicleByReferenceID.nextNode == null || vehicleByReferenceID.nextNode.GetOccupantReferenceID().Equals(controller.ReferenceID);
						}
					}
					controller.pathDisruptedByVehicle = true;
					data.hornCounter++;
					if (data.hornCounter > 60 && Utils.ChanceOccured(0.01f))
					{
						data.hornCounter = 0;
						indexesToPlaySound.Add(i);
					}
				}
				else
				{
					if (controller.pathDisruptedByVehicle)
					{
						controller.isAllowedToOverrideNodeOccupancy = false;
					}
					controller.pathDisruptedByVehicle = false;
				}
				if (controller.vehiclePathSmooth == null || controller.vehiclePathSmooth.nodes == null || data.model == null)
				{
					controller.cancellationAction(arg1: true, "exception issue");
					indexesToRemove.Add(i);
					return; // this is equivalent to continue
				}
				if (controller.targetIndex < controller.vehiclePathSmooth.nodes.Length - controller.currentNbrOfNodeToOccupy)
				{
					for (int k = 0; k < controller.currentNbrOfNodeToOccupy; k++)
					{
						if (controller.vehiclePathSmooth.nodes[(controller.targetIndex + k).Clamp(0, controller.vehiclePathSmooth.nodes.Length)] == null)
						{
							controller.pathDisruptedByRoad = true;
							break;
						}
						controller.pathDisruptedByRoad = false;
					}
				}
				else
				{
					controller.pathDisruptedByRoad = false;
				}
			}
			if (!controller.pathDisruptedByVehicle && !controller.pathDisruptedByVehicle && controller.shouldDecelerate)
			{
				controller.isDecelerating = true;
				data.accelerationCounter = 0f;
				data.breakValueReference = 5f;
				switch (Singleton<TimeController>.Instance.GetCurrentTimeState())
				{
				case Enums.TimeState.Normal:
					data.breakForceValue = 0.25f;
					break;
				case Enums.TimeState.Double:
					data.breakForceValue = 1f;
					break;
				case Enums.TimeState.Triple:
					data.breakForceValue = 1.75f;
					break;
				}
				controller.currentSpeed = Mathf.Lerp(data.breakValueReference, controller.currentSpeed - data.breakForceValue, distance / controller.currentSpeed);
				if (controller.currentSpeed < controller.turnSpeed)
				{
					controller.currentSpeed = controller.turnSpeed;
				}
			}
			if (controller.pathDisruptedByRoad || controller.pathDisruptedByVehicle)
			{
				controller.isDecelerating = true;
				data.accelerationCounter = 0f;
				if (controller.pathDisruptedByRoad)
				{
					data.breakValueReference = 0f;
				}
				else if (controller.pathDisruptedByVehicle)
				{
					data.breakValueReference = controller.targetSpeed;
				}
				distance = Utils.GetDistance(controller.nextNode.worldPosition, positions[i]);
				switch (Singleton<TimeController>.Instance.GetCurrentTimeState())
				{
				case Enums.TimeState.Normal:
					data.breakForceValue = 0.5f;
					break;
				case Enums.TimeState.Double:
					data.breakForceValue = 
						5f;
					break;
				case Enums.TimeState.Triple:
					data.breakForceValue = 2f;
					break;
				}
				controller.currentSpeed = Mathf.Lerp(data.breakValueReference, controller.currentSpeed - data.breakForceValue, distance / controller.currentSpeed);
				if (controller.currentSpeed < 1f)
				{
					controller.currentSpeed = 0f;
					if (!controller.isStandingStill)
					{
						controller.isStandingStill = true;
						controller.timeStartedStandingStill = Singleton<TimeController>.Instance.GetCurrentContinuousTime();
					}
				}
				if (data.followingNodeBasedPath && controller.isStandingStill && (controller.pathDisruptedByRoad || controller.pathDisruptedByVehicle) && (controller.timeStartedStandingStill - Singleton<TimeController>.Instance.GetCurrentContinuousTime()).TotalMinutes <= -2.5)
				{
					controller.timeStartedStandingStill = Singleton<TimeController>.Instance.GetCurrentContinuousTime();
					if (controller.pathDisruptedByRoad)
					{
						try
						{
							pathfindLock.EnterWriteLock();
							controller.PerformPathRequest(controller.targetPosition);
						}
						finally
						{
							pathfindLock.ExitWriteLock();
						}
					}
					else
					{
						if (!controller.pathDisruptedByVehicle || controller.bypassAttemptCounter <= controller.bypassAttemptThereshold)
						{
							controller.bypassAttemptCounter++;
							return;
						}
						controller.bypassAttemptCounter = 0;
						if (!controller.nextNode.isOccupiedByPasser)
						{
							controller.vehicleInFront = Singleton<TrafficController>.Instance.GetVehicleByReferenceID<VehicleController>(controller.nextNode.GetOccupantReferenceID());
							if (controller.vehicleInFront != null)
							{
								VehicleController nextVehicleInFront = Singleton<TrafficController>.Instance.GetVehicleByReferenceID<VehicleController>(controller.vehicleInFront.nextNode?.GetOccupantReferenceID());
								while (nextVehicleInFront != null)
								{
									nextVehicleInFront = Singleton<TrafficController>.Instance.GetVehicleByReferenceID<VehicleController>(nextVehicleInFront?.nextNode?.GetOccupantReferenceID());
									if (!(nextVehicleInFront != null) || !(nextVehicleInFront != controller) || !(controller.vehicleInFront != nextVehicleInFront) || !nextVehicleInFront.isFollowingNodeBasedPath || !(controller.targetPosition != nextVehicleInFront.targetPosition) || !(GetCurrentPathDistance(controller, positions[i]) > GetCurrentPathDistance(nextVehicleInFront, positions[vehiclesToMove.IndexOf(nextVehicleInFront)])))
									{
										break;
									}
									controller.vehicleInFront = nextVehicleInFront;
									return;
								}
								if (controller.vehicleInFront != null && controller.vehicleInFront.isFollowingNodeBasedPath && controller.targetPosition != controller.vehicleInFront.targetPosition && GetCurrentPathDistance(controller, positions[i]) > GetCurrentPathDistance(controller.vehicleInFront, positions[vehiclesToMove.IndexOf(controller.vehicleInFront)]))
								{
									try
									{
										pathfindLock.EnterWriteLock();
										controller.AttemptFindPathAvoidingBlockingVehicle();
									}
									finally
									{
										pathfindLock.ExitWriteLock();
									}
								}
							}
						}
					}
					while (!controller.pathValidated)
					{
						return;
					}
					if (!controller.pathFound)
					{
						controller.cancellationAction(arg1: true, "path issue");
						indexesToRemove.Add(i);
						return; // this is equivalent to continue
					}
					return;
				}
			}
			else if (!controller.shouldDecelerate && !controller.pathDisruptedByRoad && !controller.pathDisruptedByVehicle && controller.currentSpeed < controller.targetSpeed)
			{
				controller.bypassAttemptCounter = 0;
				controller.isDecelerating = false;
				controller.isStandingStill = false;
				data.accelerationCounter += 0.01f;
				controller.currentSpeed = Mathf.Lerp(controller.currentSpeed, controller.targetSpeed, data.accelerationCounter / controller.targetSpeed);
			}

			positions[i] += (rotations[i] * Vector3.right) * delta * controller.currentSpeed;

			if (controller.currentSpeed > 0.1f && distance > 0.25f)
			{
				Vector3 vector = controller.vehiclePathSmooth.lookPoints[controller.targetIndex] - (Vector2)positions[i];
				float angle = (float)Math.Atan2(vector.y, vector.x) * 57.29578f;
				rotations[i] = Quaternion.Slerp(rotations[i], Quaternion.AngleAxis(angle, Vector3.forward), controller.currentSpeed * 0.75f * delta);
			}
			return;
		});


		// Set all positions and states
		for (int i = 0; i < vehiclesToMove.Count; i++)
		{
			if (vehiclesToMove[i] == null || vehiclesToMove[i].transform == null)
			{
				indexesToRemove.Add(i);
				continue;
			}
			vehiclesToMove[i].transform.position = positions[i];
			vehiclesToMove[i].transform.rotation = rotations[i];
			vehiclesToMove[i].UpdateAttachedTrailerPositioning();
		}

		foreach (int index in indexesToPlaySound)
		{
			vehiclesToMove[index].audioManager.PlayHornSound();
		}
		indexesToRemove.Sort();
		indexesToRemove.Reverse();
		foreach (int index in indexesToRemove)
		{
			vehiclesToMove.RemoveAt(index);
		}
    }

    private static void PrepVehicleForMoving(VehicleController controller, bool followingNodeBasedPath)
    {
		VehicleData data = new VehicleData();
		controller.ToggleDriver(true);
		data.accelerationCounter = 0f;
		data.breakValueReference = 0f;
		data.breakForceValue = 0f;
		data.hornCounter = 0;
		data.model = controller.vehicleModel;
		VehicleModel vehicleModel = controller.vehicleModel;
		data.followingNodeBasedPath = followingNodeBasedPath;
		if (!followingNodeBasedPath)
		{
			controller.currentSpeed = (vehicleModel.isReversing ? 3f : 7.5f);
			controller.targetSpeed = controller.currentSpeed;
			controller.isDecelerating = false;
			controller.shouldDecelerate = false;
			controller.pathDisruptedByVehicle = false;
			controller.pathDisruptedByVehicle = false;
		}
		else
		{
			controller.targetSpeed = (controller.isPatrolling ? (vehicleModel.topSpeed * 0.5f) : vehicleModel.topSpeed);
		}
		controller.originalTargetSpeed = controller.targetSpeed;
		controller.turnSpeed = controller.targetSpeed * 0.33f;
		controller.targetIndex = 0;
		if (controller.vehiclePathSmooth.lookPoints.Length == 0)
		{
			PopVehicle(controller);
		}
		controller.currentWaypoint = controller.vehiclePathSmooth.lookPoints[controller.targetIndex];
		controller.targetPosition = controller.vehiclePathSmooth.lookPoints[controller.vehiclePathSmooth.lookPoints.Length - 1];
		controller.isFollowingNodeBasedPath = followingNodeBasedPath;
		if (vehicleModel.HasAttachedTrailer)
		{
			controller.VerifyCurrentTrailerAttachment();
		}
		if (followingNodeBasedPath)
		{
			controller.CycleNodes(controller.targetIndex);
		}
		data.distanceCheckingTransform = controller.transform;
		data.turningCheckingTransform = controller.turnPosition;
		if (controller.checkDistanceWithFrontPosition)
		{
			data.distanceCheckingTransform = controller.frontPosition;
			data.turningCheckingTransform = controller.frontPosition;
		}
		controller.bypassAttemptCounter = 0;
		controller.bypassAttemptThereshold = Utils.RandomRangeI(0f, 3f);
		controller.isAllowedToOverrideNodeOccupancy = true;

		vehicleData[controller] = data;
	}

    private static void PopVehicle(VehicleController controller)
    {
        vehiclesToMove.Remove(controller);
    }

	public static float GetCurrentPathDistance(VehicleController controller, Vector2 pos)
	{
		float num = 0f;
		float num2 = float.MaxValue;
		int num3 = 0;
		if (controller.vehiclePathSmooth != null)
		{
			for (int i = 0; i < controller.vehiclePathSmooth.nodes.Length; i++)
			{
				float distance2D = Utils.GetDistance2D(pos, controller.vehiclePathSmooth.nodes[i].worldPosition);
				if (distance2D < num2)
				{
					num2 = distance2D;
					num3 = i;
				}
			}
			num += num2;
			RoadNode roadNode = null;
			for (int j = num3; j < controller.vehiclePathSmooth.nodes.Length; j++)
			{
				RoadNode roadNode2 = controller.vehiclePathSmooth.nodes[j];
				if (roadNode != null)
				{
					num += Utils.GetDistance2D(roadNode2.worldPosition, roadNode.worldPosition);
				}
				roadNode = roadNode2;
			}
		}
		return num;
	}

   // [HarmonyPrefix]
   // [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.FollowPath))]
   // static void StopCorutinePatch(VehicleController __instance, ref IEnumerator __result)
   // {
   //     try
   //     {
   //         __result = Bypass();
   //     }
   //     catch (Exception ex)
   //     {
   //         PerformanceCEO.LogError($"Failed to patch FollowPath coroutine. {ExceptionUtils.ProccessException(ex)}");
   //         return;
   //     }
   // }
   // [HarmonyPrefix]
   // [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.FollowPath), MethodType.Enumerator)]
   // static bool StopCorutinePatch2(VehicleController __instance)
   // {
   //     try
   //     {
			//VehicleController controller = Traverse.Create(__instance).Field<VehicleController>("<>4__this").Value;
			//if (controller == null)
			//{
			//	PerformanceCEO.LogInfo("they are in fact, all null");
			//}
   //         vehiclesToMove.Add(controller);
   //         PrepVehicleForMoving(controller, Traverse.Create(__instance).Field<bool>("followingNodeBasedPath").Value);

			//return false;
   //     }
   //     catch (Exception ex)
   //     {
   //         PerformanceCEO.LogError($"Failed to patch FollowPath coroutine. {ExceptionUtils.ProccessException(ex)}");
   //         return false;
   //     }
   // }


    public static System.Collections.IEnumerator Bypass()
    {
        yield break;
    }

}

public class VehicleData
{
    public float accelerationCounter;
	public float breakValueReference;
	public float breakForceValue;
	public int hornCounter;
	public VehicleModel model;
	public bool followingNodeBasedPath;
	public Transform distanceCheckingTransform;
	public Transform turningCheckingTransform;
	public Vector3 distanceCheckingPos;
	public Vector3 turningCheckingPos;

}