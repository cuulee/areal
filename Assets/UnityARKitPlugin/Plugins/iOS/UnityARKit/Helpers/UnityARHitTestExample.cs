using System;
using System.Collections.Generic;
using Lean.Touch;
using Mapbox.Examples;

namespace UnityEngine.XR.iOS
{
	public class UnityARHitTestExample : MonoBehaviour, PlaneAppearDetector
	{
		public Transform m_HitTransform;
		public float maxRayDistance = 30.0f;
		public LayerMask collisionLayer = 1 << 10;  //ARKitPlane layer
		public CanvasController ccontroller;
		public UnityARGeneratePlane generate_script;

		public GameObject pointCloud;

		private GameObject MAP;


		private bool mapWasShown = false;
		private bool planeAppeared = false;
		private GameObject planeObj;
		private SpawnOnMap spawnScript;

		private static int SHOW_MAP_ANIM = 1;
		private static int HIDE_MAP_ANIM = 2;


        bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
			if (hitResults.Count > 0 && !mapWasShown && planeAppeared ) {
				mapWasShown = true;

                foreach (var hitResult in hitResults) {
                    m_HitTransform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
                    m_HitTransform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
					m_HitTransform.localScale = new Vector3 (0.08f, 0.08f, 0.08f);

					Transform map;
					for (int i = 0; i < m_HitTransform.childCount; i++) {
						map = m_HitTransform.GetChild (i);
						if (map.name == "Map") {
							MAP = map.gameObject;
							MAP.GetComponent<Animator>().SetInteger("mapAnimTransition",SHOW_MAP_ANIM);
							spawnScript = MAP.GetComponent<SpawnOnMap> ();
						//	switchCloud(false);
							m_HitTransform.gameObject.GetComponent<LeanScale>().enabled = true;
							

						}
					}
					generate_script.getManager ().HidePrefabs ();
						


					if(ccontroller.about_map_Panel.activeInHierarchy) //check if panel was not closed manually, so next panel will be shown by event from animation clip
						ccontroller.hide_about_map_text (false);
					else
						ccontroller.show_about_pins(); //else - show next panel from code patently
					ccontroller.show_reload_btn ();
					ccontroller.show_screenShot_btn ();
					return true;
                }
            }
            return false;
        }
		
		// Update is called once per frame
		void Update () {
			#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				
				//we'll try to hit one of the plane collider gameobjects that were generated by the plugin
				//effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
				if (Physics.Raycast (ray, out hit, maxRayDistance, collisionLayer)) {
					//we're going to get the position from the contact point
					m_HitTransform.position = hit.point;
//					Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

					//and the rotation from the transform of the plane collider
					m_HitTransform.rotation = hit.transform.rotation;
				}
			}
			#else
			if (Input.touchCount > 0 && m_HitTransform != null)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
						//ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    }; 
					
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
				}
			}
			#endif

		}

		void PlaneAppearDetector.planeDetect(){
			planeAppeared = true;
			switchCloud(false);
		}

		public void reload_map(){
			mapWasShown = false;
			m_HitTransform.localScale = new Vector3 (0, 0, 0);
			MAP.GetComponent<Animator>().SetInteger("mapAnimTransition",0);
// 			for (int i = 0; i < m_HitTransform.childCount; i++) {
// 				Transform child = m_HitTransform.GetChild (i);
// 				if (child.name == "Map") {
					
// //					child.localScale = new Vector3(0,0,0);
// //					child.gameObject.SetActive (true);
// 				}
// 			}
			
			switchCloud(true);
			ccontroller.hide_back_Button ();
			ccontroller.hide_reload_btn ();
			ccontroller.hide_about_model ();
			ccontroller.hide_about_map_text (false);
			ccontroller.hide_about_Isaac_info ();
			ccontroller.hide_info_btn ();
		}
			

		private void switchCloud(bool value){
				UnityPointCloudExample cloud = pointCloud.GetComponent<UnityPointCloudExample>();
				cloud.setCloudWorks(false);
				List<GameObject> list = cloud.getCloud();
				foreach (GameObject ob in list){
					float v = value ? 0.002f : 0;
					ob.transform.localScale = new Vector3(v,v,v);
				}
		}
	}
}

