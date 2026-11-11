using UnityEngine;
namespace Lamie.AssetStagePreview
{
 public sealed class AssetStageAtmosphere:MonoBehaviour
 {
  public Transform[] clouds; public float[] speeds; Vector3[] origins;
  void OnEnable(){origins=new Vector3[clouds.Length];for(int i=0;i<clouds.Length;i++)if(clouds[i])origins[i]=clouds[i].localPosition;}
  void Update(){for(int i=0;i<clouds.Length;i++)if(clouds[i])clouds[i].localPosition=origins[i]+new Vector3(Mathf.Sin(Time.timeSinceLevelLoad*speeds[i])*.85f,Mathf.Sin(Time.timeSinceLevelLoad*.13f+i)*.04f,0);}
 }
}
