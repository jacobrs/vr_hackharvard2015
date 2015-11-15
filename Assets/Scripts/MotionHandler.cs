using UnityEngine;
using System.Collections;
using Leap;

public class MotionHandler : MonoBehaviour {
  Controller controller;
  public float target = 0f;
  public float smooth = 2.0f;
	public float pendingZoom = 0f;
  public float initialZoom = 0f;
  public float zoomAim = 0f;

  public static bool wasTapped;
  public static float timeSinceLastTap;

  void Start(){
    controller = new Controller();
    EnableGestures();
  }

  void EnableGestures(){
    controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
    controller.Config.SetFloat("Gesture.Swipe.MinLength", 100.0f);
    controller.Config.SetFloat("Gesture.Swipe.MinVelocity", 300f);
    controller.EnableGesture (Gesture.GestureType.TYPE_SCREEN_TAP);
    controller.Config.SetFloat("Gesture.ScreenTap.MinForwardVelocity", 10.0f);
    controller.Config.SetFloat("Gesture.ScreenTap.MinDistance", 1.0f);
    controller.Config.Save();
  }

  void Update(){
    Frame frame = controller.Frame();
    GestureList gestures = frame.Gestures();
    for(int i = 0; i < gestures.Count; i++){
      Gesture gesture = gestures[i];
      if(gesture.Type == Gesture.GestureType.TYPESWIPE){
        HandleSwipe(gesture);
      }
      else if(gesture.Type == Gesture.GestureType.TYPE_SCREEN_TAP){
        wasTapped = true;
        timeSinceLastTap = Time.timeSinceLevelLoad;
        if(CollisionHandler.lastCollision != Vector3.zero){
          CollisionHandler.pendingTagLocation = CollisionHandler.lastCollision;
          wasTapped = false;
        }
      }
    }
		EvaluateHands(frame);
    MoveTowardsTarget();

    if(Time.timeSinceLevelLoad - timeSinceLastTap >= 1 && wasTapped == true){
      wasTapped = false;
    }
	}

  void HandleSwipe(Gesture gesture){
    SwipeGesture Swipe = new SwipeGesture(gesture);
    Vector swipeDirection = Swipe.Direction;
    if(swipeDirection.x < 0){
      //TriggerLeftSwipe(swipeDirection.x);
    } else if(swipeDirection.x > 0){
      //TriggerRightSwipe(swipeDirection.x);
    }
  }

  void TriggerLeftSwipe(float magnitude){
    RotateElem(magnitude * -100);
  }

  void TriggerRightSwipe(float magnitude){
    RotateElem(magnitude * -100);
  }

  void RotateElem(float angle){
    target += angle;
  }

	void EvaluateHands(Frame frame){
		HandList hands = frame.Hands;
    if(hands.Count > 1 && BothHandsArePinching(hands)){

      Hand leftmostHand = hands.Leftmost;
			Hand rightmostHand = hands.Rightmost;
			Finger leftIndex = leftmostHand.Fingers[1];
			Finger rightIndex = rightmostHand.Fingers[1];

      float squaredXs =  Mathf.Pow((leftIndex.TipPosition.x - rightIndex.TipPosition.x), 2);
			float squaredYs =  Mathf.Pow((leftIndex.TipPosition.y - rightIndex.TipPosition.y), 2);

			pendingZoom = Mathf.Sqrt(squaredYs + squaredXs);

      if(initialZoom == 0){
        initialZoom = pendingZoom;
      }

      zoomAim -= (pendingZoom - initialZoom)/100;
		}else if(initialZoom != 0 && pendingZoom != 0){
			pendingZoom = 0;
      zoomAim = 0;
      initialZoom = 0;
		}
	}

	bool BothHandsArePinching(HandList hands){
		return hands[0].PinchStrength > .8 && hands[1].PinchStrength > .8;
	}

  private void MoveTowardsTarget() {
    //the speed, in units per second, we want to move towards the target
    float speed = 0.2f;
    //move towards the center of the world (or where ever you like)
    Vector3 targetPosition = new Vector3(
      GameObject.Find("WaterMolecule").transform.position.x ,
      GameObject.Find("WaterMolecule").transform.position.y ,
      Mathf.Max(-10.15f, GameObject.Find("WaterMolecule").transform.position.z + zoomAim));

    Vector3 currentPosition = this.transform.position;
    //first, check to see if we're close enough to the target
    if(Vector3.Distance(currentPosition, targetPosition) > .1f) {
        Vector3 directionOfTravel = targetPosition - currentPosition;
        //now normalize the direction, since we only want the direction information
        directionOfTravel.Normalize();
        //scale the movement on each axis by the directionOfTravel vector components

        this.transform.Translate(
            (directionOfTravel.x * speed * Time.deltaTime),
            (directionOfTravel.y * speed * Time.deltaTime),
            (directionOfTravel.z * speed * Time.deltaTime),
            Space.World);
    }
}

}
