using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ElementTypeList
{
    public List<EElementType> ElementTypes;
    public ElementTypeList()
    {
        ElementTypes = new List<EElementType>();
    }
}

[Serializable]
public class ElementalStrengths
{
    public EElementType Element;
    public ElementTypeList StrongAgainst;
    public ElementTypeList WeakAgainst;
    public ElementTypeList NeutralAgainst;

    public ElementalStrengths()
    { 
    }

    public bool IsStrongAgainst(EElementType element)
    {
        return StrongAgainst.ElementTypes.Contains(element);
    }

    public bool IsWeakAgainst(EElementType element)
    {
        return WeakAgainst.ElementTypes.Contains(element);
    }
}

public class ElementDataReader
{
    public static List<ElementalStrengths> ReadElementalData(string filePath)
    {
        if(!File.Exists(filePath))
        {
            UnityEngine.Debug.Log("Elemental data file not found at path: " + filePath);
            return null;
        }

        var elements = new List<ElementalStrengths>();

        var lines = File.ReadLines(filePath);

        // Skip the header line
        bool isFirstLine = true;

        foreach (var line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false; // Skip the header
                continue;
            }

            var values = line.Split(',');

            if (values.Length >= 4)
            {
                // Safely parse the element
                if (Enum.TryParse(values[0].Trim().ToUpper(), out EElementType element))
                {
                    var strongAgainst = ParseElementList(values[1]);
                    strongAgainst.AddRange(ParseElementList(values[2]));

                    var weakAgainst = ParseElementList(values[3]);
                    weakAgainst.AddRange(ParseElementList(values[4]));

                    var neutralAgainst = ParseElementList(values[5]);
                    neutralAgainst.AddRange(ParseElementList(values[6]));

                    elements.Add(new ElementalStrengths
                    {
                        Element = element,
                        StrongAgainst = new ElementTypeList { ElementTypes = strongAgainst },
                        WeakAgainst = new ElementTypeList { ElementTypes = weakAgainst },
                        NeutralAgainst = new ElementTypeList { ElementTypes = neutralAgainst }
                    });
                }
                else
                {
                    Debug.Log($"Invalid element name found: {values[0]}");
                }
            }
        }

        return elements;
    }

    private static List<EElementType> ParseElementList(string elementList)
    {
        var elements = new List<EElementType>();
        var elementNames = elementList.Split(',');

        foreach (var name in elementNames)
        {
            // Safely parse each element name
            if (Enum.TryParse(name.Trim('/', '"').ToUpper(), out EElementType element))
            {
                elements.Add(element);
            }
            else
            {
                Debug.Log($"Invalid element name: {name.Trim()}");
            }
        }

        return elements;
    }
}