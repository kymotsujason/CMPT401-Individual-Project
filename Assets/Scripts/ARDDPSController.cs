//-----------------------------------------------------------------------
// <copyright file="PointcloudVisualizer.cs" company="Google">
//
// Copyright 2018 Google. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.ARDDPS
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Rendering;
    using UnityEngine.UI;

#if UNITY_EDITOR
    using Input = InstantPreviewInput;
#endif
    /// <summary>
    /// Controls the ARDDPS example.
    /// </summary>
    public class ARDDPSController : MonoBehaviour
    {
        // should call if something is dropped in
        
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject TrackedPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject DronePrefab;
        public GameObject BuildingPrefab1;
        public GameObject BuildingPrefab2;

        public GameObject PlayButton;
        public GameObject ByeText;
        public GameObject ExitButton;

        /// <summary>
        /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject SearchingForPlaneUI;

        /// <summary>
        /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();

        /// <summary>
        /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        private bool droneInstantiated = false;

        public void Quit()
        {
            float timer = 2.0f;
            while (timer > 0)
            {
                
                timer -= Time.deltaTime;
            }
            Application.Quit();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            _QuitOnConnectionErrors();

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                if (!m_IsQuitting && Session.Status.IsValid())
                {
                    SearchingForPlaneUI.SetActive(true);
                }

                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
            }

            // Disable the snackbar UI when no planes are valid.
            Session.GetTrackables<TrackedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

            // On first touch, instantiate object
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            if (!droneInstantiated)
            {
                TrackableHit hit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                    TrackableHitFlags.FeaturePointWithSurfaceNormal;

                if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
                {
                    var droneObject = Instantiate(DronePrefab, hit.Pose.position, hit.Pose.rotation);
                    Vector3 buildingPos1 = hit.Pose.position;
                    buildingPos1.x += 1;
                    //buildingPos1.z += 1;
                    Vector3 buildingPos2 = hit.Pose.position;
                    buildingPos2.x -= 1;
                    //buildingPos2.z -= 1;
                    var buildingObject1 = Instantiate(BuildingPrefab1, buildingPos1, hit.Pose.rotation);
                    var buildingObject2 = Instantiate(BuildingPrefab2, buildingPos2, hit.Pose.rotation);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var droneAnchor = hit.Trackable.CreateAnchor(hit.Pose);
                    var buildingAnchor1 = hit.Trackable.CreateAnchor(hit.Pose);
                    var buildingAnchor2 = hit.Trackable.CreateAnchor(hit.Pose);

                    // drone should look at the camera but still be flush with the plane.
                    if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
                    {
                        // Get the camera position and match the y-component with the hit position.
                        Vector3 droneCameraPositionSameY = FirstPersonCamera.transform.position;
                        droneCameraPositionSameY.y = hit.Pose.position.y;

                        Vector3 buildingCameraPositionSameY = FirstPersonCamera.transform.position;
                        buildingCameraPositionSameY.y = hit.Pose.position.y;

                        // Have drone look toward the camera respecting his "up" perspective, which may be from ceiling.
                        droneObject.transform.LookAt(droneCameraPositionSameY, droneObject.transform.up);
                        buildingObject1.transform.LookAt(buildingCameraPositionSameY, droneObject.transform.up);
                        buildingObject2.transform.LookAt(buildingCameraPositionSameY, droneObject.transform.up);
                    }

                    // Make drone model a child of the anchor.
                    droneObject.transform.parent = droneAnchor.transform;
                    buildingObject1.transform.parent = buildingAnchor1.transform;
                    buildingObject2.transform.parent = buildingAnchor2.transform;

                    droneInstantiated = true;
                    PlayButton.SetActive(true);
                    ExitButton.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void _QuitOnConnectionErrors()
        {
            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
