using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//<summary>
//Game object, that creates maze and instantiates it in scene
//</summary>
public class MazeSpawner : MonoBehaviour {
	public enum MazeGenerationAlgorithm
    {
		PureRecursive,
		RecursiveTree,
		RandomTree,
		OldestTree,
		RecursiveDivision,
	}

    private enum OpenPort
    {
        Right,
        Front,
        Left,
        Back,
    }

	public MazeGenerationAlgorithm Algorithm = MazeGenerationAlgorithm.PureRecursive;
	public bool FullRandom = false;
	public int RandomSeed = 12345;
	public GameObject Floor = null;
	public GameObject Wall = null;
	public GameObject Pillar = null;
	public int Rows = 5;
	public int Columns = 5;
	public float CellWidth = 5;
	public float CellHeight = 5;
	public bool AddGaps = true;
    [SerializeField]
    private int _GoalSlotCount = 0;
    public int GoalSlotCount { get { return _GoalSlotCount; } }

    public List<GameObject> ActorList = null;

    private BasicMazeGenerator mMazeGenerator = null;

    public GameStateManager mGameStateManager = null;



    void Awake () {
        _GoalSlotCount = 0;
        List<Vector3> _GoalPosList = new List<Vector3>();
        List<OpenPort> _GoalDirList = new List<OpenPort>();

        if (!FullRandom) {
			Random.InitState(RandomSeed);
		}
		switch (Algorithm) {
		case MazeGenerationAlgorithm.PureRecursive:
			mMazeGenerator = new RecursiveMazeGenerator (Rows, Columns);
			break;
		case MazeGenerationAlgorithm.RecursiveTree:
			mMazeGenerator = new RecursiveTreeMazeGenerator (Rows, Columns);
			break;
		case MazeGenerationAlgorithm.RandomTree:
			mMazeGenerator = new RandomTreeMazeGenerator (Rows, Columns);
			break;
		case MazeGenerationAlgorithm.OldestTree:
			mMazeGenerator = new OldestTreeMazeGenerator (Rows, Columns);
			break;
		case MazeGenerationAlgorithm.RecursiveDivision:
			mMazeGenerator = new DivisionMazeGenerator (Rows, Columns);
			break;
		}
		mMazeGenerator.GenerateMaze ();
		for (int row = 0; row < Rows; row++) {
			for(int column = 0; column < Columns; column++){
				float x = column*(CellWidth+(AddGaps?.2f:0));
				float z = row*(CellHeight+(AddGaps?.2f:0));
                int _OpenPortMagicNum = 0;
				MazeCell cell = mMazeGenerator.GetMazeCell(row,column);
				GameObject tmp;
				tmp = Instantiate(Floor,new Vector3(x,0,z), Quaternion.Euler(0,0,0)) as GameObject;
				tmp.transform.parent = transform;
				if(cell.WallRight){
					tmp = Instantiate(Wall,new Vector3(x+CellWidth/2,0,z)+Wall.transform.position,Quaternion.Euler(0,90,0)) as GameObject;// right
					tmp.transform.parent = transform;
                    _OpenPortMagicNum += 1;

                }
				if(cell.WallFront){
					tmp = Instantiate(Wall,new Vector3(x,0,z+CellHeight/2)+Wall.transform.position,Quaternion.Euler(0,0,0)) as GameObject;// front
					tmp.transform.parent = transform;
                    _OpenPortMagicNum += 3;
                }
				if(cell.WallLeft){
					tmp = Instantiate(Wall,new Vector3(x-CellWidth/2,0,z)+Wall.transform.position,Quaternion.Euler(0,270,0)) as GameObject;// left
					tmp.transform.parent = transform;
                    _OpenPortMagicNum += 5;
                }
				if(cell.WallBack){
					tmp = Instantiate(Wall,new Vector3(x,0,z-CellHeight/2)+Wall.transform.position,Quaternion.Euler(0,180,0)) as GameObject;// back
					tmp.transform.parent = transform;
                    _OpenPortMagicNum += 7;
                }
				if(cell.IsGoal){
                    _GoalSlotCount += 1;
                    _GoalPosList.Add(new Vector3(x, 1, z));
                    if(_OpenPortMagicNum == (3 + 5 + 7))
                    {
                        _GoalDirList.Add(OpenPort.Right);
                    }
                    else if(_OpenPortMagicNum == (1 + 5 + 7))
                    {
                        _GoalDirList.Add(OpenPort.Front);
                    }
                    else if (_OpenPortMagicNum == (1 + 3 + 7))
                    {
                        _GoalDirList.Add(OpenPort.Left);
                    }
                    else if (_OpenPortMagicNum == (1 + 3 + 5))
                    {
                        _GoalDirList.Add(OpenPort.Back);
                    }
                    else
                    {
                        _GoalDirList.Add(OpenPort.Front);
                    }
                }
			}
		}



        if (Pillar != null){
			for (int row = 0; row < Rows+1; row++) {
				for (int column = 0; column < Columns+1; column++) {
					float x = column*(CellWidth+(AddGaps?.2f:0));
					float z = row*(CellHeight+(AddGaps?.2f:0));
					GameObject tmp = Instantiate(Pillar,new Vector3(x-CellWidth/2,0,z-CellHeight/2),Quaternion.identity) as GameObject;
					tmp.transform.parent = transform;
				}
			}
		}

        var actor_index_list = Utilities.GetRandomIntList(0, ActorList.Count);
        var goal_slot_index_list = Utilities.GetRandomIntList(0, _GoalPosList.Count);


        for(int i = 0;i < actor_index_list.Count;i++)
        {
            int actor_index = actor_index_list[i];
            int goal_slot_index = goal_slot_index_list[i];
            GameObject tmp;
            Quaternion quat = Quaternion.identity;
            if(_GoalDirList[goal_slot_index] == OpenPort.Back)
            {
                quat = Quaternion.Euler(0, 180f, 0);
            }
            else if(_GoalDirList[goal_slot_index] == OpenPort.Front)
            {
                quat = Quaternion.Euler(0, 0, 0);
            }
            else if (_GoalDirList[goal_slot_index] == OpenPort.Left)
            {
                quat = Quaternion.Euler(0, -90f, 0);
            }
            else if (_GoalDirList[goal_slot_index] == OpenPort.Right)
            {
                quat = Quaternion.Euler(0, 90f, 0);
            }
            tmp = Instantiate(ActorList[actor_index], _GoalPosList[goal_slot_index], quat) as GameObject;
            tmp.transform.parent = transform;
        }

        mGameStateManager.StartGame();
    }
}
