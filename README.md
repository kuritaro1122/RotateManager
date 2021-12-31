# RotateManager
GameObjectの回転を制御する.
## Public Function

* RotateManager Set(Quaternion toRotation)
* RotateManager Set(Func<Quaternion> toRotation)
* RotateManager Set(Vector3 target, Func<Vector3> upwards = null, bool lockPitch = false)
* RotateManager Set(Func<Vector3> target, Func<Vector3> upwards = null, bool lockPitch = false)
* RotateManager Set(Transform target, Func<Vector3> upwards = null, bool lockPitch = false)

* void StartRotate(float duration, bool speedBase = false, TransitionType transitionType = TransitionType.FixedTimeAndKeep)
* void StartRotate_FixedTime(float duration, bool speedBase = false)
* void StartRotate_FixedTimeAndKeep(float duration, bool speedBase = false)
* StartRotate_KeepConstansSpeed(float speed)
  
* bool GetActive()
* bool GetFinished()
  
## Demo
```
GameObject self;
Transform target;
  
self.ComponentRotateManager()
  .Set(target, lockPitch:true)
  .StartRotate_KeepConstansSpeed(90f);
```
