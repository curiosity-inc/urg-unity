# urg-unity

URG a.k.a. Sokuiki sensor (測域センサー) library for Unity written in pure C#. This library conforms to [SCIP 2.0 protocol]<https://www.hokuyo-aut.co.jp/dl/URG_SCIP20_1431403739.pdf>.

## Install

Import an unitypackage file found in Release page, or use npm (Node Package Manager) and unity3d-package-syncer module to install this library.

## Get started with your scene.

1 (a) If you can find your sensor name under `Prefabs` folder, just drag'drop it to your scene.

1 (b) If you cannot find existing prefab, you can attach necessary componenets to utilize your sensor. Attach `Scripts/UrgSensor` and `Scripts/Transport/EthernetTransport` (If your sensor is compatible with Ethernet) or `Scripts/Transport/SerialTransport` (If your sensor is compatible with RS232 over USB) to your GameObject.

2 Refer `Example/DebugRenderer` to how to get distances (The main part is copied and pasted below).
```
        void Awake()
        {
            urg.OnDistanceReceived += Urg_OnDistanceReceived;
        }

        void Update()
        {
            if (urg != null)
            {
                if (distances != null && distances.Length > 0)
                {
                    for (int i = 0; i < distances.Length; i++)
                    {
                        float distance = distances[i];
                        float angle = urg.StepAngleRadians * i + urg.OffsetRadians;
                        var cos = Mathf.Cos(angle);
                        var sin = Mathf.Sin(angle);
                        var dir = new Vector3(cos, sin, 0);
                        var pos = distance * dir;

                        Debug.DrawRay(urg.transform.position, pos, Color.blue);
                    }
                }
            }
        }

        private void Urg_OnDistanceReceived(float[] distances)
        {
            // this.distances = distances;
            this.distances = Utils.MedianFilter<float>(distances);
        }
```

## License

MIT License.
