/****************************************************
 *             获取模型外包围圈盒子(基于MeshFilter)
 * Chinese ：
 *      根据模型的父节点，遍历其下面的子节点，获取每个子节点
 * 的包围盒子，最终得到物体整个的包围盒子。
 * English：
 *      Accooding to the Parent of Mode node,we can get the All
 * the children box shell from the parent node.calculating all
 * childern box shell so that we can get the final entire box 
 * Shell.
 *                                -----TheDarkKnight
 * ***************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheDarkKnight.MeshBoxShell
{

    public class MeshBoxShellFactory
    {

        static MeshBoxShellFactory MeshBox;

        //private Vector3 CenterPo;

        private Vector3[] corners = new Vector3[8];

        private MeshBoxShellFactory() { }

        private Vector3 CenterWorldPos;

        /// <summary>
        /// 单例工厂
        /// </summary>
        /// <returns></returns>
        public static MeshBoxShellFactory GetInstance()
        {
            if (MeshBox == null)
                return MeshBox = new MeshBoxShellFactory();
            else
                return MeshBox;
        }

        /// <summary>
        /// 计算模型的包围盒子
        /// </summary>
        /// <param name="parent">父节点模型</param>
        public Bounds CalculateBoxShell(GameObject parent, out Vector3[] conernPoint,bool isDebug = true)
        {
            conernPoint = corners;
            Bounds boxShell = new Bounds(parent.transform.position, Vector3.zero);
            List<Vector3> boundsList = CaculateBoxShellCornersPoint(parent.GetComponentsInChildren<MeshFilter>());
            if (boundsList.Count > 0)
            {
                // We now have a list of all points in world space
                // Translate them all to =  local space
                for (int i = 0 ; i < boundsList.Count ; i++)
                {
                    boundsList[i] = parent.transform.InverseTransformPoint(boundsList[i]);
                }
                // Encapsulate the points with a local bounds
                boxShell.center = boundsList[0];
                boxShell.size = Vector3.zero;
                foreach (Vector3 point in boundsList)
                {
                    boxShell.Encapsulate(point);
                }
                conernPoint = GetBoxCornerPositions(boxShell, parent, isDebug);
            }
            boxShell.center = (conernPoint[7] + conernPoint[0]) / 2;
            boxShell.size = new Vector3((conernPoint[7].x- conernPoint[0].x), (conernPoint[7].y - conernPoint[0].y) , (conernPoint[7].z - conernPoint[0].z));
            CenterWorldPos = GetCenterWorldPosition(corners);
            return boxShell;
        }

        private Vector3[] GetBoxCornerPositions(Bounds box, GameObject parent, bool isDebug)
        {
            Vector3 v3Center = box.center;
            Vector3 v3Extents = box.extents;
            Color DrawLine = Color.green;

            Vector3 v3FrontTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
            Vector3 v3FrontTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner

            Vector3 v3FrontBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
            Vector3 v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner

            Vector3 v3BackTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
            Vector3 v3BackTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner

            Vector3 v3BackBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
            Vector3 v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

            Vector3[] cornPoint = new Vector3[8];

            //Debug.Log("boundsList count :" + boundsList.Count);
            cornPoint[0] = parent.transform.TransformPoint(v3FrontTopLeft);
            cornPoint[1] = parent.transform.TransformPoint(v3FrontTopRight);

            cornPoint[2] = parent.transform.TransformPoint(v3FrontBottomLeft);
            cornPoint[3] = parent.transform.TransformPoint(v3FrontBottomRight);

            cornPoint[4] = parent.transform.TransformPoint(v3BackTopLeft);
            cornPoint[5] = parent.transform.TransformPoint(v3BackTopRight);

            cornPoint[6] = parent.transform.TransformPoint(v3BackBottomLeft);
            cornPoint[7] = parent.transform.TransformPoint(v3BackBottomRight);

            if (isDebug) {

                Debug.DrawLine(cornPoint[0], cornPoint[1], DrawLine, 10000);
                Debug.DrawLine(cornPoint[1], cornPoint[3], DrawLine, 10000);
                Debug.DrawLine(cornPoint[3], cornPoint[2], DrawLine, 10000);
                Debug.DrawLine(cornPoint[2], cornPoint[0], DrawLine, 10000);

                Debug.DrawLine(cornPoint[4], cornPoint[5], DrawLine, 10000);
                Debug.DrawLine(cornPoint[5], cornPoint[7], DrawLine, 10000);
                Debug.DrawLine(cornPoint[7], cornPoint[6], DrawLine, 10000);
                Debug.DrawLine(cornPoint[6], cornPoint[4], DrawLine, 10000);

                Debug.DrawLine(cornPoint[0], cornPoint[4], DrawLine, 10000);
                Debug.DrawLine(cornPoint[1], cornPoint[5], DrawLine, 10000);
                Debug.DrawLine(cornPoint[3], cornPoint[7], DrawLine, 10000);
                Debug.DrawLine(cornPoint[2], cornPoint[6], DrawLine, 10000);
            }
            return cornPoint;
        }

        /// <summary>
        /// 获取边框定点的位置坐标
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetCorners()
        {
            return corners;
        }

        /// <summary>
        /// 计算包围盒子的八个角的位置
        /// </summary>
        /// <param name="meshFilters">各个子模型的MeshFilter</param>
        /// <returns></returns>
        private List<Vector3> CaculateBoxShellCornersPoint(MeshFilter[] meshFilters)
        {
            List<Vector3> boundsList = new List<Vector3>();
            foreach (MeshFilter mf in meshFilters)
            {
                GetCornerPositions(mf.sharedMesh.bounds, mf.transform);
                boundsList.AddRange(corners);
            }
            return boundsList;
        }

        private void GetCornerPositions(Bounds bounds, Transform transform)
        {
            // Calculate the local points to transform.
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            float leftEdge = center.x - extents.x;
            float rightEdge = center.x + extents.x;
            float bottomEdge = center.y - extents.y;
            float topEdge = center.y + extents.y;
            float frontEdge = center.z - extents.z;
            float backEdge = center.z + extents.z;


            // Transform all the local points to world space.
            corners[0] = transform.TransformPoint(leftEdge, bottomEdge, frontEdge);//左下前
            corners[1] = transform.TransformPoint(leftEdge, bottomEdge, backEdge);//左下后
            corners[2] = transform.TransformPoint(leftEdge, topEdge, frontEdge);//左上前
            corners[3] = transform.TransformPoint(leftEdge, topEdge, backEdge);//左上后
            corners[4] = transform.TransformPoint(rightEdge, bottomEdge, frontEdge);//右下前
            corners[5] = transform.TransformPoint(rightEdge, bottomEdge, backEdge);//右下后
            corners[6] = transform.TransformPoint(rightEdge, topEdge, frontEdge);//右上前
            corners[7] = transform.TransformPoint(rightEdge, topEdge, backEdge);//右上后

        }

        public Vector3 GetCenterWorldPosition() {
            return CenterWorldPos;
        }

        public Vector3 GetCenterWorldPosition(Vector3[] cornPoints) {
            if (cornPoints.Length < 8) {
                Debug.LogError("Vector3 array Length is not right,at last lengt large 8");
                return Vector3.zero;
            }
               
            return new Vector3(((cornPoints[0].x+ cornPoints[7].x)/2f),((cornPoints[0].y + cornPoints[7].y) / 2f), ((cornPoints[0].z + cornPoints[7].z) / 2f));
        }


        /// <summary>
        /// 在Editor视图中，绘画包围盒子
        /// </summary>
        /// <param name="localTargetBounds"></param>
        /// <param name="parent"></param>
        /// <param name="IsDebug"></param>
        public void DrawBox(Bounds localTargetBounds, GameObject parent, Color DrawLine, bool IsDebug = false)
        {
            DrawLine = (DrawLine == null ? Color.green : DrawLine);
            Vector3 v3Center = localTargetBounds.center;
            Vector3 v3Extents = localTargetBounds.extents;

            Vector3 v3FrontTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
            Vector3 v3FrontTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner

            Vector3 v3FrontBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
            Vector3 v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner

            Vector3 v3BackTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
            Vector3 v3BackTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner

            Vector3 v3BackBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
            Vector3 v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

            Vector3[] cornPoint = new Vector3[8];

            //Debug.Log("boundsList count :" + boundsList.Count);
            cornPoint[0] = parent.transform.TransformPoint(v3FrontTopLeft);
            cornPoint[1] = parent.transform.TransformPoint(v3FrontTopRight);

            cornPoint[2] = parent.transform.TransformPoint(v3FrontBottomLeft);
            cornPoint[3] = parent.transform.TransformPoint(v3FrontBottomRight);

            cornPoint[4] = parent.transform.TransformPoint(v3BackTopLeft);
            cornPoint[5] = parent.transform.TransformPoint(v3BackTopRight);

            cornPoint[6] = parent.transform.TransformPoint(v3BackBottomLeft);
            cornPoint[7] = parent.transform.TransformPoint(v3BackBottomRight);

            if (IsDebug)
            {
                Debug.Log("Center : x" + v3Center.x + " y " + v3Center.y + " z "+ v3Center.z);
                Debug.Log("corners[0]  x:" + cornPoint[0].x + " y :" + cornPoint[0].y + " z :" + cornPoint[0].z);
                Debug.Log("corners[1]  x:" + cornPoint[1].x + " y :" + cornPoint[1].y + " z :" + cornPoint[1].z);
                Debug.Log("corners[2]  x:" + cornPoint[2].x + " y :" + cornPoint[2].y + " z :" + cornPoint[2].z);
                Debug.Log("corners[3]  x:" + cornPoint[3].x + " y :" + cornPoint[3].y + " z :" + cornPoint[3].z);
                Debug.Log("corners[4]  x:" + cornPoint[4].x + " y :" + cornPoint[4].y + " z :" + cornPoint[4].z);
                Debug.Log("corners[5]  x:" + cornPoint[5].x + " y :" + cornPoint[5].y + " z :" + cornPoint[5].z);
                Debug.Log("corners[6]  x:" + cornPoint[6].x + " y :" + cornPoint[6].y + " z :" + cornPoint[6].z);
                Debug.Log("corners[7]  x:" + cornPoint[7].x + " y :" + cornPoint[7].y + " z :" + cornPoint[7].z);
            }


            Debug.DrawLine(cornPoint[0], cornPoint[1], DrawLine,10000);
            Debug.DrawLine(cornPoint[1], cornPoint[3], DrawLine,10000);
            Debug.DrawLine(cornPoint[3], cornPoint[2], DrawLine,10000);
            Debug.DrawLine(cornPoint[2], cornPoint[0], DrawLine, 10000);

            Debug.DrawLine(cornPoint[4], cornPoint[5], DrawLine, 10000);
            Debug.DrawLine(cornPoint[5], cornPoint[7], DrawLine, 10000);
            Debug.DrawLine(cornPoint[7], cornPoint[6], DrawLine, 10000);
            Debug.DrawLine(cornPoint[6], cornPoint[4], DrawLine, 10000);

            Debug.DrawLine(cornPoint[0], cornPoint[4], DrawLine, 10000);
            Debug.DrawLine(cornPoint[1], cornPoint[5], DrawLine, 10000);
            Debug.DrawLine(cornPoint[3], cornPoint[7], DrawLine, 10000);
            Debug.DrawLine(cornPoint[2], cornPoint[6], DrawLine, 10000);
        }
    }
}
