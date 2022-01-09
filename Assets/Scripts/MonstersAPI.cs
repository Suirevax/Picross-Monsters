using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Xml;
using JetBrains.Annotations;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;


public class MonstersAPI : MonoBehaviour
{
    private static readonly Uri UriLink = new Uri("https://app.pixelencounter.com/api/basic/monsters/random");
    private const int GridSize = 12; //max width & height of a monster


    public async void GETMonster()
    {
        var httpClient = new HttpClient();
        var httpResponseMessage = await httpClient.GetAsync(UriLink);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            Debug.LogWarning("API Response failed");
            return;
        }

        var test = httpResponseMessage.Content;
        var text = await httpResponseMessage.Content.ReadAsStringAsync();
//        Debug.Log(text);

        //Parsing to XML DOM
        var settings = new XmlReaderSettings
        {
            Async = true
        };

        var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
        var reader = XmlReader.Create(stream, settings);
        Color[,] pixelGrid = new Color[GridSize,GridSize];

        //put all pixels in a list
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "rect")
                {
                    var x = 0;
                    var y = 0;
                    if(!int.TryParse(reader.GetAttribute("x"), out x)) continue;
                    if(!int.TryParse(reader.GetAttribute("y"), out y)) continue;
//                    Debug.Log("Filledpos: " + x + "," + y);

                    var colorString = reader.GetAttribute("fill");

                    colorString = colorString.Remove(0, 1);
                    var colorInt = int.Parse(colorString, System.Globalization.NumberStyles.HexNumber);
                    pixelGrid[y/10, x/10] = new Color((colorInt >> 16) & 0xff, (colorInt >> 8) & 0xff, colorInt & 0xff);
                }
            }
        }
        LogPixelGrid(pixelGrid);
        
        List<int> rowHints = new List<int>{1,1,1};
        var row = new List<bool> {false,true,false,true,false,true,false,false,false,false,false,false };

        // for(var i = 0; i < 30; i++)
        // {
        //     Debug.Log( IterateRow(rowHints, row, out List<bool> iteratedRow));
        //     row = iteratedRow;
        // }


        CheckForMultipleSolutions(GenerateRowHints(pixelGrid), GenerateColumnHints(pixelGrid));
    }

    private List<List<int>> GenerateColumnHints(Color[,] pixelGrid)
    {
        var columnsHints = new List<List<int>>();
        for (var x = 0; x < GridSize; x++)
        {
            var columnHints = new List<int>();
            var counter = 0;

            for (var y = 0; y < GridSize; y++)
            {
                if (pixelGrid[y, x].a != 0f)
                {
                    counter++;
                }
                else
                {
                    if(counter != 0) columnHints.Add(counter);
                    counter = 0;
                }
            }
            if(counter != 0) columnHints.Add(counter);
            columnsHints.Add(columnHints);
            
            // string debugMessage = "";
            // foreach (var hint in columnHints)
            // {
            //     debugMessage += hint.ToString() + ",";
            // }
            // Debug.Log(debugMessage);
        }

        return columnsHints;
    }

    private List<List<int>> GenerateRowHints(Color[,] pixelGrid)
    {
        var rowsHints = new List<List<int>>();
        for (var y = 0; y < GridSize; y++)
        {
            var rowHints = new List<int>();
            var counter = 0;
            for (var x = 0; x < GridSize; x++)
            {
                if (pixelGrid[y, x].a != 0f)
                {
                    counter++;
                }
                else
                {
                    if(counter != 0) rowHints.Add(counter);
                    counter = 0;
                }
            }
            if(counter != 0) rowHints.Add(counter);
            rowsHints.Add(rowHints);

             string debugMessage = "";
             foreach (var hint in rowHints)
             {
                 debugMessage += hint.ToString() + ",";
             }
            Debug.Log(debugMessage);
        }
        return rowsHints;
    }

    void LogPixelGrid(Color[,] pixelGrid)
    {
        var text = "[[";
        for (var y = 0; y < GridSize; y++)
        {
            if (y > 0)
            {
                text += "\n[";
            }
            for (var x = 0; x < GridSize; x++)
            {
                var value = pixelGrid[y, x].a != 0f;
                if (x < GridSize - 1)
                {
                    text += Convert.ToInt16(value) + ",";
                }
                else
                {
                    text += Convert.ToInt16(value);
                }
            }
            text += "]";
        }
        text += "]";
        Debug.Log(text);
    }
    
    void LogPixelGrid(bool[,] pixelGrid)
    {
        var text = "[[";
        for (var y = 0; y < GridSize; y++)
        {
            if (y > 0)
            {
                text += "\n[";
            }
            for (var x = 0; x < GridSize; x++)
            {
                var value = pixelGrid[y, x];
                if (x < GridSize - 1)
                {
                    text += Convert.ToInt16(value) + ",";
                }
                else
                {
                    text += Convert.ToInt16(value);
                }
            }
            text += "]";
        }
        text += "]";
        Debug.Log(text);
    }
    
    void LogPixelGrid(List<List<bool>> pixelGrid)
    {
        var text = "[[";
        for (var y = 0; y < GridSize; y++)
        {
            if (y > 0)
            {
                text += "\n[";
            }
            for (var x = 0; x < GridSize; x++)
            {
                var value = pixelGrid[y][x];
                if (x < GridSize - 1)
                {
                    text += Convert.ToInt16(value) + ",";
                }
                else
                {
                    text += Convert.ToInt16(value);
                }
            }
            text += "]";
        }
        text += "]";
        Debug.Log(text);
    }

    void CheckForMultipleSolutions(List<List<int>> rowsHints, List<List<int>> columnsHints)
    {
        var height = rowsHints.Count;
        var width = columnsHints.Count;

        //first iteration of grid
        var grid = MakeInitialGrid(rowsHints, width);
        LogPixelGrid(grid);


        DebugBlockMovingFunctions(grid[0], 0);
        
        LogPixelGrid(grid);

        var InitialGrid = GridDeepCopy(grid);
        var rowIndex = 0;

        var lst = new List<bool> {true, false, false, false, false, false, false, false, false, false, false, true};
        var lstHints = new List<int> {1, 1};
        
        Debug.Log(CanBeIterated(lst, lstHints.Count));

        for (var i = 0; i < 30; i++)
        {
            if (CanBeIterated(lst, lstHints.Count))
            {
                Iterate(lst, lstHints);
                {
                    string result = "";
                    foreach (var item in lst)
                    {
                        result += item.ToString() + ", ";
                    }
        
                    Debug.Log(result);
                }
            }
        }
       
        
        
        // //Testing IterateFunction
        //  while (CanBeIterated(grid[rowIndex], rowsHints.Count))
        //  {
        //      Debug.Log("test");
        //      Iterate(grid[rowIndex], rowsHints[rowIndex]);
        //      
        //      
        //  }
        
        

        // //Debug.Log( IterateRow(rowsHints[0],grid[0],out var iteratedRow));
        //
        // //actually start checking
        // //Depth First Search
        // // eindig condities:
        // //      als een 2de oplossing wordt gevonden
        // //      Als elke oplossing geprobeerd is.
        // // Initiele status is dat elke rij alle blokken zo ver mogelijk links heeft staan. En langzaam wordt naar rechts gewerkt.
        // // werk van beneden naar boven. (onderste rij wordt het regelematigst ge iterate.
        // // opbouw algoritme:
        // var solutionCount = 0;
        // var rowIndex = 0;
        //
        // var initialList = new List<List<bool>>();
        // initialList = grid;
        //
        // //RecursiveCheck(ref grid, ref solutionCount, rowIndex, rowsHints, initialList);
    }

    void Check(List<List<bool>> grid, List<List<bool>> initialGrid, int rowIndex, List<List<int>> rowsHints)
    {
        
        //Start at index 0
        //Depth First Search
        //Reset row to inital values and shift right
        if (rowIndex < grid.Count)
        {
            // Check(grid, initialGrid, rowIndex-1);
            // while()
        }
        else
        {
            //Lowest row
            //CheckSolution(grid);
            while (CanBeIterated(grid[rowIndex],rowsHints[rowIndex].Count))
            {
                //Iterate
                
            }
        }
    }

    void Iterate(List<bool> row, List<int> rowHints)
    {
        var selectedBlock = rowHints.Count - 1;
        if (CheckIfBlockCanBeMovedRight(selectedBlock, row))
        {
            Debug.Log("first block moved right");
            MoveBlockRight(row, selectedBlock);
        }
        else
        {
            while (selectedBlock > 0)
            {
                Debug.Log(selectedBlock);
                selectedBlock--;
                if (CheckIfBlockCanBeMovedRight(selectedBlock, row))
                {
                    MoveBlockRight(row, selectedBlock);
                    //&
                    //Reset all blocks right of selectedBlock
                    if (selectedBlock == rowHints.Count - 1) return;
                    
                    var index = GetEndPositionOfBlock(selectedBlock, row);
                    for (; selectedBlock < rowHints.Count; selectedBlock++)
                    {
                        row[++index] = false;
                        for (var i = rowHints[selectedBlock]; i < selectedBlock; i++)
                        {
                            row[++index] = true;
                        }
                    }

                    while (++index < row.Count)
                    {
                        row[index] = false;
                    }
                    return;
                }
            }

        }
        return;

    }

    bool CanBeIterated(List<bool> row, int blockCount)
    {
        if (!row[row.Count - 1]) return true;
        
        var previousValue = false;
        for (var i = row.Count - 1; i >= 0; i--)
        {
            if (!previousValue && !row[i])
            {
                return true;
            }

            previousValue = row[i];
        }
        return false;
    }

    List<List<bool>> GridDeepCopy(List<List<bool>> grid)
    {
        var gridCopy = new List<List<bool>>();
        foreach (var row in grid)
        {
            gridCopy.Add(new List<bool>(row));
        }
        return gridCopy;
    }

    void DebugBlockMovingFunctions(List<bool> row, int blockIndex)
    {
        var rowCopy1 = new List<bool>(row);
        var rowCopy2 = new List<bool>(row);
        Debug.Log("Check if all blocks can be moved right: " + CheckIfAllBlocksCanBeMovedRight(row)
                                                             + "\nCheck if block can be moved right: " +
                                                             CheckIfBlockCanBeMovedRight(blockIndex, row));
        
        
        //Debug.Log("Move all blocks right: " + MoveAllBlocksRight(row)); 
        Debug.Log("All blocks moved right: " + LogRow(row));
        
        Debug.Log("Move block right: " + MoveBlockRight(rowCopy2, blockIndex)); 
        Debug.Log("All blocks moved right: " + LogRow(rowCopy2));
    }

    string LogRow(List<bool> row)
    {
        string tmp = "";
        foreach (var value in row)
        {
            tmp += value + ",";
        }
        return tmp;
    }

    List<List<bool>> MakeInitialGrid(List<List<int>> rowsHints, int columnCount)
    {
        var grid = new List<List<bool>>();

        foreach(var rowHints in rowsHints)
        {
            var newRow = new List<bool>( new bool[columnCount]);
            var currentLocation = 0;
            foreach (var hint in rowHints)
            {
                for (var i = 0; i < hint; i++)
                {
                    newRow[currentLocation++] = true;
                }
                currentLocation++;
            }
            grid.Add(newRow);
        }

        return grid;
    }

    void RecursiveCheck(ref List<List<bool>> grid, ref int solutionCount, int rowIndex, List<List<int>> rowsHints, List<List<bool>> startGrid)
    {
 
    }
    



    
    void ResetToPreviousInitialRow()
    {
        
    }

    bool MoveBlockRight(List<bool> row, int blockIndex)
    {
        if (!CheckIfBlockCanBeMovedRight(blockIndex, row)) return false;

        var startIndex = GetStartPositionOfBlock(blockIndex, row);
        var index = GetEndPositionOfBlock(blockIndex, row);
        Debug.Log("Index = " + index);
        if (index == -1) return false;

        
        while (row[index])
        {
            row[index + 1] = row[index];
            if (index == 0)
            {
                row[0] = false;
                break;
            }
            index--;
        }
        row[startIndex] = false;

        return true;
    }
    
    bool CheckIfBlockCanBeMovedRight(int blockIndex, List<bool> row)
    {
        var endPosIndex = GetEndPositionOfBlock(blockIndex, row);
        if (endPosIndex == -1) return false;
        if (endPosIndex == row.Count - 1) return false;
        if (endPosIndex + 2 >= row.Count) return true;
        return !row[endPosIndex + 2];
    }

    //returns last 'true' position of the blockIndex block
    int GetEndPositionOfBlock(int blockIndex, List<bool> row)
    {
        //FindStartOfTheBlock
        var index = GetStartPositionOfBlock(blockIndex, row);
        //return -1 if block was not found
        if (index == -1) return -1;
        
        //FindEndOfBlock
        while (row[index])
        {
            index++;
            if(index >= row.Count) break;
        }

        return index - 1;
    }

    int GetStartPositionOfBlock(int blockIndex, List<bool> row)
    {
        var blockCounter = 0;
        var previousValue = false;
        for (var i = 0; i < row.Count; i++)
        {
            if (previousValue == false && row[i] == true)
            {
                //FoundBeginOFABlock
                if (blockCounter == blockIndex) return i;
                blockCounter++;
            }
            previousValue = row[i];
        }

        return -1;
    }

    //moves all block that are right of the blockIndex block 1 to the right
    //MOVES BLOCKINDEX BLOCK
    bool MoveAllBlocksRight(List<bool> row, int blockIndex)
    {
        if(!CheckIfAllBlocksCanBeMovedRight(row)) return false;

        var minIndex = GetStartPositionOfBlock(blockIndex, row);

        for (var i = row.Count - 2; i >= minIndex; i--)
        {
            row[i + 1] = row[i];
        }

        row[minIndex] = false;
        
        return true;
    }

    bool CheckIfAllBlocksCanBeMovedRight(List<bool> row)
    {
        return row.Contains(true) && !row[row.Count - 1];
    }


    
    bool IterateRow( List<int> rowHints,List<bool> row, out List<bool> iteratedRow)
    {
        //TODO: remove Debuglogs
        {
            string result = "List contents: ";
            foreach (var item in row)
            {
                result += item.ToString() + ", ";
            }

            Debug.Log(result);
        }

        iteratedRow = new List<bool>(new bool[row.Count]);
        var blockCounter = 0;
        var rightWall = row.Count;
        for (var i = row.Count - 1; i >= 0; i--)
        {
            if (row[i] == true)
            {
                //move block 1 right if possible. otherwise continue.
                if (i + 1 < rightWall)
                {
                    var leftWall = i + 1;
                    //move current block 1 to the right
                    while (row[i] == true)
                    {
                        iteratedRow[i + 1] = true;
                        if (i == 0) break;
                        i--;
                    }

                    var leftWallStartPosition = leftWall;
                    //move all blocks from the right of the current block as far left possible
                    // j = de index van rowHints
                    for (var j = (rowHints.Count - 1) - (blockCounter - 1); j < rowHints.Count; j++)
                    {
                        var currentBlock = rowHints[j];
                        var startPlacement = leftWall + 1;

                        for (var n = 0; n < currentBlock; n++)
                        {
                            iteratedRow[startPlacement + n] = true;
                        }

                        leftWall += currentBlock + 1;
                    }
                    
                    //leave everything left of the left wall the way it is.
                    for (var b = i-1; b >= 0; b--)
                    {
                        iteratedRow[b] = row[b];
                    }
                    
                    //Done with successful iteration
                    //TODO: remove DebugStuff
                    {
                        string result = "List contents: ";
                        foreach (var item in iteratedRow)
                        {
                            result += item.ToString() + ", ";
                        }

                        Debug.Log(result);
                    }
                    return true;
                }
                else
                {
                    Debug.Log("Block could not be moved more right");
                    //move rightwall past the block & continue with next block
                    rightWall -= rowHints[(rowHints.Count - 1) - blockCounter] + 1;
                    i = rightWall;
                    blockCounter++;
                }
            }
        }
        
        
        iteratedRow = row;
        //TODO: remove DebugStuff
        {
            string result = "List contents: ";
            foreach (var item in iteratedRow)
            {
                result += item.ToString() + ", ";
            }

            Debug.Log(result);
        }
        return false;
    }
}
