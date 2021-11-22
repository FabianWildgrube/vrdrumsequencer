using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class Utils { 
    public static bool componentwiseSignEquals(Vector3 lhs, Vector3 rhs)
    {
        return (lhs.x > 0 == rhs.x > 0) && (lhs.y > 0 == rhs.y > 0) && (lhs.z > 0 == rhs.z > 0);
    }

    /// <summary>
    /// Equals comparison to a degree of precision of 0.001
    /// </summary>
    public static bool roughEquals(Vector3 lhs, Vector3 rhs)
    {
        const float delta = 0.001f;
        return Mathf.Abs(lhs.x - rhs.x) < delta && Mathf.Abs(lhs.y - rhs.y) < delta && Mathf.Abs(lhs.z - rhs.z) < delta;
    }

    public static Vector3 componentwiseMultiplication(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
    }

    public static float[] getRandomSampleVectorValues()
    {
        return Enumerable
            .Repeat(0, GlobalConstants.NR_OF_VECTOR_VALUES)
            .Select(i => UnityEngine.Random.Range(0.0f, 1.0f))
            .ToArray();
    }

    /* Random Adjectives and names taken from:
    Copyright (c) 2021 by Mike Ryan (https://codepen.io/mikedryan/pen/vLrgqr)
    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */
    private static readonly string[] randomAdjectives = { "admiring", "adoring", "agitated", "amazing", "angry", "awesome", "backstabbing", "berserk", "big", "boring", "clever", "cocky", "compassionate", "condescending", "cranky", "desperate", "determined", "distracted", "dreamy", "drunk", "ecstatic", "elated", "elegant", "evil", "fervent", "focused", "furious", "gigantic", "gloomy", "goofy", "grave", "happy", "high", "hopeful", "hungry", "insane", "jolly", "jovial", "kickass", "lonely", "loving", "mad", "modest", "naughty", "nauseous", "nostalgic", "pedantic", "pensive", "prickly", "reverent", "romantic", "sad", "serene", "sharp", "sick", "silly", "sleepy", "small", "stoic", "stupefied", "suspicious", "tender", "thirsty", "tiny", "trusting" };
    private static readonly string[] randomNames = { "albattani", "allen", "almeida", "archimedes", "ardinghelli", "aryabhata", "austin", "babbage", "banach", "bardeen", "bartik", "bassi", "bell", "bhabha", "bhaskara", "blackwell", "bohr", "booth", "borg", "bose", "boyd", "brahmagupta", "brattain", "brown", "carson", "chandrasekhar", "colden", "cori", "cray", "curie", "darwin", "davinci", "dijkstra", "dubinsky", "easley", "einstein", "elion", "engelbart", "euclid", "euler", "fermat", "fermi", "feynman", "franklin", "galileo", "gates", "goldberg", "goldstine", "goldwasser", "golick", "goodall", "hamilton", "hawking", "heisenberg", "heyrovsky", "hodgkin", "hoover", "hopper", "hugle", "hypatia", "jang", "jennings", "jepsen", "joliot", "jones", "kalam", "kare", "keller", "khorana", "kilby", "kirch", "knuth", "kowalevski", "lalande", "lamarr", "leakey", "leavitt", "lichterman", "liskov", "lovelace", "lumiere", "mahavira", "mayer", "mccarthy", "mcclintock", "mclean", "mcnulty", "meitner", "meninsky", "mestorf", "minsky", "mirzakhani", "morse", "murdock", "newton", "nobel", "noether", "northcutt", "noyce", "panini", "pare", "pasteur", "payne", "perlman", "pike", "poincare", "poitras", "ptolemy", "raman", "ramanujan", "ride", "ritchie", "roentgen", "rosalind", "saha", "sammet", "shaw", "shirley", "shockley", "sinoussi", "snyder", "spence", "stallman", "stonebraker", "swanson", "swartz", "swirles", "tesla", "thompson", "torvalds", "turing", "varahamihira", "visvesvaraya", "volhard", "wescoff", "williams", "wilson", "wing", "wozniak", "wright", "yalow", "yonath" };

    public static string NewRandomName => randomAdjectives[UnityEngine.Random.Range(0, randomAdjectives.Length)] + "_" + randomNames[UnityEngine.Random.Range(0, randomNames.Length)];

    public static List<Vector3> getPositionsOnUnitFibbonaciSphere(int numPoints)
    {
        List<Vector3> result = new List<Vector3>();
        //Adapted partially from: https://stackoverflow.com/a/26127012
        float rnd = 1f;
        float offset = 2f / numPoints;
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < numPoints; i++)
        {
            float y = ((i * offset) - 1f) + (offset / 2f);
            float r = Mathf.Sqrt(1f - Mathf.Pow(y, 2f));
            float theta = ((i + rnd) % numPoints) * phi;
            float scale = 1.0f;

            Vector3 point = new Vector3(Mathf.Cos(theta) * r * scale, y * scale, Mathf.Sin(theta) * r * scale) * 0.5f;

            result.Add(point);
        }
        return result;
    }
}

