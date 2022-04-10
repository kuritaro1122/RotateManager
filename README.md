# RotateManager
GameObjectの回転を制御する。回転にかかる速度や時間を自由に制御可能。

# Demo
![gif](/img/demo.gif/)

# Requirement
* UnityEngine;
* System;

# Usage
① 任意のGameObjectにRotateManagerをコンポーネント\
② Set関数で目標の回転を設定\
③ StartRotate関数で値を設定して回転開始
```
Transform target;
GameObject self;
RotateManager manager = self.ComponentRotateManager();

manager.Set(target: target, lockPitch: true)
    .StartRotate_FixedTime(duration: 1f);
```

# Contains

## Public Function
```
RotateManager Set(Quaternion toRotation) // 静的な回転
RotateManager Set(System.Func<Quaternion> toRotation) // 動的な回転
RotateManager Set(Vector3 target, System.Func<Vector3> upwards = null, bool lockPitch = false) // ターゲットを向く（静的な座標）
RotateManager Set(System.Func<Vector3> target, System.Func<Vector3> upwards = null, bool lockPitch = false) // ターゲットを向く（動的な座標）
RotateManager Set(Transform target, System.Func<Vector3> upwards = null, bool lockPitch = false) // ターゲットを向く（Transform）

void StartRotate(float duration, bool speedBase = false, TransitionType transitionType = TransitionType.FixedTimeAndKeep)
void StartRotate_FixedTime(float duration, bool speedBase = false) // 固定時間かけて回転する
void StartRotate_FixedTimeAndKeep(float duration, bool speedBase = false) // 固定時間かけて回転し、ターゲットを向き続ける
void StartRotate_KeepConstansSpeed(float speed) // 一定速度で目標に向けて回転し続ける

bool GetActive()
bool GetFinished()
```

## Extension method
```
RotateManager ComponentRotateManager(this GameObject self)
```

# Note
* StartRotate()のspeedBaseをtrueにすると処理が重くなるため、falseを推奨します。
* ターゲットを向かせる時には、upwardsを設定した方が挙動が安定しやすいです。

# License
"RotateManager" is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).
