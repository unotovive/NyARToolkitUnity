using UnityEngine;
using System;
using System.Collections;
using NyARUnityUtils;
using jp.nyatla.nyartoolkit.cs.markersystem;
using jp.nyatla.nyartoolkit.cs.core;

namespace NyARUnityUtils
{
	public class NyARUnityMarkerSystem:NyARMarkerSystem
	{
		public NyARUnityMarkerSystem(INyARMarkerSystemConfig i_config):base(i_config)
		{
		}
		protected override void initInstance(INyARMarkerSystemConfig i_config)
		{
			base.initInstance(i_config);		
			this._projection_mat=new Matrix4x4();
		}
	
		private Matrix4x4 _projection_mat;
	
		/**
		 * OpenGLスタイルのProjectionMatrixを返します。
		 * @param i_gl
		 * @return
		 * [readonly]
		 */
		public Matrix4x4 getUnityProjectionMatrix()
		{
			return this._projection_mat;
		}
		public override void setProjectionMatrixClipping(double i_near,double i_far)
		{
			base.setProjectionMatrixClipping(i_near,i_far);
			NyARUnityUtil.toCameraFrustumRH(this._ref_param,1,i_near,i_far,ref this._projection_mat);
			
		}
		/**
		 * この関数はUnityMatrix形式の姿勢変換行列を返します.
		 * @param i_buf
		 * @return
		 * 値を格納したMatrix44オブジェクト
		 */
		public Matrix4x4 getUnityMarkerMatrix(int i_id)
		{
			Matrix4x4 mat=new Matrix4x4();
			return this.getUnityMarkerMatrix(i_id,ref mat);
		}
		/**
		 * この関数は、i_bufに指定idのOpenGL形式の姿勢変換行列を設定して返します。
		 * @param i_id
		 * @param i_buf
		 * @return
		 */
		public Matrix4x4 getUnityMarkerMatrix(int i_id,ref Matrix4x4 i_buf)
		{
			NyARUnityUtil.toCameraViewRH(this.getMarkerMatrix(i_id),1,ref i_buf);
			return i_buf;
		}
		/// <summary>
		/// Gets the unity marker transform.
		/// </summary>
		/// <param name='i_id'>
		/// I_id.
		/// </param>
		/// <param name='o_pos'>
		/// O_pos.
		/// </param>
		/// <param name='o_rotation'>
		/// O_rotation.
		/// </param>
		public void getUnityMarkerTransform(int i_id,ref Vector3 o_pos,ref Quaternion o_rotation)
		{
			NyARUnityUtil.toCameraViewRH(this.getMarkerMatrix(i_id),1,ref o_pos,ref o_rotation);
		}
		/// <summary>
		/// Sets marker matrix to unity transform
		/// </summary>
		/// <param name='i_id'>
		/// I_id.
		/// </param>
		/// <param name='i_t'>
		/// I_t.
		/// </param>
		public void setUnityMarkerTransform(int i_id,Transform i_t)
		{
			Vector3 p=new Vector3();
			Quaternion r=new Quaternion();
			NyARUnityUtil.toCameraViewRH(this.getMarkerMatrix(i_id),1,ref p,ref r);
			i_t.position=p;
			i_t.rotation=r;
		}
		public void setUnityMarkerTransform(int i_id,GameObject i_go)
		{
			this.setUnityMarkerTransform(i_id,i_go.transform);
		}		
		
		/// <summary>
		/// この関数は,cameraオブジェクトにProjectionMatrixを指定します.
		/// </summary>
		public void setARCameraProjection(Camera i_camera)
		{
			NyARFrustum f=this.getFrustum();
			NyARFrustum.PerspectiveParam pp=f.getPerspectiveParam(new NyARFrustum.PerspectiveParam());
			//setup camera projection
			i_camera.nearClipPlane=(float)pp.near;
			i_camera.farClipPlane=(float)pp.far;
			i_camera.fieldOfView=(float)(360*pp.fovy/(2*Math.PI));
			i_camera.aspect=(float)(pp.aspect);
			i_camera.transform.LookAt(new Vector3(0,0,0),new Vector3(1,0,0));
		}
		/// <summary>
		/// この関数は,背景画像の姿勢行列をtransformメンバに設定します.
		/// </summary>
		public void setARBackgroundTransform(Transform i_transform)
		{
			NyARFrustum f=this.getFrustum();
			NyARFrustum.FrustumParam fp=f.getFrustumParam(new NyARFrustum.FrustumParam());
			float bg_pos=(float)fp.far;
			i_transform.position=new Vector3(0,0,(float)bg_pos);
			double b=bg_pos/fp.near/10;// 10?
			i_transform.localScale=new Vector3((float)(-(fp.right-fp.left)*b),1f,-(float)((fp.top-fp.bottom)*b));
			i_transform.eulerAngles=new Vector3(-90,0,0);
		}
		
		/// <summary>
		/// This is based on http://marupeke296.com/DXG_No58_RotQuaternionTrans.html
		/// </summary>
		/// <returns>
		/// The rot mat to quaternion2.
		/// </returns>
		/// <param name='i_mat'>
		/// I_mat.
		/// </param>
		private static Quaternion transformRotMatToQuaternion(ref Matrix4x4 i_mat)
		{
		    // 最大成分を検索
		    double elem0 = i_mat.m00 - i_mat.m11 - i_mat.m22 + 1.0f;
		    double elem1 = -i_mat.m00 + i_mat.m11 - i_mat.m22 + 1.0f;
		    double elem2 = -i_mat.m00 - i_mat.m11 + i_mat.m22 + 1.0f;
		    double elem3 = i_mat.m00 + i_mat.m11 + i_mat.m22 + 1.0f;
			Quaternion q=new Quaternion();
			if(elem0>elem1 && elem0>elem2 && elem0>elem3){
			    double v = Math.Sqrt(elem0) * 0.5f;
			    double mult = 0.25f / v;
				q.x = (float)v;
		        q.y = (float)((i_mat.m10 + i_mat.m01) * mult);
		        q.z = (float)((i_mat.m02 + i_mat.m20) * mult);
		        q.w = (float)((i_mat.m21 - i_mat.m12) * mult);
			}else if(elem1>elem2 && elem1>elem3){
			    double v = Math.Sqrt(elem1) * 0.5f;
			    double mult = 0.25f / v;
		        q.x = (float)((i_mat.m10 + i_mat.m01) * mult);
				q.y = (float)(v);
		        q.z = (float)((i_mat.m21 + i_mat.m12) * mult);
		        q.w = (float)((i_mat.m02 - i_mat.m20) * mult);
			}else if(elem2>elem3){
			    double v = Math.Sqrt(elem2) * 0.5f;
			    double mult = 0.25f / v;
		        q.x =(float)((i_mat.m02 + i_mat.m20) * mult);
		        q.y =(float)((i_mat.m21 + i_mat.m12) * mult);
				q.z =(float)(v);
		        q.w =(float)((i_mat.m10 - i_mat.m01) * mult);
			}else{
			    double v = Math.Sqrt(elem3) * 0.5f;
			    double mult = 0.25f / v;
		        q.x =(float)((i_mat.m21 - i_mat.m12) * mult);
		        q.y =(float)((i_mat.m02 - i_mat.m20) * mult);
		        q.z =(float)((i_mat.m10 - i_mat.m01) * mult);
				q.w =(float)v;
			}
			return q;
		}
	}
}

