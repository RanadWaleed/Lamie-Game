using UnityEngine;
namespace Lamie.NativeStageDesign
{
    public sealed class NativeStageMotion : MonoBehaviour
    {
        public Transform[] clouds;
        public Transform[] lights;
        Vector3[] starts;
        void OnEnable()
        {
            starts = new Vector3[clouds.Length];
            for (int i=0;i<clouds.Length;i++) if(clouds[i]) starts[i]=clouds[i].localPosition;
        }
        void Update()
        {
            for(int i=0;i<clouds.Length;i++) if(clouds[i])
                clouds[i].localPosition=starts[i]+new Vector3(Mathf.Sin(Time.time*.09f+i)*1.2f, Mathf.Sin(Time.time*.2f+i)*.08f,0);
            for(int i=0;i<lights.Length;i++) if(lights[i])
                lights[i].localScale=Vector3.one*(.12f+Mathf.Sin(Time.time*1.8f+i)*.025f);
        }
    }
}
