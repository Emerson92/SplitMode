using System;
using System.Collections;
using System.Collections.Generic;
using TheDarkKnight.MeshBoxShell;
using UnityEngine;
using UnityEngine.UI;

public class DrawBox : MonoBehaviour
{

    public Vector3[] conernPoint;

    //public Bounds bounds;

    public GameObject GameObjectPool;

    public List<ModelBoxStruct> boundsPool = new List<ModelBoxStruct>();

    public Slider SplitSlider;

    private MeshFilter[] subGameobject;

    private Bounds totalBounds;

    private float offsetValue;

    // Start is called before the first frame update
    void Start()
    {
        SplitSlider.onValueChanged.AddListener(SplitModelProcess);
        ///Total bounds
        Vector3[] concer;
        totalBounds = MeshBoxShellFactory.GetInstance().CalculateBoxShell(GameObjectPool, out concer, false);
        offsetValue = Mathf.Sqrt(totalBounds.extents.x * totalBounds.extents.x + totalBounds.extents.y * totalBounds.extents.y + totalBounds.extents.z * totalBounds.extents.z);
        subGameobject = GameObjectPool.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < subGameobject.Length; i++)
        {

            ModelBoxStruct m = new ModelBoxStruct();
            m.name = subGameobject[i].name;
            m.bounds = MeshBoxShellFactory.GetInstance().CalculateBoxShell(subGameobject[i].gameObject, out m.ConcerPoint, false);
            
            m.Trans = subGameobject[i].transform;
            Vector3 dir = (m.bounds.center - totalBounds.center).normalized;
            m.OriginalPos = subGameobject[i].transform.localPosition;
            //m.TargetPos = subGameobject[i].transform.parent.InverseTransformPoint(offsetValue * dir + subGameobject[i].transform.position);
            //要考虑父节点的旋转角度问题
            m.TargetPos = subGameobject[i].transform.parent.InverseTransformVector(offsetValue * dir) + subGameobject[i].transform.localPosition ;
            boundsPool.Add(m);
        }
    }

    private void SplitModelProcess(float progress)
    {
        Debug.Log(progress);
        for (int i = 0; i < boundsPool.Count; i++)
        {
            boundsPool[i].Trans.localPosition = Vector3.Lerp(boundsPool[i].OriginalPos, boundsPool[i].TargetPos, progress);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GameObjectPool.transform.Rotate(Vector3.up, 0.1f);
    }

    public class ModelBoxStruct
    {

        public Vector3 OriginalPos;

        public Vector3 TargetPos;

        public Transform Trans;

        public string name;

        public Bounds bounds;

        public Vector3[] ConcerPoint = new Vector3[8];



    }


    public class Bound2D
    {

        public Vector2 center;

        /// <summary>
        /// 0 左上
        /// 1 右上
        /// 2 右下
        /// 3 左下
        ///  0 *********** 1
        ///  *             *
        ///  *   Bound2D   *  
        ///  *             *
        ///  *             *
        ///  3 *********** 2
        /// </summary>
        public Vector2[] concer = new Vector2[4];

        /// <summary>
        /// 是否包含这个点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contain(Vector2 point)
        {
            if (concer[0].x < point.x && point.x < concer[1].x && point.y > concer[0].y && point.y < concer[2].y)
            {
                return true;
            }
            return false;
        }

        public bool Contain(Bound2D bound)
        {
            int num = 0;
            for (int i = 0; i < bound.concer.Length; i++)
            {
                if (Contain(bound.concer[i]))
                {
                    num++;
                }
            }
            if (num == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 是否与当前包围盒相交
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public bool Intersects(Bound2D bound)
        {
            for (int i = 0; i < bound.concer.Length; i++)
            {
                if (Contain(bound.concer[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否与当前包围盒相交
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public bool Intersects(Bound2D bound, out int num)
        {
            num = 0;
            for (int i = 0; i < bound.concer.Length; i++)
            {
                if (Contain(bound.concer[i]))
                {
                    num++;
                }
            }
            if (num > 0) return true;
            return false;
        }

    }
}
