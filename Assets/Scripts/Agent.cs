using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {

	public float Agent_Radius;

	private Vector2 m_Velocity;
	private float m_HeadingAngle;
	private Transform m_Transform;

	public float WanderRadius;
	public float WanderDistance;
	public float WanderJitter;

	private Vector2 m_vWanderTarget;

	private GameWorld m_GameWorld;
	private int m_CurrentGrid;

	private int m_NebCount;

	public int SeparationWeight;
	public int AlignmentWeight;
	public int CohesionWeight;
	public int WallAvoidanceWeight;
	public int WanderWeight;
	public int MaxSteeringForce;

	// Use this for initialization
	void Start () {
	    
		m_Transform = transform;

		m_vWanderTarget = new Vector2(1,0);

		m_GameWorld = GameWorld.Instance;
		m_CurrentGrid = -1;
		m_NebCount = 0;
	
	}
	
	void FixedUpdate(){

		GetComponent<Rigidbody2D>().AddForce (SteeringForce());

		//set heading
		m_Velocity = GetComponent<Rigidbody2D>().velocity;
		m_HeadingAngle = (Mathf.Atan2(m_Velocity.y,m_Velocity.x))*Mathf.Rad2Deg;
		GetComponent<Rigidbody2D>().MoveRotation (m_HeadingAngle);

		UpdateGrid ();
	}
	
	private void UpdateGrid(){
		
		int x = (int)(m_Transform.position.x*0.25f);
		int y = (int)(m_Transform.position.y*0.25f);
		
		int i =((y*4)+x);
		
		if (m_CurrentGrid != i) {
			
			if(m_CurrentGrid!=-1)m_GameWorld.Grid[m_CurrentGrid].Remove(this);
			m_CurrentGrid = i;
			m_GameWorld.Grid[i].Add(this);
			
		}
	}

	
	private Vector2 SteeringForce(){
	
		m_NebCount = 0;
		
		Vector2 Separation=new Vector2(0,0);
		Vector2 Alignment=new Vector2(0,0);
		Vector2 Cohesion=new Vector2(0,0);
		Vector2 SteerForce=new Vector2(0,0);

		
		int x_0 = (int)((m_Transform.position.x-Agent_Radius)*0.25f);
		if (x_0 < 0)x_0 = 0;

		int y_0 = (int)((m_Transform.position.y+Agent_Radius)*0.25f);
		if (y_0 > 2)y_0 = 2;
		
		int x_1 = (int)((m_Transform.position.x+Agent_Radius)*0.25f);
		if (x_1 > 3)x_1 = 3;
		
		int y_1 = (int)((m_Transform.position.y-Agent_Radius)*0.25f);
		if (y_1 < 0)y_1 = 0;

		List<Agent> N_lst;
		Vector2 agentToTar;
		float mag_of_agentToTar;

		for(int i=y_1;i<=y_0;i++){
			
			for(int j=x_0;j<=x_1;j++){
				
				N_lst=m_GameWorld.Grid[((i*4)+j)];

				foreach(Agent val in N_lst){
					
					if(this.GetInstanceID()!=val.GetInstanceID()){
						
						agentToTar = (Vector2)(m_Transform.position-val.transform.position);
						mag_of_agentToTar = agentToTar.magnitude;

						//check the radius
						if(mag_of_agentToTar<Agent_Radius){

							m_NebCount++;
							//separation
							Separation += agentToTar.normalized/mag_of_agentToTar; 
							//sum of heading
							Alignment += (Vector2)val.transform.right;
							//sum of mass
							Cohesion += (Vector2)val.transform.position;

						}
					}
				}



				
			}
			
		}


		if (m_NebCount > 0) {

			Alignment /= m_NebCount;
			Alignment -= (Vector2)m_Transform.right;

			Cohesion /= m_NebCount;
			Cohesion = Seek(Cohesion);

		}

		SteerForce += AccForce(SteerForce.magnitude,WallAvoid()*WallAvoidanceWeight);
		SteerForce += AccForce(SteerForce.magnitude,Separation*SeparationWeight);
		SteerForce += AccForce (SteerForce.magnitude, Cohesion*CohesionWeight);
		SteerForce += AccForce(SteerForce.magnitude,Alignment*AlignmentWeight);
		SteerForce += AccForce(SteerForce.magnitude,Wander ()*WanderWeight);

		return SteerForce;

	}

	private Vector2 Seek(Vector2 targetPo){

		Vector2 DesiredVelocity = (targetPo - (Vector2)m_Transform.position).normalized;
		return (DesiredVelocity - m_Velocity);

	}

	private Vector2 Wander(){

		float R = Random.Range(-10f,10f);
		//Debug.Log (GetInstanceID()+" R "+R);

		m_vWanderTarget += (new Vector2(R,R)*WanderJitter);
		m_vWanderTarget.Normalize ();
		m_vWanderTarget *= WanderRadius;

		Vector2 target = new Vector2 (WanderDistance,0);
		target += m_vWanderTarget;
		target = m_Transform.TransformPoint (target);

		target = (target - (Vector2)m_Transform.position).normalized;

		return (target - m_Velocity);
	}

	private Vector2 WallAvoid(){

		int layerMask = 1 << 8;
		float hit_Distance=2;
		Vector3 Wall_normal=new Vector3(0,0,0);

		RaycastHit2D hit = Physics2D.Raycast (m_Transform.position,m_Transform.right,1,layerMask);
		if (hit.collider != null){
			if(hit_Distance>hit.distance){

				Wall_normal = hit.collider.transform.right;
			}
		}
		//Debug.DrawRay (m_Transform.position, m_Transform.right,Color.red);

		hit = Physics2D.Raycast (m_Transform.position,Quaternion.Euler(0,0,45)*m_Transform.right,1,layerMask);
		if (hit.collider != null){
			if(hit_Distance>hit.distance){
				
				Wall_normal = hit.collider.transform.right;
			}
		}
		//Debug.DrawRay (m_Transform.position, Quaternion.Euler(0,0,45)*m_Transform.right,Color.red);

		hit = Physics2D.Raycast (m_Transform.position,Quaternion.Euler(0,0,-45)*m_Transform.right,1,layerMask);
		if (hit.collider != null){
			if(hit_Distance>hit.distance){
				
				Wall_normal = hit.collider.transform.right;
			}
		}
		//Debug.DrawRay (m_Transform.position, Quaternion.Euler(0,0,-45)*m_Transform.right,Color.red);
	    
		return (Vector2)Wall_normal * hit_Distance;
	}

	private Vector2 AccForce(float SteeringMag,Vector2 ForceToAdd){

		Vector2 returnForce = new Vector2(0,0);
		float ForceToAddMag = ForceToAdd.magnitude;
		float RemainingForceMag = MaxSteeringForce - SteeringMag;

		if(RemainingForceMag <=  0)
			return returnForce;

		if (ForceToAddMag < RemainingForceMag)
						return ForceToAdd;
				else
						return (ForceToAdd.normalized * RemainingForceMag);

	}
}
