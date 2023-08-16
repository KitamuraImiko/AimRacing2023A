using UnityEngine;
using System.Collections;
using System.Linq;

namespace AIM
{
    public partial class WheelController : MonoBehaviour
    {

        #region UnityDefault

        public void GetWorldPose(out Vector3 pos, out Quaternion quat)
        {
            pos = wheel.worldPosition;
            quat = wheel.worldRotation;
        }

        public float brakeTorque
        {
            get
            {
                return wheel.brakeTorque;
            }
            set
            {
                if (value >= 0)
                {
                    wheel.brakeTorque = value;
                }
                else
                {
                    wheel.brakeTorque = 0;
                }
            }
        }

        public bool isGrounded
        {
            get
            {
                if (hasHit)
                    return true;
                else
                    return false;
            }
        }

        public float mass
        {
            get
            {
                return wheel.mass;
            }
            set
            {
                wheel.mass = value;
            }
        }

        public float motorTorque
        {
            get
            {
                return wheel.motorTorque;
            }
            set
            {
                wheel.motorTorque = value;
            }
        }

        public float radius
        {
            get
            {
                return tireRadius;
            }
            set
            {
                tireRadius = value;
                InitializeScanParams();
            }
        }

        public float rimOffset
        {
            get
            {
                return wheel.rimOffset;
            }
            set
            {
                wheel.rimOffset = value;
            }
        }

        public float tireRadius
        {
            get
            {
                return wheel.tireRadius;
            }
            set
            {
                wheel.tireRadius = value;
                InitializeScanParams();
            }
        }

        public float tireWidth
        {
            get
            {
                return wheel.width;
            }
            set
            {
                wheel.width = value;
                InitializeScanParams();
            }
        }

        public float rpm
        {
            get
            {
                return wheel.rpm;
            }
        }

        public float steerAngle
        {
            get
            {
                return wheel.steerAngle;
            }
            set
            {
                wheel.steerAngle = value;
            }
        }

        public bool GetGroundHit(out WheelHit hit)
        {
            hit = this.wheelHit;
            return hasHit;
        }

        #endregion

        #region Geometry

        public float camber
        {
            get
            {
                return wheel.camberAngle;
            }
        }

        public void SetCamber(float camberAtTop, float camberAtBottom)
        {
            wheel.GenerateCamberCurve(camberAtTop, camberAtBottom);
        }

        public void SetCamber(float camber)
        {
            wheel.GenerateCamberCurve(camber, camber);
        }

        public void SetCamber(AnimationCurve curve)
        {
            wheel.camberCurve = curve;
        }

        #endregion

        #region Spring

        public float springCompression
        {
            get
            {
                return 1f - spring.compressionPercent;
            }
        }

        public float springVelocity
        {
            get
            {
                return spring.velocity;
            }
        }

        public bool springBottomedOut
        {
            get
            {
                return spring.bottomedOut;
            }
        }

        public bool springOverExtended
        {
            get
            {
                return spring.overExtended;
            }
        }

        public float suspensionForce
        {
            get
            {
                return spring.force;
            }
            set
            {
                spring.force = value;
            }
        }

        public float springMaximumForce
        {
            get
            {
                return spring.maxForce;
            }
            set
            {
                spring.maxForce = value;
            }
        }

        public AnimationCurve springCurve
        {
            get
            {
                return spring.forceCurve;
            }
            set
            {
                spring.forceCurve = value;
            }
        }

        public float springLength
        {
            get
            {
                return spring.maxLength;
            }
            set
            {
                spring.maxLength = value;
            }
        }

        public float springTravel
        {
            get
            {
                return spring.length;
            }
        }

        public Vector3 springTravelPoint
        {
            get
            {
                return transform.position - transform.up * spring.length;
            }
        }

        #endregion

        #region Damper

        public float damperForce
        {
            get
            {
                return damper.force;
            }
        }

        public float damperUnitReboundForce
        {
            get
            {
                return damper.unitReboundForce;
            }
            set
            {
                damper.unitReboundForce = value;
            }
        }

        public float damperUnitBumpForce
        {
            get
            {
                return damper.unitBumpForce;
            }
            set
            {
                damper.unitBumpForce = value;
            }
        }

        public AnimationCurve damperCurve
        {
            get
            {
                return damper.dampingCurve;
            }
            set
            {
                damper.dampingCurve = value;
            }
        }

        #endregion

        #region Friction

        public float dragForce
        {
            get
            {
                return wheel.dragForce;
            }
            set
            {
                wheel.dragForce = Mathf.Clamp(value, 0, Mathf.Infinity);
            }
        }

        public Friction forwardFriction
        {
            get
            {
                return fFriction;
            }
            set
            {
                fFriction = value;
            }
        }

        public Friction sideFriction
        {
            get
            {
                return sFriction;
            }
            set
            {
                sFriction = value;
            }
        }

        public float MaxPutDownForce
        {
            get
            {
                return maxPutDownForce;
            }
        }

        public void SetActiveFrictionPreset(FrictionPreset fp)
        {
            activeFrictionPresetEnum = (FrictionPreset.FrictionPresetEnum)System.Enum.Parse(typeof(FrictionPreset.FrictionPresetEnum), fp.name);
            activeFrictionPreset = fp;
        }

        public void SetActiveFrictionPreset(FrictionPreset.FrictionPresetEnum fpe)
        {
            activeFrictionPresetEnum = fpe;
            activeFrictionPreset = GetFrictionPreset((int)fpe);
        }

        public FrictionPreset GetFrictionPreset(int index)
        {
            return activeFrictionPreset = FrictionPreset.FrictionPresetList[index];
        }

        #endregion

        #region Misc

        public LRSide VehicleSide
        {
            get
            {
                return vehicleLRSide;
            }
            set
            {
                vehicleLRSide = value;
            }
        }

        public float speed
        {
            get
            {
                return fFriction.speed;
            }
        }

        public int ForwardScanResolution
        {
            get
            {
                return forwardScanResolution;
            }
            set
            {
                forwardScanResolution = value;

                if (forwardScanResolution < 1)
                {
                    forwardScanResolution = 1;
                }
                InitializeScanParams();
            }
        }

        public int SideToSideScanResolution
        {
            get
            {
                return sideToSideScanResolution;
            }
            set
            {
                sideToSideScanResolution = value;
                if (sideToSideScanResolution < 1)
                {
                    sideToSideScanResolution = 1;
                }
                InitializeScanParams();
            }
        }

        public GameObject Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        public GameObject Visual
        {
            get
            {
                return wheel.visual;
            }
            set
            {
                wheel.visual = value;
            }
        }

        public GameObject NonRotating
        {
            get
            {
                return wheel.nonRotating;
            }
            set
            {
                wheel.nonRotating = value;
            }
        }

        public Vector3 pointVelocity
        {
            get
            {
                return parentRigidbody.GetPointVelocity(wheel.worldPosition);
            }
        }

        public float angularVelocity
        {
            get
            {
                return wheel.angularVelocity;
            }
        }

        public LayerMask ScanIgnoreLayers
        {
            get
            {
                return scanIgnoreLayers;
            }

            set
            {
                scanIgnoreLayers = value;
            }
        }

        #endregion
    }
}
