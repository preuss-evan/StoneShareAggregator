using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Add all connection types here
public enum ConnectionType { conPink = 0 , conYellow = 1, conBlue = 2, conOrange = 3, conCyan = 4, conGreen = 5, conStair = 6}
public class Connection
{

    #region public fields
    public ConnectionType Type;
    public string Name;
    public List<TilePattern> ConnectingTiles;

    #endregion

    #region private fields

    #endregion

    #region constructors
    public Connection(ConnectionType type, string name)
    {
        Type = type;
        Name = name;
    }

    #endregion

    #region public functions
    public void AddTilePatternToConnection(TilePattern pattern)
    {
        if(ConnectingTiles == null)ConnectingTiles = new List<TilePattern>();
        ConnectingTiles.Add(pattern);
    }

    #endregion

    #region private functions

    #endregion

}
