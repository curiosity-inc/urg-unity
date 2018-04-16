# urg-unity
URG a.k.a. Sokuiki sensor library for Unity written in pure C#. This library conforms to [SCIP 2.0 protocol]<https://www.hokuyo-aut.co.jp/dl/URG_SCIP20_1431403739.pdf>.

## Get started with your scene.
1. Import an unitypackage file which is found in Release page.
2. Attach `Scripts/UrgSensor` to your GameObject.
3. Refer `Example/DebugRenderer` to how to get distances.

## Limitation
This library currently only supports RS232 or USB communication, even if the latest version of sensors support Ethernet.

## License
MIT License.
