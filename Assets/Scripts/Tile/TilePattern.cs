using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PatternType { Stair, Elevator, Landing}   
public class TilePattern
{

    #region public fields

    public List<Connection> ConnectionTypes;
    public Connection[] Connections;
    public int Index;

    #endregion

    #region private fields
    GameObject _goTilePrefab;
    
    #endregion

    #region constructors
    public TilePattern(int index, GameObject goTilePrefab, List<Connection> connectionTypes)
    {
        Index = index;
        _goTilePrefab = goTilePrefab;
        ConnectionTypes = connectionTypes;
        GetConnections();
    }

    #endregion

    #region public functions
    
    #endregion

    #region private functions
    public void GetConnections()
    {
        Connections = new Connection[6];

        List<GameObject> goConnections = Util.GetChildObjectByLayer(_goTilePrefab.transform, LayerMask.NameToLayer("Connections"));
        if(goConnections.Count != 6)
        {
            Debug.Log($"no 6 connections found for prefab {_goTilePrefab.name} ");
        }

        foreach (var goConnection in goConnections)
        {
            var connection = ConnectionTypes.First(c => c.Name == goConnection.tag);
            connection.AddTilePatternToConnection(this);
            Vector3 rotation = goConnection.transform.rotation.eulerAngles;

            //Connection[0]: -x
            //Connection[1]: x
            //Connection[2]: -y
            //Connection[3]: y
            //Connection[4]: -z
            //Connection[5]: z

            if (rotation.x != 0)
            {
                //we know it is a vertical connection
                if (rotation.x == 90)
                {
                    Connections[2] = connection; //positive y axis (debug once working to ensure that this is correct) 
                }
                else
                {
                    Connections[3] = connection; //negative y axis (debug once working to ensure that this is correct)
                }
            }
            
            //Connections[(int)rotation.y % 90] = connection;
            else if (rotation.y == 90)                                
            {
                Connections[1] = connection; //positive x axis   
            }
            else if (rotation.y == 180)                                 
            {
                Connections[4] = connection; //negative z axis 
            }
            else if (rotation.y == 270)    
            {
                Connections[0] = connection; //negative x axis  
            }
            else                                                        
            {
                Connections[5] = connection; //positive z axis  
            }
        }
    }
    #endregion

}
