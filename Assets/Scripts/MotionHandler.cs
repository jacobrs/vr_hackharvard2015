using UnityEngine;
using System.Collections;
using Leap;

public class MotionHandler : MonoBehaviour {
  Controller controller;
  public float target = 0f;
  public float smooth = 2.0f;

  void Start(){
    controller = new Controller();
    EnableGestures();
  }

  void EnableGestures(){
    controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
    controller.Config.SetFloat("Gesture.Swipe.MinLength", 100.0f);
    controller.Config.SetFloat("Gesture.Swipe.MinVelocity", 300f);
		//controller.EnableGesture(Gesture.GestureType.TYPEPINCH);
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
    }

		//evaluateHands(frame);
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
    rotateElem(magnitude * -100);
  }

  void TriggerRightSwipe(float magnitude){
    rotateElem(magnitude * -100);
  }

  void rotateElem(float angle){
    target += angle;
  }



}
