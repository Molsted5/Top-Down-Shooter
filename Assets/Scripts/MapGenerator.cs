using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {

    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlineFraction;

    public float tileSize = 1;
    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    Map currentMap;

    void Awake() {
        FindAnyObjectByType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber ) {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap() {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);

        // Generating coords
        allTileCoords = new List<Coord>();
        for( int x = 0; x < currentMap.mapSize.x; x++ ) {
            for( int y = 0; y < currentMap.mapSize.y; y++ ) {
                allTileCoords.Add( new Coord( x, y ) );
            }
        }
        shuffledTileCoords = new Queue<Coord>( Utility.ShuffleArray( allTileCoords.ToArray(), currentMap.seed ) );

        // Create map holder object
        string holderName = "Generated Map";
        if( transform.Find( holderName ) ) {
            DestroyImmediate(transform.Find( holderName ).gameObject ); // immediate because it is for the editor
        }

        Transform mapHolder = new GameObject( holderName ).transform;
        mapHolder.parent = transform;

        // Spawning tiles
        for( int x = 0; x < currentMap.mapSize.x; x++ ) {
            for( int y = 0; y < currentMap.mapSize.y; y++ ) {
                Vector3 tilePosition = CoordToPosition( x, y );
                Transform newTile = Instantiate( tilePrefab, tilePosition, Quaternion.Euler( Vector3.right * 90 ) ) as Transform;
                newTile.localScale = Vector3.one * (1 - outlineFraction) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x,y] = newTile;
            }
        }

        // Spawning obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstacleFraction);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        for( int i = 0; i < obstacleCount; i++ ) {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if( randomCoord != currentMap.mapCentre && MapIsFullyAccesible( obstacleMap, currentObstacleCount ) ) {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition( randomCoord.x, randomCoord.y );
                
                Transform newObstacle = Instantiate( obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity ) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3( ( 1 - outlineFraction ) * tileSize, obstacleHeight, ( 1 - outlineFraction ) * tileSize );

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorFraction = randomCoord.y/(float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorFraction);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord>( Utility.ShuffleArray( allOpenCoords.ToArray(), currentMap.seed ) );

        // Creating navmesh mask
        Transform maskLeft = Instantiate( navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity ) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3( (maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y ) * tileSize;

        Transform maskRight = Instantiate( navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity ) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3( (maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y ) * tileSize;

        Transform maskTop = Instantiate( navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity ) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3( maxMapSize.x, 1, ( maxMapSize.y - currentMap.mapSize.y ) / 2f ) * tileSize;

        Transform maskBottom = Instantiate( navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity ) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3( maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f ) * tileSize;

        navmeshFloor.localScale = new Vector3( maxMapSize.x, 0, maxMapSize.y ) * tileSize * 0.1f;
        mapFloor.localScale = new Vector3( currentMap.mapSize.x, 1, currentMap.mapSize.y ) * tileSize * 0.1f;
        mapFloor.GetComponent<BoxCollider>().size = new Vector3(10, 1, 10);

    }

    bool MapIsFullyAccesible( bool[,] obstacleMap, int currentObstacleCount ) {
        // We use floodfill to find how many tiles are accesible and checks if it equals how many tiles that should be accesible 
        // If not the same, we know that some areas of the map were not accesible
        // Floodfill checks horizontal and vertical neighbors and moves ahead to new accesible tiles within the map

        bool[,] mapFlags = new bool[obstacleMap.GetLength( 0 ), obstacleMap.GetLength( 1 )];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue( currentMap.mapCentre );
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accesibleTileCount = 1;

        while( queue.Count > 0 ) {
            Coord tile = queue.Dequeue();

            for( int x = -1; x <= 1; x++ ) {
                for( int y = -1; y <= 1; y++ ) {
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;
                    if( x == 0 ^ y == 0 ) {
                        if( neighborX >= 0 && neighborX < obstacleMap.GetLength(0) &&  neighborY >= 0 && neighborY < obstacleMap.GetLength(1) ) {
                            if( !mapFlags[neighborX, neighborY] && !obstacleMap[neighborX, neighborY] ) {
                                mapFlags[neighborX, neighborY] = true;
                                queue.Enqueue( new Coord( neighborX, neighborY ) );
                                accesibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccesibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccesibleTileCount == accesibleTileCount;
    }

    Vector3 CoordToPosition( int x, int y ) {
        return new Vector3( -currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y ) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position ) {
        int x = Mathf.RoundToInt( position.x / tileSize + ( currentMap.mapSize.x - 1 ) / 2f );
        int y = Mathf.RoundToInt( position.z / tileSize + ( currentMap.mapSize.y - 1 ) / 2f );
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue( randomCoord );
        return randomCoord;
    }

    public Transform GetRandomOpenTile() {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue( randomCoord );
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [Serializable]
    public struct Coord {
        public int x;
        public int y;
        
        public Coord( int _x, int _y ) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1 , Coord c2 ) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1 , Coord c2 ) {
            return !(c1 == c2);
        }

        public override bool Equals( object obj ) {
            if( obj is Coord other )
                return this == other;
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine( x, y );
        }

    }
    
    [Serializable]
    public class Map {
        
        public Coord mapSize;
        [Range( 0, 1 )]
        public float obstacleFraction;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCentre {
            get {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }

    }
}
