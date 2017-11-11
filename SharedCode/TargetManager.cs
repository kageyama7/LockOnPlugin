﻿using IllusionUtility.GetUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LockOnPlugin
{
    internal class CameraTargetManager
    {
        public const string MOVEMENTPOINT_NAME = "MovementPoint";

        private List<GameObject> allTargets = new List<GameObject>();
        private List<GameObject> normalTargets = new List<GameObject>();
        private List<CustomTarget> customTargets = new List<CustomTarget>();
        private CenterPoint centerPoint;
        private MovementPoint movementPoint;

        public List<GameObject> GetAllTargets() => allTargets;

        public void UpdateCustomTargetTransforms()
        {
            customTargets.ForEach(target => target.UpdateTransform());
            if(centerPoint != null && centerPoint.GetPoint()) centerPoint.UpdatePosition();
            if(movementPoint != null && movementPoint.GetPoint()) movementPoint.UpdatePosition();
        }

        public void UpdateAllTargets(CharInfo character)
        {
            if(character)
            {
                allTargets = normalTargets = UpdateNormalTargets(character);
                customTargets = UpdateCustomTargets(character);
                customTargets.ForEach(target => allTargets.Add(target.GetTarget()));
                centerPoint = new CenterPoint(character);
                if(centerPoint != null && centerPoint.GetPoint()) allTargets.Add(centerPoint.GetPoint());
                movementPoint = new MovementPoint(character);
                allTargets.Add(movementPoint.GetPoint());
            }
            else
            {
                allTargets = new List<GameObject>();
                normalTargets = new List<GameObject>();
                customTargets = new List<CustomTarget>();
                centerPoint = null;
                movementPoint = null;
            }
        }

        private List<GameObject> UpdateNormalTargets(CharInfo character)
        {
            List<GameObject> normalTargets = new List<GameObject>();
            string prefix = character is CharFemale ? "cf_" : "cm_";

            foreach(string targetName in FileManager.GetNormalTargetNames())
            {
                GameObject bone = character.chaBody.objBone.transform.FindLoop(prefix + targetName);
                if(bone) normalTargets.Add(bone);
            }

            return normalTargets;
        }

        private List<CustomTarget> UpdateCustomTargets(CharInfo character)
        {
            List<CustomTarget> customTargets = new List<CustomTarget>();
            string prefix = character is CharFemale ? "cf_" : "cm_";

            foreach(List<string> data in FileManager.GetCustomTargetNames())
            {
                GameObject point1 = character.chaBody.objBone.transform.FindLoop(prefix + data[1]);
                GameObject point2 = character.chaBody.objBone.transform.FindLoop(prefix + data[2]);

                foreach(CustomTarget target in customTargets)
                {
                    if(target.GetTarget().name == data[1])
                    {
                        point1 = target.GetTarget();
                    }

                    if(target.GetTarget().name == data[2])
                    {
                        point2 = target.GetTarget();
                    }
                }

                if(point1 && point2)
                {
                    float midpoint = 0.5f;
                    if(data.ElementAtOrDefault(3) != null)
                    {
                        if(!float.TryParse(data[3], out midpoint))
                        {
                            midpoint = 0.5f;
                        }
                    }

                    CustomTarget target = new CustomTarget(data[0], point1, point2, midpoint);
                    customTargets.Add(target);
                }
            }

            return customTargets;
        }

        private class CustomTarget
        {
            private GameObject target;
            private GameObject point1;
            private GameObject point2;
            private float midpoint;

            public CustomTarget(string name, GameObject point1, GameObject point2, float midpoint = 0.5f)
            {
                target = new GameObject(name);
                this.point1 = point1;
                this.point2 = point2;
                this.midpoint = midpoint;
                UpdateTransform();
            }

            public GameObject GetTarget()
            {
                return target;
            }

            public void UpdateTransform()
            {
                UpdatePosition();
                UpdateRotation();
            }

            private void UpdatePosition()
            {
                Vector3 pos1 = point1.transform.position;
                Vector3 pos2 = point2.transform.position;
                target.transform.position = Vector3.Lerp(pos1, pos2, midpoint);
            }

            private void UpdateRotation()
            {
                Quaternion rot1 = point1.transform.rotation;
                Quaternion rot2 = point2.transform.rotation;
                target.transform.rotation = Quaternion.Slerp(rot1, rot2, 0.5f);
            }
        }

        private class CenterPoint
        {
            private Dictionary<GameObject, float> points = new Dictionary<GameObject, float>();
            private GameObject point;

            public CenterPoint(CharInfo character)
            {
                string prefix = character is CharFemale ? "cf_" : "cm_";

                foreach(List<string> data in FileManager.GetCenterTargetWeights())
                {
                    GameObject point = character.chaBody.objBone.transform.FindLoop(prefix + data[0]);
                    float weight = 1f;
                    if(!float.TryParse(data[1], out weight))
                    {
                        weight = 1f;
                    }
                    points.Add(point, weight);
                }

                if(points.Count > 0)
                {
                    point = new GameObject("CenterPoint");
                    UpdatePosition();
                }
                else
                {
                    point = null;
                }
            }

            public GameObject GetPoint()
            {
                return point;
            }

            public void UpdatePosition()
            {
                point.transform.position = CalculateCenterPoint(points);
            }

            private Vector3 CalculateCenterPoint(Dictionary<GameObject, float> points)
            {
                Vector3 center = new Vector3();
                float totalWeight = 0f;
                foreach(KeyValuePair<GameObject, float> point in points)
                {
                    center += point.Key.transform.position * point.Value;
                    totalWeight += point.Value;
                }
                return center / totalWeight;
            }
        }
        
        private class MovementPoint
        {
            private CharInfo character;
            private GameObject point;

            public MovementPoint(CharInfo character)
            {
                this.character = character;
                point = new GameObject(MOVEMENTPOINT_NAME);
                UpdatePosition();
            }

            public GameObject GetPoint()
            {
                return point;
            }

            public void UpdatePosition()
            {
                point.transform.position = character.transform.position + new Vector3{ y = 1.3f };
                Console.WriteLine("test");
            }
        }
    }
}
