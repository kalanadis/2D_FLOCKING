using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	private static GameWorld m_Instance;	
	public static GameWorld Instance{	
		get{	
			return m_Instance;	
		}	
	}

	private List<Agent>[] m_Grid;
	public List<Agent>[] Grid{

		get{
			return m_Grid;
		}
	}


	void Awake() {
		m_Instance = this;
	}

	// Use this for initialization
	void Start () {
	     
		m_Grid = new List<Agent>[12];
		for(int i=0;i<12;i++){
			m_Grid[i]=new List<Agent>();
		}
	}
	

}