public class FileIOUtils
{
    public static bool DuplicateSafeWriteToAppDataDir(string filename, string extension, string content)
    {
        return DuplicateSafeWriteToFile(GlobalConstants.appDataDirPath, filename, extension, content);
    }

    public static bool DuplicateSafeWriteToFile(string absoluteDirectoryPath, string filename, string extension, string content)
    {
        Directory.CreateDirectory(absoluteDirectoryPath);
        string completeFilePath = Path.Combine(absoluteDirectoryPath, filename + "." + extension);
        string duplicateFreeFilename = filename;
        int ctr = 0;
        while (File.Exists(completeFilePath))
        {
            duplicateFreeFilename = filename + "_v" + ++ctr;
            completeFilePath = Path.Combine(absoluteDirectoryPath, duplicateFreeFilename + "." + extension);
        }

        return WriteToFile(completeFilePath, content);
    }

    public static bool WriteToFile(string absFilePath, string content)
    {
        try
        {
            File.WriteAllText(absFilePath, content);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to {absFilePath} with exception {e}");
            return false;
        }
    }

    public static bool LoadFileFromAppData(string filname, out string result)
    {
        return LoadFromFile(Path.Combine(GlobalConstants.appDataDirPath, filname), out result);
    }

    public static bool LoadFromFile(string absFilePath, out string result)
    {
        try
        {
            Debug.Log("Loading file from: " + absFilePath);
            result = File.ReadAllText(absFilePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read from {absFilePath} with exception {e}");
            result = "";
            return false;
        }
    }

    public static FileInfo[] getJsonFileInfos(string absDirPath)
    {
        try
        {
            return new DirectoryInfo(absDirPath).GetFiles("*.json");
        }
        catch (DirectoryNotFoundException)
        {
            return new FileInfo[0];
        }
    }


    public static T createFromJsonFile<T>(string absFilePath, bool returnDefaultConstructedOnError = true) where T : new()
    {
        string jsonFileContent;
        if (FileIOUtils.LoadFromFile(absFilePath, out jsonFileContent))
        {
            try
            {
                T returnObj = JsonConvert.DeserializeObject<T>(jsonFileContent);
                return returnObj;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during parsing {typeof(T)}: " + e.Message);

                if (returnDefaultConstructedOnError)
                {
                    Debug.Log($"Could not load {typeof(T)} from File {absFilePath}, creating default.");
                    return new T();
                } else
                {
                    Debug.LogWarning($"Could not load {typeof(T)} from File {absFilePath}, returning null.");
                    throw new System.Exception();
                }
            }
        } else
        {
            if (returnDefaultConstructedOnError)
            {
                Debug.Log($"Could not load {typeof(T)} from File {absFilePath}, creating default.");
                return new T();
            }
            else
            {
                Debug.LogWarning($"Could not load {typeof(T)} from File {absFilePath}, returning null.");
                return default(T);
            }
        }
    }
}